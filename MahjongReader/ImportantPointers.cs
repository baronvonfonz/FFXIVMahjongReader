using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MahjongReader
{
    public unsafe class ImportantPointers {
        private IntPtr topLevelBoard = IntPtr.Zero;
        private List<IntPtr> playerHand = new List<IntPtr>();

        public IntPtr TopLevelBoard { get; }
        public List<IntPtr> PlayerHand {
            get
            {
                return playerHand;
            }
        }

        public void WipePointers() {
            topLevelBoard = IntPtr.Zero;
            playerHand = new List<IntPtr>();
        }

        public void MaybeTrackPointer(IntPtr rawPtr) {
            var node = (AtkResNode*)rawPtr;
            if ((ushort)node->Type == (ushort)MahjongNodeType.PLAYER_TILE) {
                var nodeId = node->NodeID;
                if (PlayerHandNodeIds.MOST_RECENT_DRAWN == nodeId || PlayerHandNodeIds.PLAYER_HAND_TILE_NODE_IDS.Contains(nodeId)) {
                    playerHand.Add((IntPtr)node);
                }
            }
        }
    }
}