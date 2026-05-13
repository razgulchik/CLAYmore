using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class ShockwaveCellBridge : MonoBehaviour
    {
        [HideInInspector] public Vector2Int Cell;
        [HideInInspector] public Vector3    WorldPosition;

        // Вызывается Animation Event
        public void OnImpact()
            => World.Current?.Events.Publish(new ShockwaveCellImpactEvent { Cell = Cell, WorldPosition = WorldPosition });
    }
}
