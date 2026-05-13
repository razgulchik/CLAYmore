using System.Collections;
using System.Collections.Generic;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class BallLightningVFXController : MonoBehaviour
    {
        [SerializeField] private PrefabPool ballLightningPool;
        [SerializeField] private PrefabPool explosionPool;

        [Tooltip("Seconds before ball lightning auto-triggers. 0 = stays until activated by player.")]
        public float lifetime = 0f;

        private readonly Dictionary<Vector2Int, GameObject> _activeOrbs = new();

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<BallLightningSpawnedEvent>(OnBallLightningSpawned);
            World.Current?.Events.Subscribe<BallLightningExplodedEvent>(OnBallLightningExploded);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<BallLightningSpawnedEvent>(OnBallLightningSpawned);
            World.Current?.Events.Unsubscribe<BallLightningExplodedEvent>(OnBallLightningExploded);
        }

        private void OnBallLightningSpawned(BallLightningSpawnedEvent evt)
        {
            if (ballLightningPool == null) return;
            if (_activeOrbs.ContainsKey(evt.Cell)) return;

            GameObject orb = ballLightningPool.Get(evt.WorldPosition);
            _activeOrbs[evt.Cell] = orb;

            if (lifetime > 0f)
                StartCoroutine(LifetimeCoroutine(evt.Cell));
        }

        private void OnBallLightningExploded(BallLightningExplodedEvent evt)
        {
            if (_activeOrbs.TryGetValue(evt.Cell, out GameObject orb))
            {
                _activeOrbs.Remove(evt.Cell);
                if (ballLightningPool != null) ballLightningPool.Return(orb);
            }

            StartCoroutine(ExplodeAfterDelay(evt.Cell, evt.WorldPosition));
        }

        private IEnumerator ExplodeAfterDelay(Vector2Int cell, Vector3 worldPos)
        {
            if (explosionPool != null)
                StartCoroutine(ReturnExplosionAfterDelay(explosionPool.Get(worldPos)));

            if (explosionDelay > 0f)
                yield return new WaitForSeconds(explosionDelay);

            World.Current?.Events.Publish(new BallLightningDetonateEvent { Cell = cell, WorldPosition = worldPos });
        }

        private IEnumerator LifetimeCoroutine(Vector2Int cell)
        {
            yield return new WaitForSeconds(lifetime);
            if (!_activeOrbs.ContainsKey(cell)) yield break;
            World.Current?.Events.Publish(new BallLightningExpiredEvent { Cell = cell });
        }

        [Tooltip("Delay before the explosion animation plays and damage is applied")]
        [SerializeField] private float explosionDelay = 0f;

        [Tooltip("How long to wait before returning the explosion VFX to the pool (match animation clip length)")]
        [SerializeField] private float explosionDuration = 1f;

        private IEnumerator ReturnExplosionAfterDelay(GameObject vfx)
        {
            yield return new WaitForSeconds(explosionDuration);
            if (explosionPool != null) explosionPool.Return(vfx);
        }
    }
}
