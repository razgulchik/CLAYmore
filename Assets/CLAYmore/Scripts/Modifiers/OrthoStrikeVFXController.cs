using System.Collections;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Listens to OrthoStrikeEvent and spawns spark VFX flying outward in 4 directions.
    /// Attach to any scene GameObject and assign sparksPool in the Inspector.
    /// </summary>
    public class OrthoStrikeVFXController : MonoBehaviour
    {
        public PrefabPool sparksPool;
        public IslandGenerator islandGenerator;

        [Tooltip("How far (world units) each spark travels — set to cell width")]
        public float travelDistance = 1f;

        [Tooltip("Duration of the outward travel in seconds")]
        public float travelDuration = 0.2f;

        [Tooltip("How long before returning the VFX to the pool after travel ends")]
        public float lingerDuration = 0.1f;

        private void OnEnable()
            => World.Current?.Events.Subscribe<OrthoStrikeEvent>(OnOrthoStrike);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<OrthoStrikeEvent>(OnOrthoStrike);

        private void OnOrthoStrike(OrthoStrikeEvent evt)
        {
            if (sparksPool == null) return;

            float distance = islandGenerator != null
                ? islandGenerator.tilemap.cellSize.x
                : travelDistance;

            Vector3[] dirs = evt.MovedHorizontally
                ? new[] { Vector3.up, Vector3.down }
                : new[] { Vector3.right, Vector3.left };

            foreach (Vector3 dir in dirs)
            {
                GameObject vfx = sparksPool.Get(evt.Origin);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                vfx.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                StartCoroutine(MoveAndReturn(vfx, dir * distance));
            }
        }

        private IEnumerator MoveAndReturn(GameObject vfx, Vector3 delta)
        {
            Vector3 start = vfx.transform.position;
            Vector3 end   = start + delta;
            float elapsed = 0f;

            while (elapsed < travelDuration)
            {
                elapsed += Time.deltaTime;
                vfx.transform.position = Vector3.Lerp(start, end, elapsed / travelDuration);
                yield return null;
            }

            vfx.transform.position = end;
            yield return new WaitForSeconds(lingerDuration);
            sparksPool.Return(vfx);
        }
    }
}
