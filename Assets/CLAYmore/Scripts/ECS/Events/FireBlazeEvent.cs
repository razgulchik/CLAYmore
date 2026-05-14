using UnityEngine;

namespace CLAYmore
{
    public struct FireBlazeActivatedEvent
    {
        public Vector3    Origin;
        public Vector2Int DiagDir1;
        public Vector2Int DiagDir2;
        public Vector2Int Cell1;
        public Vector2Int Cell2;
    }

    public struct FireBlazeImpactEvent
    {
        public Vector2Int Cell;
    }
}
