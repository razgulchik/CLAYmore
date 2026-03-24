using UnityEngine;

namespace CLAYmore
{
    public struct PlayerMoveResultEvent
    {
        public Vector2Int Direction;
        public Vector3    Target;       // мировая позиция цели (Walk / Bounce)
        public Vector3    SlideTarget;  // если задан — точка возврата при Bounce вместо transform.position
        public MoveType   MoveType;
    }
}
