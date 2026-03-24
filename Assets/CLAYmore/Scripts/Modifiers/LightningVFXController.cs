using System.Collections;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Listens to LightningStrikeEvent and plays the lightning VFX prefab
    /// from a pool at the struck pot's world position.
    /// Attach to any scene GameObject and assign lightningPool in the Inspector.
    /// </summary>
    public class LightningVFXController : MonoBehaviour
    {
        public PrefabPool      lightningPool;
        public IslandGenerator islandGenerator;

        [Tooltip("How long to wait before returning the VFX to the pool (match animation clip length)")]
        public float vfxDuration = 1f;

        public Vector2 positionOffset;

        private void OnEnable()
            => World.Current?.Events.Subscribe<LightningStrikeEvent>(OnLightningStrike);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<LightningStrikeEvent>(OnLightningStrike);

        private void OnLightningStrike(LightningStrikeEvent evt)
        {
            if (lightningPool == null) return;
            GameObject vfx = lightningPool.Get(evt.WorldPosition);
            AlignBottomToCell(vfx, evt.WorldPosition);
            StartCoroutine(ReturnAfterDelay(vfx));
        }

        // Сдвигает VFX так, чтобы нижний край спрайта совпал с нижним краем клетки
        private void AlignBottomToCell(GameObject vfx, Vector3 cellCenter)
        {
            if (islandGenerator == null) return;
            var sr = vfx.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) return;

            float cellBottom   = cellCenter.y - islandGenerator.tilemap.cellSize.y * 0.5f;
            float spriteBottom = sr.bounds.min.y;
            vfx.transform.position += new Vector3(positionOffset.x, cellBottom - spriteBottom + positionOffset.y, 0f);
        }

        private IEnumerator ReturnAfterDelay(GameObject vfx)
        {
            yield return new WaitForSeconds(vfxDuration);
            lightningPool.Return(vfx);
        }
    }
}
