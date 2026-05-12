using UnityEngine;

namespace CLAYmore
{
    public struct BallLightningSpawnedEvent
    {
        public Vector2Int Cell;
        public Vector3    WorldPosition;
    }

    public struct BallLightningExplodedEvent
    {
        public Vector2Int Cell;
        public Vector3    WorldPosition;
    }

    // Published by BallLightningVFXController when lifetime expires (if lifetime > 0)
    public struct BallLightningExpiredEvent
    {
        public Vector2Int Cell;
    }
}
