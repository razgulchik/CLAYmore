using UnityEngine;

namespace CLAYmore
{
    public struct PlayerLandedEvent
    {
        public Vector2Int Cell;
        public Vector3    WorldPosition;
        public Vector2Int FacingDirection;
    }
}
