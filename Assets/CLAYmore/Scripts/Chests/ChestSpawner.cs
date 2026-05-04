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
        [SerializeField] private float _initialInterval = 30f;
        [SerializeField] private float _minInterval     = 20f;

        private IslandGenerator _islandGenerator;
        private PrefabPool      _chestPool;
        private Entity          _entity;
        private bool            _isGameOver;

        public void Init(IslandGenerator islandGenerator, PrefabPool chestPool,
                         float initialInterval, float minInterval)
        {
            _islandGenerator  = islandGenerator;
            _chestPool        = chestPool;
            _initialInterval  = initialInterval;
            _minInterval      = minInterval;
        }

        private void Start()
        {
            if (_chestPool == null || _islandGenerator == null)
            {
                Debug.LogWarning("ChestSpawner: missing references — disabled.");
                enabled = false;
                return;
            }

            _entity = gameObject.AddComponent<Entity>();
            _entity.Add(new SpawnerComponent
            {
                InitialInterval           = _initialInterval,
                MinInterval               = _minInterval,
                IntervalDecreasePerSecond = 0f,
                CurrentInterval           = _initialInterval,
                Timer                     = _initialInterval,
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
            if (!_islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 landPos, avoidPlayerNeighbours: true)) return;
            if (!_islandGenerator.TryMarkChestLanded(landPos)) return;

            GameObject chestGO = _chestPool.Get(landPos);
            if (!chestGO.TryGetComponent<Chest>(out Chest chest))
            {
                Debug.LogWarning("ChestSpawner: chest prefab is missing a Chest component.");
                _islandGenerator.ClearChest(landPos);
                _chestPool.Return(chestGO);
                return;
            }

            chest.Initialize(landPos, _islandGenerator.tilemap, _chestPool, _islandGenerator);
        }
    }
}
