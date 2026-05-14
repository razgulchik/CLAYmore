using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class LightningVFXController : MonoBehaviour
    {
        public PrefabPool      lightningPool;
        public IslandGenerator islandGenerator;
        public Vector2         positionOffset;

        private void OnEnable()
            => World.Current?.Events.Subscribe<LightningStrikeEvent>(OnLightningStrike);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<LightningStrikeEvent>(OnLightningStrike);

        private void OnLightningStrike(LightningStrikeEvent evt)
        {
            if (lightningPool == null) return;
            GameObject vfx = lightningPool.Get(evt.WorldPosition);
            AlignBottomToCell(vfx, evt.WorldPosition);

            var bridge = vfx.GetComponent<VFXBridge>();
            if (bridge != null)
            {
                bridge.OnImpact   = () => World.Current?.Events.Publish(new LightningImpactEvent { Target = evt.Target, WorldPosition = evt.WorldPosition });
                bridge.OnComplete = () => lightningPool.Return(vfx);
            }
        }

        private void AlignBottomToCell(GameObject vfx, Vector3 cellCenter)
        {
            if (islandGenerator == null) return;
            var sr = vfx.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) return;

            float cellBottom   = cellCenter.y - islandGenerator.tilemap.cellSize.y * 0.5f;
            float spriteBottom = sr.bounds.min.y;
            vfx.transform.position += new Vector3(positionOffset.x, cellBottom - spriteBottom + positionOffset.y, 0f);
        }
    }
}
