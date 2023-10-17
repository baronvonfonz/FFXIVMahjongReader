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
}