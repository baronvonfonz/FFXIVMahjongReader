using System;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using MahjongReader.Windows;
using System.Collections.Generic;
using System.Linq;

namespace MahjongReader
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Sample Plugin";
        private const string CommandName = "/mahjong";


        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("MahjongReader");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        private ImportantPointers ImportantPointers { get; init; }
        private NodeCrawlerUtils NodeCrawlerUtils { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, goatImage);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "Emj", OnAddonPostSetup);
            ImportantPointers = new ImportantPointers(PluginLog);
            NodeCrawlerUtils = new NodeCrawlerUtils(PluginLog);
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
            AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "Emj", OnAddonPostRefresh);
            var addonPtr = args.Addon;
            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
            var addon = (AtkUnitBase*)addonPtr;
            var rootNode = addon->RootNode;
            ImportantPointers.WipePointers();
            NodeCrawlerUtils.TraverseAllAtkResNodes(rootNode, (intPtr) => ImportantPointers.MaybeTrackPointer(intPtr));
        }

        private unsafe void OnAddonPostRefresh(AddonEvent type, AddonArgs args) {
            var addonPtr = args.Addon;
            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }

            var observedTiles = GetObservedTiles();
            PluginLog.Info($"tiles count: {observedTiles.Count}");
            observedTiles.ForEach(tile => PluginLog.Info(tile.ToString()));
        }

        private unsafe void OnCommand(string command, string args)
        {
            var addonPtr = GameGui.GetAddonByName("Emj", 1);

            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
        }

        private unsafe List<TileTexture> GetObservedTiles() {
            var observedTileTextures = new List<TileTexture>();
            ImportantPointers.PlayerHand.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTexture = NodeCrawlerUtils.GetTileTextureFromPlayerHandTile(ptr);
                if (tileTexture != null) {
                    observedTileTextures.Add(tileTexture);
                }
            });
        
            // Discarded tiles have their own node tree shape
            ImportantPointers.PlayerDiscardPile
                .Concat(ImportantPointers.RightDiscardPile)
                .Concat(ImportantPointers.FarDiscardPile)
                .Concat(ImportantPointers.LeftDiscardPile)
                .ToList()
                .ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTexture = NodeCrawlerUtils.GetTileTextureFromDiscardTile(ptr);
                if (tileTexture != null) {
                    if (!tileTexture.IsMelded) {
                        observedTileTextures.Add(tileTexture.TileTexture);
                    }
                }
            });

            // Player melds have their own shape
            ImportantPointers.PlayerMeldGroups.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTextures = NodeCrawlerUtils.GetTileTexturesFromPlayerMeldGroup(ptr);
                tileTextures?.ForEach(texture => observedTileTextures.Add(texture));
            });

            // Melds that are not your own have a different node tree shape
            ImportantPointers.RightMeldGroups
                .Concat(ImportantPointers.FarMeldGroups)
                .Concat(ImportantPointers.LeftMeldGroups)
                .ToList()
                .ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                var tileTextures = NodeCrawlerUtils.GetTileTexturesFromMeldGroup(ptr);
                tileTextures?.ForEach(texture => observedTileTextures.Add(texture));
            });
            
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
