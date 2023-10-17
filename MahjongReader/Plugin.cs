using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MahjongReader.Windows;
using System;
using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

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
            ImportantPointers = new ImportantPointers();
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

        private void OnAddonPostSetup(AddonEvent type, AddonArgs args) {
            AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "Emj", OnAddonPostRefresh);
        }

        private unsafe void OnAddonPostRefresh(AddonEvent type, AddonArgs args) {
            var addonPtr = args.Addon;
            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }

            var addon = (AtkUnitBase*)addonPtr;
            var rootNode = addon->RootNode;
            var prevNode = rootNode->PrevSiblingNode;
        }

        public unsafe void DelveNode(AtkResNode* node, Action<IntPtr>? nodeHandlerFn) {
            var nodeType = node->Type;
#if DEBUG
            PluginLog.Info("NodeID: " + node->NodeID);
            PluginLog.Info("NodeType: " + nodeType);
#endif

            if (nodeType == NodeType.Text) {
                var castedTextNode = (AtkTextNode*)node;
#if DEBUG
                PluginLog.Info(castedTextNode->NodeText.ToString());
#endif
            } else if (nodeType == NodeType.Image) {
                var castedImageNode = (AtkImageNode*)node;
                int partId = castedImageNode->PartId;
                var partsList = castedImageNode->PartsList;
                var partsCount = partsList->PartCount;

                if (partId > partsCount) {
#if DEBUG
                    PluginLog.Info("Bad parts count for node: " + node->NodeID + " partID: " + partId);
#endif
                    return;
                }

                var uldAsset = partsList->Parts[partId].UldAsset;
                if (uldAsset->AtkTexture.TextureType != TextureType.Resource) {
#if DEBUG
                    PluginLog.Info("Bad texture type for node: " + node->NodeID + " partID: " + partId);
#endif
                    return;
                }
                var texFileNameStdString = &uldAsset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                var texString = texFileNameStdString->Length < 16
                                    ? Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->Buffer)
                                    : Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->BufferPtr);
#if DEBUG
                PluginLog.Info("length " + texFileNameStdString->Length + " texString " + texString);
#endif
                if (texString != null) {
                    var maybeTileTex = TileTextureMap.Instance.GetTileTextureFromTexturePath(texString);
#if DEBUG
                    PluginLog.Info("maybeTileTex: " + maybeTileTex?.ToString() ?? "whiffed?");
#endif
                }
            } else if ((ushort)nodeType > 999) {
                DelveComponentNode(node, nodeHandlerFn);
            }
        }

        public unsafe void TraverseAllAtkResNodes(AtkResNode* node, Action<IntPtr>? nodeHandlerFn) {
            if (node == null) {
                return;
            }

#if DEBUG
            PluginLog.Info("START TOP OF A NODE TREE");
#endif

            var childPtr = node->ChildNode;

            while (childPtr != null) {
                DelveNode(childPtr, nodeHandlerFn);
                nodeHandlerFn?.Invoke((nint)childPtr);
                childPtr = childPtr->PrevSiblingNode;
                TraverseAllAtkResNodes(childPtr, nodeHandlerFn);
            }
#if DEBUG
            PluginLog.Info("END NODE TREE");
#endif
        }

        private unsafe void DelveComponentNode(AtkResNode* node, Action<IntPtr>? nodeHandlerFn) {
            var compNode = (AtkComponentNode*)node;
            var componentInfo = compNode->Component->UldManager;
            var childCount = componentInfo.NodeListCount;

            var objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo == null)
            {
                return;
            }
            for (var i = 0; i < childCount; i++) {
                DelveNode(compNode->Component->UldManager.NodeList[i], nodeHandlerFn);
            }
        }

        private unsafe void OnCommand(string command, string args)
        {
            var addonPtr = GameGui.GetAddonByName("Emj", 1);

            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
            // TileTextureMap.PrintTest(PluginLog);
            PluginLog.Info("Found Emj");
            ImportantPointers.WipePointers();
            var addon = (AtkUnitBase*)addonPtr;
            var rootNode = addon->RootNode;
            TraverseAllAtkResNodes(rootNode, (intPtr) => ImportantPointers.MaybeTrackPointer(intPtr));
            
            PluginLog.Info("Hand size: " + ImportantPointers.PlayerHand.Count);
            ImportantPointers.PlayerHand.ForEach(ptr => {
                var castedPtr = (AtkResNode*)ptr;
                PluginLog.Info("Hand: " + castedPtr->NodeID);
                // DelveNode(, (_) => {});
            });
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
