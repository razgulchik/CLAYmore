using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class FireBlazeVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool vfxPool;
        [SerializeField] private Vector2 spawnOffset;

        private void OnEnable()
            => World.Current?.Events.Subscribe<FireBlazeActivatedEvent>(OnFireBlazeActivated);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<FireBlazeActivatedEvent>(OnFireBlazeActivated);

        private void OnFireBlazeActivated(FireBlazeActivatedEvent evt)
        {
            if (vfxPool == null) return;
            SpawnBlaze(evt.Origin, evt.DiagDir1, evt.Cell1);
            SpawnBlaze(evt.Origin, evt.DiagDir2, evt.Cell2);
        }

        private void SpawnBlaze(Vector3 origin, Vector2Int dir, Vector2Int cell)
        {
            Vector3 spawnPos = origin + new Vector3(dir.x * spawnOffset.x, dir.y * spawnOffset.y, 0f);
            GameObject vfx = vfxPool.Get(spawnPos);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            vfx.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var bridge = vfx.GetComponent<VFXBridge>();
            if (bridge != null)
            {
                bridge.OnImpact   = () => World.Current?.Events.Publish(new FireBlazeImpactEvent { Cell = cell });
                bridge.OnComplete = () => vfxPool.Return(vfx);
            }
        }
    }
}
