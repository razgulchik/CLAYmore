using UnityEngine;

namespace CLAYmore
{
    public struct WhirlwindActivatedEvent
    {
        public Vector2Int Cell;
        public Vector3    WorldPosition;
    }

    public struct WhirlwindDetonateEvent
    {
        public Vector2Int Cell;
    }
}
