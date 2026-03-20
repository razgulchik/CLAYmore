using UnityEngine;

namespace CLAYmore.ECS
{
    public struct PlayerTileChangedEvent
    {
        public Vector2Int OldIndex;  // (int.MinValue, int.MinValue) = no previous tile
        public Vector2Int NewIndex;  // absolute tilemap coord
    }
}
