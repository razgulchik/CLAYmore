namespace CLAYmore
{
    public enum IslandTileType
    {
        // Border tiles — edges of the island (9)
        LeftBot,        // left side, bottom corner
        LeftMid,        // left side, middle
        LeftTop,        // left side, top corner

        BotLeft,        // bottom row, leftmost
        BotCenter,      // bottom row, center
        BotRight,       // bottom row, rightmost

        RightBot,       // right side, bottom corner
        RightMid,       // right side, middle
        RightTop,       // right side, top corner

        // Interior tiles — ground of the island (7)
        InteriorBotLeftCorner,   // 2nd row from bottom, leftmost interior
        InteriorBotCenter,       // 2nd row from bottom, center
        InteriorBotRightCorner,  // 2nd row from bottom, rightmost interior

        CenterInner,             // all remaining interior cells

        InteriorTopLeftCorner,   // top row, leftmost interior (top edge)
        InteriorTopCenter,       // top row, center (top edge)
        InteriorTopRightCorner,  // top row, rightmost interior (top edge)
    }
}
