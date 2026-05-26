using System.Collections;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class FireTrailVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool firePool;

        private void OnEnable()
            => World.Current?.Events.Subscribe<FireTrailEvent>(OnFireTrail);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<FireTrailEvent>(OnFireTrail);

        private void OnFireTrail(FireTrailEvent evt)
        {
            if (firePool == null) return;
            GameObject vfx = firePool.Get(evt.WorldPosition);
            StartCoroutine(ReturnAfterBurn(vfx, evt.Lifetime));
        }

        private IEnumerator ReturnAfterBurn(GameObject vfx, float duration)
        {
            yield return new WaitForSeconds(duration);
            firePool.Return(vfx);
        }
    }
}
