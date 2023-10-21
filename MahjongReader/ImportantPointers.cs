using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MahjongReader
{
    public unsafe class ImportantPointers {
        private IntPtr topLevelBoard = IntPtr.Zero;
        private List<IntPtr> playerHand = new List<IntPtr>();
        private List<IntPtr> playerDiscardPile = new List<IntPtr>();
        private List<IntPtr> rightDiscardPile = new List<IntPtr>();
        private List<IntPtr> farDiscardPile = new List<IntPtr>();
        private List<IntPtr> leftDiscardPile = new List<IntPtr>();

        private List<IntPtr> rightMelds = new List<IntPtr>();
        private List<IntPtr> farMelds = new List<IntPtr>();
        private List<IntPtr> leftMelds = new List<IntPtr>();

        public IntPtr TopLevelBoard { get; }
        public List<IntPtr> PlayerHand {
            get
            {
                return playerHand;
            }
        }

        public List<IntPtr> PlayerDiscardPile {
            get
            {
                return playerDiscardPile;
            }
        }

        public List<IntPtr> RightDiscardPile {
            get
            {
                return rightDiscardPile;
            }
        }

        public List<IntPtr> FarDiscardPile {
            get
            {
                return farDiscardPile;
            }
        }        

        public List<IntPtr> LeftDiscardPile {
            get
            {
                return leftDiscardPile;
            }
        }

        public List<IntPtr> RightMelds {
            get
            {
                return rightMelds;
            }
        }

        public List<IntPtr> FarMelds {
            get
            {
                return farMelds;
            }
        }        

        public List<IntPtr> LeftMelds {
            get
            {
                return leftMelds;
            }
        }
        private IPluginLog PluginLog { get; init; }

        public ImportantPointers(IPluginLog pluginLog) {
            PluginLog = pluginLog;
        }

        public void WipePointers() {
            topLevelBoard = IntPtr.Zero;
            playerHand = new List<IntPtr>();

            playerDiscardPile = new List<IntPtr>();
            rightDiscardPile = new List<IntPtr>();
            farDiscardPile = new List<IntPtr>();
            leftDiscardPile = new List<IntPtr>();

            rightMelds = new List<IntPtr>();
            farMelds = new List<IntPtr>();
            leftMelds = new List<IntPtr>();
        }

        public void MaybeTrackPointer(IntPtr rawPtr) {
            var node = (AtkResNode*)rawPtr;
            var nodeTypeUShort = (ushort)node->Type;
            if (nodeTypeUShort == (ushort)MahjongNodeType.PLAYER_HAND_TILE) {
                var nodeId = node->NodeID;
                if (PlayerHandNodeIds.MOST_RECENT_DRAWN == nodeId || PlayerHandNodeIds.PLAYER_HAND_TILE_NODE_IDS.Contains(nodeId)) {
                    playerHand.Add(rawPtr);
                }
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.PLAYER_DISCARD_TILE) {
                playerDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.RIGHT_DISCARD_TILE) {
                rightDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.LEFT_DISCARD_TILE) {
                leftDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.FAR_DISCARD_TILE) {
                farDiscardPile.Add(rawPtr);
            }
        }
    }
}