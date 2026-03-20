using CLAYmore.ECS;
using System.Text;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade over SpawnerSystem + SpawnerComponent.
    /// Timing lives in ECS; this class handles pool retrieval and scene references.
    /// </summary>
    public class PotSpawner : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;
        public PlayerMovement playerMovement;
        public Economy economy;
        public Bootstrap bootstrap;

        [Header("Pools")]
        public PrefabPool potPool;
        public PrefabPool shadowPool;

        [Header("Pot Configs")]
        public PotConfig[] potConfigs;

        [Header("Spawn Timing")]
        public float initialInterval = 3f;
        public float minInterval = 0.5f;
        [Tooltip("Seconds subtracted from spawn interval per second of play time")]
        public float intervalDecreasePerSecond = 0.02f;

        [Header("Targeted Spawn")]
        [Tooltip("Every Nth spawn falls directly on the player's tile")]
        public int targetedSpawnEvery = 5;

        private Entity _entity;
        private int _spawnCount;

        private void Start()
        {
            _entity = gameObject.AddComponent<Entity>();
            _entity.Add(new SpawnerComponent
            {
                InitialInterval           = initialInterval,
                MinInterval               = minInterval,
                IntervalDecreasePerSecond = intervalDecreasePerSecond,
                CurrentInterval           = initialInterval,
                Timer                     = initialInterval
            });

            World.Current?.RegisterEntity(_entity);
            World.Current?.Events.Subscribe<SpawnRequestedEvent>(OnSpawnRequested);
            LogWeights();
        }

        private void LogWeights()
        {
            if (potConfigs == null || potConfigs.Length == 0) return;
            var sb = new StringBuilder("PotSpawner weights: ");
            float total = 0f;
            foreach (var c in potConfigs)
            {
                sb.Append($"{c.name}={c.spawnWeight} ");
                total += c.spawnWeight;
            }
            sb.Append($"(total={total})");
            Debug.Log(sb.ToString());
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<SpawnRequestedEvent>(OnSpawnRequested);
        }

        private void OnSpawnRequested(SpawnRequestedEvent evt)
        {
            if (evt.SpawnerEntity != _entity) return;
            if (bootstrap != null && bootstrap.IsGameOver) return;
            SpawnPot();
        }

        private PotConfig PickWeightedRandom(PotConfig[] configs)
        {
            float totalWeight = 0f;
            foreach (var c in configs)
                totalWeight += c.spawnWeight;

            if (totalWeight <= 0f)
            {
                Debug.LogWarning("PotSpawner: all spawnWeights are 0 — picking uniformly.");
                return configs[Random.Range(0, configs.Length)];
            }

            float roll = Random.Range(0f, totalWeight);
            float accumulated = 0f;
            foreach (var c in configs)
            {
                accumulated += c.spawnWeight;
                if (roll < accumulated) return c;
            }
            return configs[configs.Length - 1];
        }

        private void SpawnPot()
        {
            if (potPool == null || potConfigs == null || potConfigs.Length == 0) return;
            if (islandGenerator == null || playerMovement == null) return;

            _spawnCount++;
            bool isTargeted = targetedSpawnEvery > 0 && _spawnCount % targetedSpawnEvery == 0;

            PotConfig config = PickWeightedRandom(potConfigs);

            Vector3 landPos;
            if (isTargeted)
            {
                landPos = islandGenerator.GetCellCenter(
                    islandGenerator.GetCell(playerMovement.transform.position));
                if (!islandGenerator.TryReserveCell(landPos)) return;
            }
            else
            {
                if (!islandGenerator.TryGetRandomWalkableCellCenter(out landPos)) return;
                if (!islandGenerator.TryReserveCell(landPos)) return;
            }

            Vector3 spawnPos = landPos + Vector3.up * config.spawnHeight;

            GameObject potGO = potPool.Get(spawnPos);
            if (!potGO.TryGetComponent<Pot>(out Pot pot))
            {
                Debug.LogWarning("PotSpawner: pot prefab is missing a Pot component.");
                potPool.Return(potGO);
                return;
            }

            pot.Initialize(config, landPos, islandGenerator.tilemap,
                           economy,
                           islandGenerator, potPool, shadowPool);
        }
    }
}
