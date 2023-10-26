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

        private List<IntPtr> playerMeldGroups = new List<IntPtr>();
        private List<IntPtr> rightMeldGroups = new List<IntPtr>();
        private List<IntPtr> farMeldGroups = new List<IntPtr>();
        private List<IntPtr> leftMeldGroups = new List<IntPtr>();

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

        public List<IntPtr> PlayerMeldGroups {
            get
            {
                return playerMeldGroups;
            }
        }

        public List<IntPtr> LeftDiscardPile {
            get
            {
                return leftDiscardPile;
            }
        }

        public List<IntPtr> RightMeldGroups {
            get
            {
                return rightMeldGroups;
            }
        }

        public List<IntPtr> FarMeldGroups {
            get
            {
                return farMeldGroups;
            }
        }        

        public List<IntPtr> LeftMeldGroups {
            get
            {
                return leftMeldGroups;
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

            playerMeldGroups = new List<IntPtr>();
            rightMeldGroups = new List<IntPtr>();
            farMeldGroups = new List<IntPtr>();
            leftMeldGroups = new List<IntPtr>();
        }

        public void MaybeTrackPointer(IntPtr rawPtr) {
            var node = (AtkResNode*)rawPtr;
            var nodeTypeUShort = (ushort)node->Type;
            if (nodeTypeUShort == (ushort)MahjongNodeType.PLAYER_HAND_TILE) {
                var nodeId = node->NodeID;
                if (PlayerHandNodeIds.MOST_RECENT_DRAWN == nodeId || PlayerHandNodeIds.PLAYER_HAND_TILE_NODE_IDS.Contains(nodeId)) {
                    playerHand.Add(rawPtr);
                }
            // discards
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.PLAYER_DISCARD_TILE) {
                playerDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.RIGHT_DISCARD_TILE) {
                rightDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.LEFT_DISCARD_TILE) {
                leftDiscardPile.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.FAR_DISCARD_TILE) {
                farDiscardPile.Add(rawPtr);
            // melds
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.PLAYER_MELD_GROUP) {
                playerMeldGroups.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.RIGHT_MELD_GROUP) {
                rightMeldGroups.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.LEFT_MELD_GROUP) {
                leftMeldGroups.Add(rawPtr);
            } else if (nodeTypeUShort == (ushort)MahjongNodeType.FAR_MELD_GROUP) {
                farMeldGroups.Add(rawPtr);
            }
        }
    }
}