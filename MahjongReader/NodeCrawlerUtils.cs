using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MahjongReader
{
    public unsafe class NodeCrawlerUtils {
        private IPluginLog PluginLog { get; init; }

        public NodeCrawlerUtils(IPluginLog pluginLog) {
            PluginLog = pluginLog;
        }

        public unsafe TileTexture? GetTileTextureFromComponentNode(IntPtr nodePtr) {
            var compNode = (AtkComponentNode*)nodePtr;
            var componentInfo = compNode->Component->UldManager;

            // button is always first node here
            var buttonNode = compNode->Component->UldManager.NodeList[0];
            var tileImageNode = buttonNode->GetComponent()->UldManager.NodeList[4];
            var texString = GetImageTextString((AtkImageNode*)tileImageNode);

            return texString != null ? TileTextureMap.Instance.GetTileTextureFromTexturePath(texString) : null;
        }

        public unsafe string? GetImageTextString(AtkImageNode* imageNode) {
                int partId = imageNode->PartId;
                var partsList = imageNode->PartsList;
                var partsCount = partsList->PartCount;
                var uldAsset = partsList->Parts[partId].UldAsset;
                var texFileNameStdString = &uldAsset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                return texFileNameStdString->Length < 16
                    ? Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->Buffer)
                    : Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->BufferPtr);
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
                PluginLog.Info("nodeId: " + compNode->AtkResNode.NodeID + " child: " + i);
                DelveNode(compNode->Component->UldManager.NodeList[i], nodeHandlerFn);
            }
        }
    }
}