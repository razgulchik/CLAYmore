using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class BallLightningDetonateBridge : MonoBehaviour
    {
        [HideInInspector] public Vector2Int Cell;
        [HideInInspector] public Vector3    WorldPosition;

        // Called by Animation Event
        public void OnDetonate()
            => World.Current?.Events.Publish(new BallLightningDetonateEvent { Cell = Cell, WorldPosition = WorldPosition });
    }
}
