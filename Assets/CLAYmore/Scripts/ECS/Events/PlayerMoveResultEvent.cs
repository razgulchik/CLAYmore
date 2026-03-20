using UnityEngine;

namespace CLAYmore
{
    public struct PlayerMoveResultEvent
    {
        public Vector2Int Direction;
        public Vector3    Target;    // мировая позиция цели (Walk / Bounce)
        public MoveType   MoveType;
    }
}
