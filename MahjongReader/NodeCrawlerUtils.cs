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

        public unsafe TileTexture? GetTileTextureFromPlayerHandTile(IntPtr nodePtr) {
            var compNode = (AtkComponentNode*)nodePtr;

            // button is always first node here
            var buttonNode = compNode->Component->UldManager.NodeList[0];
            var tileImageNode = buttonNode->GetComponent()->UldManager.NodeList[4];
            var texString = GetImageTexturePath((AtkImageNode*)tileImageNode);

            return texString != null ? TileTextureMap.Instance.GetTileTextureFromTexturePath(texString) : null;
        }

        public unsafe TileTexture? GetTileTextureFromDiscardTile(IntPtr nodePtr) {
            var compNode = (AtkComponentNode*)nodePtr;

            var count = compNode->Component->UldManager.NodeListCount;

            // no button wrapper, fourth node is always the image
            var imageNode = compNode->Component->UldManager.NodeList[3];
            var texString = GetImageTexturePath((AtkImageNode*)imageNode);

            return texString != null ? TileTextureMap.Instance.GetTileTextureFromTexturePath(texString) : null;
        }

        public unsafe string? GetImageTexturePath(AtkImageNode* imageNode) {
            int partId = imageNode->PartId;

            var partsList = imageNode->PartsList;
            var partsCount = partsList->PartCount;
            var uldAsset = partsList->Parts[partId].UldAsset;
            var texType = uldAsset->AtkTexture.TextureType;

            if (texType == TextureType.Resource) {
                var texFileNameStdString = &uldAsset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                var texturePath = texFileNameStdString->Length < 16
                    ? Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->Buffer)
                    : Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->BufferPtr);

                if (texturePath == null) {
                    throw new ApplicationException("texturePath was null for nodeId: " + imageNode->AtkResNode.NodeID);
                }

                return texturePath;
            } else {
                PluginLog.Info($"texture not loaded for node ID: {imageNode->AtkResNode.NodeID} ptr = ptr = {(long)imageNode:X}");
                return null;
            }
        }

        public unsafe void DelveNode(AtkResNode* node, Action<IntPtr>? nodeHandlerFn, bool log = false) {
            var nodeType = node->Type;

            if (log) {
                PluginLog.Info($"nodeId: {node->NodeID} nodeType: {nodeType}");
            }

            if (nodeType == NodeType.Text) {
                var castedTextNode = (AtkTextNode*)node;
            } else if (nodeType == NodeType.Image) {
                var castedImageNode = (AtkImageNode*)node;
                int partId = castedImageNode->PartId;
                var partsList = castedImageNode->PartsList;
                var partsCount = partsList->PartCount;

                if (partId > partsCount) {
                    if (log) {
                        PluginLog.Info($"partId > partsCount, partId: {partId} partsCount: {partsCount}");
                    }
                    return;
                }

                var uldAsset = partsList->Parts[partId].UldAsset;
                if (uldAsset->AtkTexture.TextureType != TextureType.Resource) {
                    if (log) {
                        PluginLog.Info($"textureType not Resource: " + uldAsset->AtkTexture.TextureType);
                    }
                    return;
                }
                var texFileNameStdString = &uldAsset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName;
                var texString = texFileNameStdString->Length < 16
                    ? Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->Buffer)
                    : Marshal.PtrToStringAnsi((IntPtr)texFileNameStdString->BufferPtr);
                if (log) {
                    PluginLog.Info($"texString {texString}");
                }                    
            } else if ((ushort)nodeType > 999) {
                DelveComponentNode(node, nodeHandlerFn);
            }
        }

        public unsafe void TraverseAllAtkResNodes(AtkResNode* node, Action<IntPtr>? nodeHandlerFn, bool log = false) {
            if (node == null) {
                if (log) {
                    PluginLog.Info("null return from TraverseAllAtkResNodes");
                }
                return;
            }

            var childPtr = node->ChildNode;

            if (childPtr == null) {
                if (log) {
                    PluginLog.Info("null childPtr return from TraverseAllAtkResNodes");
                }
                return;
            }

            while (childPtr != null) {
                DelveNode(childPtr, nodeHandlerFn, log);
                nodeHandlerFn?.Invoke((nint)childPtr);
                childPtr = childPtr->PrevSiblingNode;
                TraverseAllAtkResNodes(childPtr, nodeHandlerFn, log);
            }
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
    }
}