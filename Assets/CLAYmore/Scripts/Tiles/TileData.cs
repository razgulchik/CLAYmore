using UnityEngine;

namespace CLAYmore
{
    public enum CellState
    {
        Empty,
        PotInFlight,  // reserved — pot spawned but not yet landed
        HasPot,       // pot has landed and is sitting on the cell
    }

    public class TileData
    {
        public Vector2Int Index     { get; }   // absolute tilemap cell coordinate
        public CellState  State     { get; set; } = CellState.Empty;
        public bool       HasPlayer { get; set; }

        public TileData(Vector2Int index) => Index = index;
    }
}
