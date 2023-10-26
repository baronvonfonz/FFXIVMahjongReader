using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using GameModel;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using MahjongReader.Windows;
using Dalamud.Interface.Utility;

namespace MahjongReader
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Sample Plugin";
        private const string CommandName = "/mahjong";


        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("MahjongReader");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        private ImportantPointers ImportantPointers { get; init; }
        private NodeCrawlerUtils NodeCrawlerUtils { get; init; }
        private Task WindowUpdateTask { get; set; } = null!;
        private YakuDetector YakuDetector { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, PluginLog);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Tracks observed Mahjong tiles and available Yaku (TODO)"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Emj", OnAddonPostSetup);
            ImportantPointers = new ImportantPointers(PluginLog);
            NodeCrawlerUtils = new NodeCrawlerUtils(PluginLog);
            YakuDetector = new YakuDetector();
        }

        public void Dispose()
        {
            AddonLifecycle.UnregisterListener(OnAddonPostSetup);
            AddonLifecycle.UnregisterListener(OnAddonPostRefresh);
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private unsafe void OnAddonPostSetup(AddonEvent type, AddonArgs args) {
            var addonPtr = args.Addon;
            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
            AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "Emj", OnAddonPostRefresh);
            AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "Emj", OnAddonPreFinalize);

            MainWindow.IsOpen = true;
            var addon = (AtkUnitBase*)addonPtr;
            var rootNode = addon->RootNode;
            ImportantPointers.WipePointers();
            NodeCrawlerUtils.TraverseAllAtkResNodes(rootNode, (intPtr) => ImportantPointers.MaybeTrackPointer(intPtr));
        }

        private void OnAddonPreFinalize(AddonEvent type, AddonArgs args) {
            MainWindow.IsOpen = false;
        }

        private void OnAddonPostRefresh(AddonEvent type, AddonArgs args) {
            var addonPtr = args.Addon;
            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }

            if (WindowUpdateTask == null || WindowUpdateTask.IsCompleted || WindowUpdateTask.IsFaulted || WindowUpdateTask.IsCanceled) {
                PluginLog.Info("Running window updater");
                WindowUpdateTask = Task.Run(WindowUpdater);
            }
        }

        private unsafe void WindowUpdater() {
#if DEBUG
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
#endif

            var observedTiles = GetObservedTiles();
            PluginLog.Info($"tiles count: {observedTiles.Count}");
            var remainingMap = TileTextureUtilities.TileCountTracker.RemainingFromObserved(observedTiles);
            MainWindow.ObservedTiles = observedTiles;
            MainWindow.RemainingMap = remainingMap;
#if DEBUG
    stopwatch.Stop();
    TimeSpan elapsedTime = stopwatch.Elapsed;
    PluginLog.Info($"QQQQQQQ - Elapsed time: {elapsedTime.TotalMilliseconds} ms");
#endif
        }

        private unsafe void OnCommand(string command, string args)
        {
            var addonPtr = GameGui.GetAddonByName("Emj", 1);

            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
        }

        private unsafe List<ObservedTile> GetObservedDiscardTiles(List<IntPtr> ptrs, MahjongNodeType playerArea) {
            var observedTileTextures = new List<ObservedTile>(); 
            ptrs.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTexture = NodeCrawlerUtils.GetTileTextureFromDiscardTile(ptr);
                if (tileTexture != null) {
                    if (!tileTexture.IsMelded) {
                        observedTileTextures.Add(new ObservedTile(playerArea, tileTexture.TileTexture));
                    }
                }
            });
            return observedTileTextures;
        }

        private unsafe List<ObservedTile> GetObservedMeldTiles(List<IntPtr> ptrs, MahjongNodeType playerArea) {
            var observedTileTextures = new List<ObservedTile>();
            ptrs.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTextures = NodeCrawlerUtils.GetTileTexturesFromMeldGroup(ptr);
                tileTextures?.ForEach(texture => observedTileTextures.Add(new ObservedTile(playerArea, texture)));
            });
            return observedTileTextures;
        }

        public unsafe List<ObservedTile> GetObservedTiles() {
            var observedTileTextures = new List<ObservedTile>();
            ImportantPointers.PlayerHand.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTexture = NodeCrawlerUtils.GetTileTextureFromPlayerHandTile(ptr);
                if (tileTexture != null) {
                    observedTileTextures.Add(new ObservedTile(MahjongNodeType.PLAYER_HAND_TILE, tileTexture));
                }
            });

            // Discarded tiles have their own node tree shape
            observedTileTextures.AddRange(GetObservedDiscardTiles(ImportantPointers.PlayerDiscardPile, MahjongNodeType.PLAYER_DISCARD_TILE));
            observedTileTextures.AddRange(GetObservedDiscardTiles(ImportantPointers.RightDiscardPile, MahjongNodeType.RIGHT_DISCARD_TILE));
            observedTileTextures.AddRange(GetObservedDiscardTiles(ImportantPointers.FarDiscardPile, MahjongNodeType.FAR_DISCARD_TILE));
            observedTileTextures.AddRange(GetObservedDiscardTiles(ImportantPointers.LeftDiscardPile, MahjongNodeType.LEFT_DISCARD_TILE));

            // Player melds have their own shape
            ImportantPointers.PlayerMeldGroups.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTextures = NodeCrawlerUtils.GetTileTexturesFromPlayerMeldGroup(ptr);
                tileTextures?.ForEach(texture => observedTileTextures.Add(new ObservedTile(MahjongNodeType.PLAYER_MELD_GROUP, texture)));
            });

            // Melds that are not your own have a different node tree shape
            observedTileTextures.AddRange(GetObservedMeldTiles(ImportantPointers.RightMeldGroups, MahjongNodeType.RIGHT_MELD_GROUP));
            observedTileTextures.AddRange(GetObservedMeldTiles(ImportantPointers.FarMeldGroups, MahjongNodeType.FAR_MELD_GROUP));
            observedTileTextures.AddRange(GetObservedMeldTiles(ImportantPointers.LeftMeldGroups, MahjongNodeType.LEFT_MELD_GROUP));
            
            return observedTileTextures;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
