namespace MahjongReader
{
    public enum ImportantNodeId {
        // ORDERED FROM TOP OF NODE TREE
        BOARD = 149,

        // UNDER BOARD
        PLAYER_HAND = 128
    }

    public enum MahjongNodeType : ushort {
        PLAYER_TILE = 1055,
    }

    public class PlayerHandNodeIds {
        // 0 index is furthest left, going right
        public static readonly uint[] PLAYER_HAND_TILE_NODE_IDS = {
            134,
            1340001,
            1340002,
            1340003,
            1340004,
            1340005,
            1340006,
            1340007,
            1340008,
            1340009,
            1340010,
            1340011,
            1340012,
        };

        public static readonly uint MOST_RECENT_DRAWN = 135;
    }
}