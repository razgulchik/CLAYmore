using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade over SpawnerSystem + SpawnerComponent.
    /// Spawns chests on random walkable cells at a fixed (non-accelerating) interval.
    /// </summary>
    public class ChestSpawner : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;
        public Bootstrap       bootstrap;

        [Header("Pool")]
        public PrefabPool chestPool;

        [Header("Config")]
        public ChestConfig chestConfig;
        public float initialInterval = 30f;
        public float minInterval     = 20f;

        private Entity _entity;

        private void Start()
        {
            if (chestConfig == null || chestPool == null || islandGenerator == null)
            {
                Debug.LogWarning("ChestSpawner: missing references — disabled.");
                enabled = false;
                return;
            }

            _entity = gameObject.AddComponent<Entity>();
            _entity.Add(new SpawnerComponent
            {
                InitialInterval           = initialInterval,
                MinInterval               = minInterval,
                IntervalDecreasePerSecond = 0f,   // chests don't accelerate
                CurrentInterval           = initialInterval,
                Timer                     = initialInterval,
            });

            World.Current?.RegisterEntity(_entity);
            World.Current?.Events.Subscribe<SpawnRequestedEvent>(OnSpawnRequested);
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<SpawnRequestedEvent>(OnSpawnRequested);
        }

        private void OnSpawnRequested(SpawnRequestedEvent evt)
        {
            if (evt.SpawnerEntity != _entity) return;
            if (bootstrap != null && bootstrap.IsGameOver) return;
            SpawnChest();
        }

        private void SpawnChest()
        {
            if (!islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 landPos)) return;

            GameObject chestGO = chestPool.Get(landPos);
            if (!chestGO.TryGetComponent<Chest>(out Chest chest))
            {
                Debug.LogWarning("ChestSpawner: chest prefab is missing a Chest component.");
                chestPool.Return(chestGO);
                return;
            }

            chest.Initialize(chestConfig, landPos, islandGenerator.tilemap, chestPool);
        }
    }
}
