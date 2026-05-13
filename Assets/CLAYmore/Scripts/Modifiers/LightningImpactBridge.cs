using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class LightningImpactBridge : MonoBehaviour
    {
        [HideInInspector] public Entity  Target;
        [HideInInspector] public Vector3 WorldPosition;

        // Called by Animation Event
        public void OnImpact()
            => World.Current?.Events.Publish(new LightningImpactEvent { Target = Target, WorldPosition = WorldPosition });
    }
}
