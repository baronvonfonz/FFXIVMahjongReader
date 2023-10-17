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

            var addon = (AddonEmj*)addonPtr;
            PluginLog.Info("memmers1: " + addonPtr);
            var rootNode = addon->AtkUnitBase.RootNode;
            var prevNode = rootNode->PrevSiblingNode;

            PluginLog.Info("rootNodeId: " + rootNode->NodeID);
            PluginLog.Info("childCount: " + rootNode->ChildCount);
        }

        public unsafe void DumpNode(AtkResNode* node) {
            var nodeType = node->Type;
            PluginLog.Info("NodeID: " + node->NodeID);
            PluginLog.Info("NodeType: " + nodeType);

            if (nodeType == NodeType.Text) {
                var castedTextNode = (AtkTextNode*)node;
                PluginLog.Info(castedTextNode->NodeText.ToString());
                // var stringPtr = castedTextNode->NodeText.StringPtr;

                // if (stringPtr == null) {
                //     return;
                // }

                // string text = Marshal.PtrToStringUTF8(new IntPtr(stringPtr));
                // PluginLog.Info(text);
            } else if (nodeType == NodeType.Image) {
                var castedImageNode = (AtkImageNode*)node;
                int partId = castedImageNode->PartId;
                var partsList = castedImageNode->PartsList;
                var partsCount = partsList->PartCount;

                if (partId > partsCount) {
                    PluginLog.Info("Parts count bad buhh?");
                    return;
                }

                PluginLog.Info("Part ID: " + partId + " count: " + partsCount);


                var uldAsset = partsList->Parts[partId].UldAsset;
                if (uldAsset->AtkTexture.TextureType != TextureType.Resource) {
                    PluginLog.Info("Bad texture part");
                    return;
                }
                var texFileNameStdString = &uldAsset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                var texString = texFileNameStdString->Length < 16
                                    ? Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->Buffer)
                                    : Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->BufferPtr);
                PluginLog.Info("length " + texFileNameStdString->Length + " texString " + texString);
                if (texString != null) {
                    var maybeTileTex = TileTextureMap.Instance.GetTileTextureFromTexturePath(texString);
                    PluginLog.Info("maybeTileTex: " + maybeTileTex?.ToString() ?? "oof whiffers");
                }
            } else if ((ushort)nodeType > 999) {
                DumpComponentNode(node);
            }
        }

        public unsafe void TraverseAllAtkResNodes(AtkResNode* node, Action<IntPtr>? nodeHandlerFn) {
            if (node == null) {
                return;
            }

            var childPtr = node->ChildNode;

            while (childPtr != null) {
                // if (childPtr->NodeID == 4 && childPtr->Type == NodeType.Image) {
                //     ImportantPointers.TestTime = (nint)childPtr;
                // }
                // maybe add to list?
                // DumpNode(childPtr);
                nodeHandlerFn?.Invoke((nint)childPtr);
                childPtr = childPtr->PrevSiblingNode;
                TraverseAllAtkResNodes(childPtr, nodeHandlerFn);
            }
        }

        private unsafe void DumpComponentNode(AtkResNode* node) {
            var compNode = (AtkComponentNode*)node;
            var componentInfo = compNode->Component->UldManager;
            var childCount = componentInfo.NodeListCount;

            var objectInfo = (AtkUldComponentInfo*)componentInfo.Objects;
            if (objectInfo == null)
            {
                return;
            }
            for (var i = 0; i < childCount; i++) {
                DumpNode(compNode->Component->UldManager.NodeList[i]);
            }
        }

        private unsafe void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            // MainWindow.IsOpen = true;
            var addonPtr = GameGui.GetAddonByName("Emj", 1);

            if (addonPtr == IntPtr.Zero) {
                PluginLog.Info("Could not find Emj");
                return;
            }
            
            PluginLog.Info("Found Emj");
            var addon = (AddonEmj*)addonPtr;
            PluginLog.Info("memmers1: " + addonPtr);
            var rootNode = addon->AtkUnitBase.RootNode;
            PluginLog.Info("rootNode: " + rootNode->NodeID);
            TraverseAllAtkResNodes(rootNode, (intPtr) => ImportantPointers.MaybeTrackPointer(intPtr));
            // var workPls = (AtkResNode*)ImportantPointers.TestTime;
            
            // DumpNode(workPls);
            PluginLog.Info("Hand size: " + ImportantPointers.PlayerHand.Count);
            ImportantPointers.PlayerHand.ForEach(ptr => {
                PluginLog.Info("im loopin");
                DumpNode((AtkResNode*)ptr);
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
