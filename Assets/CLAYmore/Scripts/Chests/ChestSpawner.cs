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
        [HideInInspector] public IslandGenerator islandGenerator;  // set by Bootstrap

        [Header("Pool")]
        [HideInInspector] public PrefabPool chestPool;              // set by Bootstrap

        [Header("Config")]
        public ChestConfig chestConfig;
        public float initialInterval = 30f;
        public float minInterval     = 20f;

        private Entity _entity;
        private bool   _isGameOver;

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
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<SpawnRequestedEvent>(OnSpawnRequested);
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void OnGameOver(GameOverEvent e) => _isGameOver = true;

        private void OnSpawnRequested(SpawnRequestedEvent evt)
        {
            if (evt.SpawnerEntity != _entity) return;
            if (_isGameOver) return;
            SpawnChest();
        }

        private void SpawnChest()
        {
            if (!islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 landPos, avoidPlayerNeighbours: true)) return;
            if (!islandGenerator.TryMarkChestLanded(landPos)) return;

            GameObject chestGO = chestPool.Get(landPos);
            if (!chestGO.TryGetComponent<Chest>(out Chest chest))
            {
                Debug.LogWarning("ChestSpawner: chest prefab is missing a Chest component.");
                islandGenerator.ClearChest(landPos);
                chestPool.Return(chestGO);
                return;
            }

            chest.Initialize(chestConfig, landPos, islandGenerator.tilemap, chestPool, islandGenerator);
        }
    }
}
