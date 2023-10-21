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
            if (!compNode->AtkResNode.IsVisible) { // previous games have textures lingering in the pointer but not visible
                return null;
            }

            // button is always first node here
            var buttonNode = compNode->Component->UldManager.NodeList[0];
            var tileImageNode = buttonNode->GetComponent()->UldManager.NodeList[4];
            var texString = GetImageTexturePath((AtkImageNode*)tileImageNode);

            return texString != null ? TileTextureMap.Instance.GetTileTextureFromTexturePath(texString) : null;
        }

        public unsafe DiscardTile? GetTileTextureFromDiscardTile(IntPtr nodePtr) {
            var compNode = (AtkComponentNode*)nodePtr;
            if (!compNode->AtkResNode.IsVisible) { // previous games have textures lingering in the pointer but not visible
                return null;
            }

            // no button wrapper, fourth node is always the image
            var imageNode = compNode->Component->UldManager.NodeList[3];
            var texString = GetImageTexturePath((AtkImageNode*)imageNode);

            var tileTexture = texString != null ? TileTextureMap.Instance.GetTileTextureFromTexturePath(texString) : null;
            if (tileTexture == null) {
                return null;
            }

            var resNodeForMeldedInfo = compNode->Component->UldManager.NodeList[1];
            var addRedInt = resNodeForMeldedInfo->AddRed;
            var addGreenInt = resNodeForMeldedInfo->AddGreen;
            // var addBlueInt = resNodeForMeldedInfo->AddBlue; // I think green and blue get same scale in these cases

            bool isMelded = false;
            bool isImmediatelyDiscarded = false;

            if (addGreenInt < 0 && ((addGreenInt + addRedInt) != addGreenInt * 2)) {
                isMelded = true;
            }

            if (addGreenInt < 0 && addGreenInt == addRedInt) {
                isImmediatelyDiscarded = true;
            }

            return new DiscardTile(tileTexture, isMelded, isImmediatelyDiscarded);
        }

        public unsafe TileTexture? GetTileTextureFromMeldTile(IntPtr nodePtr) {
            var compNode = (AtkComponentNode*)nodePtr;
            if (!compNode->AtkResNode.IsVisible) { // previous games have textures lingering in the pointer but not visible
                return null;
            }

            // three nodes, third is the image
            var tileImageNode = compNode->Component->UldManager.NodeList[2];
            if (!tileImageNode->IsVisible) { // previous games have textures lingering in the pointer but not visible
                return null;
            }
            var texString = GetImageTexturePath((AtkImageNode*)tileImageNode);

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
                // PluginLog.Info($"texture not loaded for node ID: {imageNode->AtkResNode.NodeID} ptr = ptr = {(long)imageNode:X}");
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