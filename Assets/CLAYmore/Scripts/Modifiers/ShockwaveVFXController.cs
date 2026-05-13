using System.Collections;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class ShockwaveVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool tilePool;
        [SerializeField] private float      staggerInterval = 0.1f;
        [SerializeField] private float      spikeReturnDelay = 0.5f;

        private void OnEnable()
            => World.Current?.Events.Subscribe<ShockwaveEvent>(OnShockwave);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<ShockwaveEvent>(OnShockwave);

        private void OnShockwave(ShockwaveEvent evt)
        {
            if (tilePool == null) return;

            for (int i = 0; i < evt.TilePositions.Length; i++)
                StartCoroutine(PlaySpike(evt.TilePositions[i], evt.Cells[i], evt.HadPot[i], i * staggerInterval));
        }

        private IEnumerator PlaySpike(Vector3 pos, Vector2Int cell, bool hadPot, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            GameObject tile = tilePool.Get(pos);

            var bridge = tile.GetComponent<ShockwaveCellBridge>();
            if (bridge != null)
            {
                bridge.Cell          = cell;
                bridge.WorldPosition = pos;
            }

            var animator = tile.GetComponent<Animator>();
            if (animator != null)
                animator.Play(hadPot ? "ShockwaveDmg" : "ShockwaveNoDmg", 0, 0f);

            yield return new WaitForSeconds(spikeReturnDelay);
            tilePool.Return(tile);
        }
    }
}
