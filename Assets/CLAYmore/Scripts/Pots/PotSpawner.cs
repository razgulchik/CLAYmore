using CLAYmore.ECS;
using System.Text;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade over SpawnerSystem + SpawnerComponent.
    /// Timing lives in ECS; this class handles pool retrieval and scene references.
    /// Responds to WaveChangedEvent to update spawn parameters each wave.
    /// </summary>
    public class PotSpawner : MonoBehaviour
    {
        [Header("References")]
        public IslandGenerator islandGenerator;
        public PlayerMovement playerMovement;
        public Economy economy;

        [Header("Pools")]
        public PrefabPool potPool;
        public PrefabPool shadowPool;
        public PrefabPool coinPool;

        [Header("Pot Configs")]
        [Tooltip("All pot types including rocks (isRock = true). Rocks are picked separately via wave rockSpawnChance.")]
        public PotConfig[] potConfigs;

        private int targetedSpawnEveryMin;
        private int targetedSpawnEveryMax;

        private Entity      _entity;
        private int         _spawnCount;
        private int         _nextTargetedAt;
        private bool        _isGameOver;

        private WaveConfig  _currentWave;
        private float       _fallDurationMultiplier = 1f;
        private PotConfig[] _potOnlyConfigs;
        private PotConfig[] _rockOnlyConfigs;

        private void Start()
        {
            _potOnlyConfigs  = System.Array.FindAll(potConfigs ?? System.Array.Empty<PotConfig>(), c => !c.isRock);
            _rockOnlyConfigs = System.Array.FindAll(potConfigs ?? System.Array.Empty<PotConfig>(), c => c.isRock);

            _entity = gameObject.AddComponent<Entity>();
            // Timing is fully controlled by WaveConfig — start with infinite timer
            // so nothing spawns until Wave 0 fires and sets real values.
            _entity.Add(new SpawnerComponent
            {
                InitialInterval           = float.MaxValue,
                MinInterval               = float.MaxValue,
                IntervalDecreasePerSecond = 0f,
                CurrentInterval           = float.MaxValue,
                Timer                     = float.MaxValue
            });

            World.Current?.RegisterEntity(_entity);
            World.Current?.Events.Subscribe<SpawnRequestedEvent>(OnSpawnRequested);
            World.Current?.Events.Subscribe<WaveChangedEvent>(OnWaveChanged);
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);
            LogWeights();
        }

        private void LogWeights()
        {
            if (potConfigs == null || potConfigs.Length == 0) return;
            var sb = new StringBuilder("PotSpawner weights: ");
            float total = 0f;
            foreach (var c in potConfigs)
            {
                sb.Append($"{c.name}={c.spawnWeight}{(c.isRock ? "(rock)" : "")} ");
                total += c.spawnWeight;
            }
            sb.Append($"(total={total})");
            Debug.Log(sb.ToString());
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<SpawnRequestedEvent>(OnSpawnRequested);
            World.Current?.Events.Unsubscribe<WaveChangedEvent>(OnWaveChanged);
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void OnGameOver(GameOverEvent e) => _isGameOver = true;

        private void OnWaveChanged(WaveChangedEvent evt)
        {
            _currentWave            = evt.Config;
            _fallDurationMultiplier = evt.Config.fallDurationMultiplier;
            targetedSpawnEveryMin = evt.Config.targetedSpawnEveryMin;
            targetedSpawnEveryMax = evt.Config.targetedSpawnEveryMax;

            if (evt.WaveIndex == 0)
                _nextTargetedAt = Random.Range(targetedSpawnEveryMin, targetedSpawnEveryMax + 1);

            var s = _entity.Get<SpawnerComponent>();
            s.CurrentInterval           = evt.Config.spawnInterval;
            s.MinInterval               = evt.Config.minSpawnInterval;
            s.IntervalDecreasePerSecond = evt.Config.spawnDecreasePerSecond;
            s.Timer                     = evt.Config.spawnInterval;

            Debug.Log($"[PotSpawner] Wave {evt.WaveIndex}: interval={evt.Config.spawnInterval:F2}s, " +
                      $"fall×{evt.Config.fallDurationMultiplier:F2}, " +
                      $"rockChance={evt.Config.rockSpawnChance:P0}");
        }

        private void OnSpawnRequested(SpawnRequestedEvent evt)
        {
            if (evt.SpawnerEntity != _entity) return;
            if (_isGameOver) return;
            SpawnPot();
        }

        // ── Weight picking ────────────────────────────────────────────────────

        private PotConfig PickWeightedRandom()
        {
            // Use per-wave overrides if defined for this wave
            if (_currentWave?.potWeights != null && _currentWave.potWeights.Length > 0)
            {
                float total = 0f;
                foreach (var pw in _currentWave.potWeights)
                    total += pw.weight;

                if (total > 0f)
                {
                    float roll = Random.Range(0f, total);
                    float acc  = 0f;
                    foreach (var pw in _currentWave.potWeights)
                    {
                        acc += pw.weight;
                        if (roll < acc) return pw.config;
                    }
                    return _currentWave.potWeights[_currentWave.potWeights.Length - 1].config;
                }
            }

            // Fallback: use PotConfig.spawnWeight from non-rock configs
            return PickWeightedFromConfigs(_potOnlyConfigs);
        }

        private PotConfig PickWeightedFromConfigs(PotConfig[] configs)
        {
            float totalWeight = 0f;
            foreach (var c in configs)
                totalWeight += c.spawnWeight;

            if (totalWeight <= 0f)
                return configs[Random.Range(0, configs.Length)];

            float roll = Random.Range(0f, totalWeight);
            float accumulated = 0f;
            foreach (var c in configs)
            {
                accumulated += c.spawnWeight;
                if (roll < accumulated) return c;
            }
            return configs[configs.Length - 1];
        }

        // ── Spawning ──────────────────────────────────────────────────────────

        private void SpawnPot()
        {
            if (potPool == null || potConfigs == null || potConfigs.Length == 0) return;
            if (islandGenerator == null || playerMovement == null) return;

            _spawnCount++;
            bool isTargeted = _spawnCount == _nextTargetedAt;
            if (isTargeted)
                _nextTargetedAt = _spawnCount + Random.Range(targetedSpawnEveryMin, targetedSpawnEveryMax + 1);

            // Decide: rock or pot? Rocks never target the player.
            bool spawnRock = !isTargeted
                && _currentWave != null
                && _currentWave.rockSpawnChance > 0f
                && _rockOnlyConfigs.Length > 0
                && Random.value < _currentWave.rockSpawnChance;

            PotConfig config;
            if (spawnRock)
            {
                config = _rockOnlyConfigs[Random.Range(0, _rockOnlyConfigs.Length)];
            }
            else
            {
                if (_potOnlyConfigs.Length == 0)
                {
                    Debug.LogWarning("PotSpawner: no non-rock pot configs available.");
                    return;
                }
                config = PickWeightedRandom();
            }

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
                           islandGenerator, potPool, shadowPool, coinPool,
                           _fallDurationMultiplier);
        }
    }
}
