using UnityEngine;

namespace CLAYmore.ECS
{
    public struct PlayerMoveHeldEvent
    {
        public Vector2Int Direction; // zero when released
    }
}
