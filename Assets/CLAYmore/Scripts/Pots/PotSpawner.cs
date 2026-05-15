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
        [Header("Startup Pots")]
        [Tooltip("Pots to spawn on the ground at game start. Add configs in any order.")]
        public PotConfig[] startupPotConfigs;

        [Header("Pot Configs")]
        [Tooltip("All pot types including rocks (isRock = true). Rocks are picked separately via wave rockSpawnChance.")]
        public PotConfig[] potConfigs;
        [Tooltip("Golden urn config — spawned independently via GoldenUrnModifier chance.")]
        [SerializeField] private PotConfig _goldenUrnConfig;
        [Tooltip("Heart pickup pool — spawned independently via LuckyDayModifier chance.")]
        [SerializeField] private PrefabPool _hearthPool;

        private IslandGenerator _islandGenerator;
        private PlayerMovement  _playerMovement;
        private Economy         _economy;
        private PrefabPool      _potPool;
        private PrefabPool      _shadowPool;
        private PrefabPool      _coinPool;
        private PrefabPool      _shardsPool;

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

        public void Init(IslandGenerator islandGenerator, Economy economy, PlayerMovement playerMovement,
                         PrefabPool potPool, PrefabPool shadowPool, PrefabPool coinPool, PrefabPool shardsPool,
                         PrefabPool hearthPool)
        {
            _islandGenerator = islandGenerator;
            _economy         = economy;
            _playerMovement  = playerMovement;
            _potPool         = potPool;
            _shadowPool      = shadowPool;
            _coinPool        = coinPool;
            _shardsPool      = shardsPool;
            _hearthPool      = hearthPool;
        }

        private void Start()
        {
            _potOnlyConfigs  = System.Array.FindAll(potConfigs ?? System.Array.Empty<PotConfig>(), c => !c.isRock);
            _rockOnlyConfigs = System.Array.FindAll(potConfigs ?? System.Array.Empty<PotConfig>(), c => c.isRock);

            _entity = gameObject.AddComponent<Entity>();
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

        // ── Startup ───────────────────────────────────────────────────────────

        public void SpawnStartupPots(Vector3 playerPos)
        {
            if (startupPotConfigs == null || startupPotConfigs.Length == 0) return;
            if (_islandGenerator == null || _potPool == null) return;

            Vector3Int playerCell = _islandGenerator.GetCell(playerPos);

            foreach (var config in startupPotConfigs)
            {
                if (config == null) continue;

                Vector3 landPos;
                int attempts = 0;
                do
                {
                    if (!_islandGenerator.TryGetRandomWalkableCellCenter(out landPos)) break;
                    attempts++;
                }
                while (_islandGenerator.GetCell(landPos) == playerCell && attempts < 20);

                if (!_islandGenerator.TryReserveCell(landPos)) continue;

                GameObject potGO = _potPool.Get(landPos);
                if (!potGO.TryGetComponent(out Pot pot))
                {
                    _potPool.Return(potGO);
                    continue;
                }

                pot.Initialize(config, landPos, _islandGenerator.tilemap,
                               _economy, _islandGenerator,
                               _potPool, _shadowPool, _coinPool, _shardsPool,
                               fallDurationMultiplier: 0f);
            }
        }

        // ── Weight picking ────────────────────────────────────────────────────

        private float GetGoldenUrnChance()
        {
            if (World.Current == null) return 0f;
            foreach (Entity e in World.Current.Query<PlayerStatsComponent>())
                return e.Get<PlayerStatsComponent>().GoldenUrnChance;
            return 0f;
        }

        private float GetHearthChance()
        {
            if (World.Current == null) return 0f;
            foreach (Entity e in World.Current.Query<PlayerStatsComponent>())
                return e.Get<PlayerStatsComponent>().HearthChance;
            return 0f;
        }

        private PotConfig PickWeightedRandom()
        {
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
            if (_potPool == null || potConfigs == null || potConfigs.Length == 0) return;
            if (_islandGenerator == null || _playerMovement == null) return;

            _spawnCount++;
            bool isTargeted = _spawnCount == _nextTargetedAt;
            if (isTargeted)
                _nextTargetedAt = _spawnCount + Random.Range(targetedSpawnEveryMin, targetedSpawnEveryMax + 1);

            bool spawnRock = !isTargeted
                && _currentWave != null
                && _currentWave.rockSpawnChance > 0f
                && _rockOnlyConfigs.Length > 0
                && Random.value < _currentWave.rockSpawnChance;

            // ── Hearth chance (before rock/urn selection) ─────────────────────
            float hearthChance = _hearthPool != null ? GetHearthChance() : 0f;
            if (!isTargeted && !spawnRock && hearthChance > 0f && Random.value < hearthChance)
            {
                if (!_islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 hearthLandPos)) return;
                if (!_islandGenerator.TryReserveCell(hearthLandPos)) return;

                GameObject hearthGO = _hearthPool.Get(hearthLandPos);
                if (hearthGO.TryGetComponent<HearthPickup>(out HearthPickup pickup))
                    pickup.Initialize(hearthLandPos, _islandGenerator, _hearthPool);
                return;
            }

            PotConfig config;
            if (spawnRock)
            {
                config = _rockOnlyConfigs[Random.Range(0, _rockOnlyConfigs.Length)];
            }
            else
            {
                float urnChance = _goldenUrnConfig != null ? GetGoldenUrnChance() : 0f;
                if (urnChance > 0f && Random.value < urnChance)
                {
                    config = _goldenUrnConfig;
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
            }

            Vector3 landPos;
            if (isTargeted)
            {
                landPos = _islandGenerator.GetCellCenter(
                    _islandGenerator.GetCell(_playerMovement.transform.position));
                if (!_islandGenerator.TryReserveCell(landPos)) return;
            }
            else
            {
                if (!_islandGenerator.TryGetRandomWalkableCellCenter(out landPos)) return;
                if (!_islandGenerator.TryReserveCell(landPos)) return;
            }

            Vector3 spawnPos = landPos + Vector3.up * config.spawnHeight;

            GameObject potGO = _potPool.Get(spawnPos);
            if (!potGO.TryGetComponent<Pot>(out Pot pot))
            {
                Debug.LogWarning("PotSpawner: pot prefab is missing a Pot component.");
                _potPool.Return(potGO);
                return;
            }

            pot.Initialize(config, landPos, _islandGenerator.tilemap,
                           _economy,
                           _islandGenerator, _potPool, _shadowPool, _coinPool, _shardsPool,
                           _fallDurationMultiplier);
        }
    }
}
