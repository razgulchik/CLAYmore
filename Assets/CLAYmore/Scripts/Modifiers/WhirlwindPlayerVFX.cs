using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class WhirlwindPlayerVFX : MonoBehaviour
    {
        [SerializeField] private GameObject _vfxObject;

        private void OnEnable()  => World.Current?.Events.Subscribe<WhirlwindActivatedEvent>(OnActivated);
        private void OnDisable() => World.Current?.Events.Unsubscribe<WhirlwindActivatedEvent>(OnActivated);

        private void OnActivated(WhirlwindActivatedEvent evt)
        {
            if (_vfxObject == null) return;
            _vfxObject.SetActive(true);
            var bridge = _vfxObject.GetComponent<VFXBridge>();
            if (bridge != null)
            {
                bridge.OnImpact   = () => World.Current?.Events.Publish(new WhirlwindDetonateEvent { Cell = evt.Cell });
                bridge.OnComplete = () => _vfxObject.SetActive(false);
            }
        }
    }
}
