using CLAYmore.ECS;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLAYmore
{
    /// <summary>
    /// Bootstrap: creates the ECS World, registers all systems, then wires scene references.
    /// No game logic lives here — only startup glue.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Bootstrap : MonoBehaviour
    {
        [Header("Config")]
        public GameConfig config;

        [Header("References")]
        public IslandGenerator  islandGenerator;
        public PotSpawner       potSpawner;
        public ChestSpawner     chestSpawner;
        public StatsTracker     statsTracker;
        public LeaderboardService leaderboardService;
        public HUD              hud;

        [Header("Camera")]
        public CinemachineCamera virtualCamera;

        [Header("Player")]
        public GameObject playerPrefab;

        [Header("Pools")]
        public PrefabPool potPool;
        public PrefabPool shadowPool;
        public PrefabPool coinPool;
        public PrefabPool chestPool;

        [Header("Chest / Modifiers")]
        [Tooltip("Coins awarded to the player when skipping a modifier choice")]
        public int coinsOnSkip = 5;
        public ModifierConfig[] modifierPool;
        public ModifierChoiceUI modifierChoiceUI;
        [Tooltip("Modifiers applied to the player at game start (for testing / preset builds)")]
        public ModifierConfig[] startingModifiers;

        public bool IsGameOver { get; private set; }

        private World  _world;
        private Entity _playerEntity;

        private void Awake()
        {
            PauseManager.Instance.Reset();

            // ── Create World and register all systems ──────────────────────
            _world = new World();
            _world.RegisterSystem(new HealthSystem());
            _world.RegisterSystem(new DamageSystem(islandGenerator));
            _world.RegisterSystem(new EconomySystem());
            _world.RegisterSystem(new SpawnerSystem());
            _world.RegisterSystem(new MovementSystem(islandGenerator));
            _world.RegisterSystem(new ChestSystem(islandGenerator));
            _world.RegisterSystem(new ModifierSystem());
            _world.RegisterSystem(new AbilitySystem(islandGenerator));
        }

        private void Start()
        {
            if (playerPrefab == null || islandGenerator == null)
            {
                Debug.LogError("Bootstrap: playerPrefab or islandGenerator is not assigned.");
                return;
            }

            // ── Spawn player ───────────────────────────────────────────────
            if (!islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 spawnPos))
            {
                Debug.LogError("Bootstrap: no walkable cell available to spawn player.");
                return;
            }
            GameObject playerGO = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            islandGenerator.SetPlayerTileFromWorldPos(spawnPos);

            if (virtualCamera != null)
                virtualCamera.Follow = playerGO.transform;

            var playerMovement = playerGO.GetComponent<PlayerMovement>();
            var playerHealth   = playerGO.GetComponent<PlayerHealth>();

            // Register player entity with the world so systems can query it.
            var playerEntity = playerGO.GetComponent<Entity>();
            if (playerEntity != null)
            {
                _world.RegisterEntity(playerEntity);
                playerEntity.Add(new CLAYmore.ECS.PlayerStatsComponent());
                playerEntity.Add(new CLAYmore.ECS.PlayerModifiersComponent());
            }

            // ── Apply config ───────────────────────────────────────────────
            if (statsTracker != null)
                statsTracker.Init(config);

            if (leaderboardService != null)
                leaderboardService.Init(statsTracker);

            if (config != null)
            {
                if (playerHealth   != null) playerHealth.maxHp        = config.playerMaxHp;
                if (playerMovement != null)
                {
                    playerMovement.moveTime         = config.moveTime;
                    playerMovement.bounceReturnTime = config.bounceReturnTime;
                }

                // Sync ECS components with config values (Awake sets Inspector defaults; Start overrides)
                if (playerEntity != null && config != null)
                {
                    var statsComp = playerEntity.Get<CLAYmore.ECS.PlayerStatsComponent>();
                    statsComp.BaseMoveTime         = config.moveTime;
                    statsComp.BaseBounceReturnTime = config.bounceReturnTime;
                    if (playerEntity.Has<MovementComponent>())
                        playerEntity.Get<MovementComponent>().MoveTime = config.moveTime;
                }
                if (chestSpawner != null)
                {
                    chestSpawner.initialInterval = config.chestSpawnInitialInterval;
                    chestSpawner.minInterval     = config.chestSpawnMinInterval;
                }
            }

            // ── Wire scene references ──────────────────────────────────────
            if (playerMovement != null)
                playerMovement.islandGenerator = islandGenerator;

            if (potSpawner != null)
            {
                potSpawner.playerMovement = playerMovement;
                potSpawner.potPool        = potPool;
                potSpawner.shadowPool     = shadowPool;
                potSpawner.coinPool       = coinPool;
            }

            if (chestSpawner != null)
            {
                chestSpawner.islandGenerator = islandGenerator;
                chestSpawner.chestPool       = chestPool;
            }


            if (modifierChoiceUI != null)
            {
                modifierChoiceUI.modifierPool = modifierPool;
                modifierChoiceUI.coinsOnSkip  = coinsOnSkip;
            }

            _world.RegisterSystem(new SessionTimerSystem(config?.waves));

            _playerEntity = playerEntity;

            if (startingModifiers != null)
            {
                var modLevels = new System.Collections.Generic.Dictionary<string, int>();
                foreach (var mod in startingModifiers)
                {
                    if (mod == null) continue;
                    modLevels.TryGetValue(mod.name, out int level);
                    if (level >= mod.maxLevel) continue;
                    modLevels[mod.name] = level + 1;
                    _world.Events.Publish(new ModifierChosenEvent { Modifier = mod });
                }
            }

            _world.Events.Subscribe<EntityDiedEvent>(OnEntityDied);
        }

        private void Update()
        {
            _world?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _world?.Events.Unsubscribe<EntityDiedEvent>(OnEntityDied);
            _world?.Destroy();
        }

        private void OnEntityDied(EntityDiedEvent e)
        {
            if (e.Entity != _playerEntity) return;
            TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            PauseManager.Instance.Push();
            _world?.Events.Publish(new GameOverEvent());
            Debug.Log("Game Over!");
        }

        public void Restart()
        {
            IsGameOver = false;
            _world?.Events.Publish(new GameRestartedEvent());
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
