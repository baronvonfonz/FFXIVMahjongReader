using System;
using System.Collections.Generic;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MahjongReader
{
    public unsafe class ImportantPointers {
        private IntPtr topLevelBoard = IntPtr.Zero;
        private List<IntPtr> playerHand = new List<nint>();

        public IntPtr TopLevelBoard { get; }
        public List<IntPtr> PlayerHand {
            get
            {
                return playerHand;
            }
        }

        public void MaybeTrackPointer(IntPtr rawPtr) {
            var node = (AtkResNode*)rawPtr;
            if ((ushort)node->Type == (ushort)MahjongNodeType.PLAYER_TILE) {
                playerHand.Add((nint)node);
            }
        }
    }
}