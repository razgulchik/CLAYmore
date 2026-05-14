using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class WhirlwindVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool vfxPool;

        private void OnEnable()
            => World.Current?.Events.Subscribe<WhirlwindActivatedEvent>(OnWhirlwindActivated);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<WhirlwindActivatedEvent>(OnWhirlwindActivated);

        private void OnWhirlwindActivated(WhirlwindActivatedEvent evt)
        {
            if (vfxPool == null) return;
            GameObject vfx = vfxPool.Get(evt.WorldPosition);

            var bridge = vfx.GetComponent<VFXBridge>();
            if (bridge != null)
            {
                bridge.OnImpact   = () => World.Current?.Events.Publish(new WhirlwindDetonateEvent { Cell = evt.Cell });
                bridge.OnComplete = () => vfxPool.Return(vfx);
            }
        }
    }
}
