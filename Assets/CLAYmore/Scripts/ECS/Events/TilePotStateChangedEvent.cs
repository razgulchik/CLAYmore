using UnityEngine;

namespace CLAYmore.ECS
{
    public struct TilePotStateChangedEvent
    {
        public Vector2Int Index;    // absolute tilemap coord
        public CellState  OldState;
        public CellState  NewState;
    }
}
