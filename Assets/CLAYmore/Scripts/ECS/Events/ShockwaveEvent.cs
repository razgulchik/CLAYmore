using UnityEngine;

namespace CLAYmore
{
    public struct ShockwaveEvent
    {
        public Vector3[]    TilePositions;
        public bool[]       HadPot;
        public Vector2Int   Direction;
        public Vector2Int[] Cells;
    }

    public struct ShockwaveCellImpactEvent
    {
        public Vector2Int Cell;
        public Vector3    WorldPosition;
    }
}
