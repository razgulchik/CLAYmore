using System.Collections;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class ShockwaveVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool tilePool;
        [SerializeField] private Sprite[]   emptyFrames;
        [SerializeField] private Sprite[]   potFrames;
        [SerializeField] private float      frameInterval = 0.1f;

        private void OnEnable()
            => World.Current?.Events.Subscribe<ShockwaveEvent>(OnShockwave);

        private void OnDisable()
            => World.Current?.Events.Unsubscribe<ShockwaveEvent>(OnShockwave);

        private void OnShockwave(ShockwaveEvent evt)
        {
            if (tilePool == null) return;

            for (int i = 0; i < evt.TilePositions.Length; i++)
            {
                Sprite[] frames = evt.HadPot[i] ? potFrames : emptyFrames;
                StartCoroutine(PlaySpike(evt.TilePositions[i], frames, i * frameInterval));
            }
        }

        private IEnumerator PlaySpike(Vector3 pos, Sprite[] frames, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            GameObject tile = tilePool.Get(pos);
            var sr = tile.GetComponent<SpriteRenderer>();

            if (sr != null && frames != null)
            {
                foreach (Sprite frame in frames)
                {
                    sr.sprite = frame;
                    yield return new WaitForSeconds(frameInterval);
                }
            }

            tilePool.Return(tile);
        }
    }
}
