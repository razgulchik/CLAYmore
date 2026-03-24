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
        public IslandGenerator islandGenerator;
        public PotSpawner      potSpawner;
        public ChestSpawner    chestSpawner;
        public HUD             hud;

        [Header("Camera")]
        public CinemachineCamera virtualCamera;

        [Header("Player")]
        public GameObject playerPrefab;

        [Header("Pools")]
        public PrefabPool potPool;
        public PrefabPool shadowPool;
        public PrefabPool chestPool;

        [Header("Chest / Modifiers")]
        public ModifierConfig[] modifierPool;
        public ModifierChoiceUI modifierChoiceUI;

        public bool IsGameOver { get; private set; }

        private World _world;
        private PlayerHealth _playerHealth;

        private void Awake()
        {
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
            if (config != null)
            {
                if (playerHealth   != null) playerHealth.maxHp        = config.playerMaxHp;
                if (playerMovement != null)
                {
                    playerMovement.moveTime   = config.moveTime;
                    playerMovement.bounceTime = config.bounceTime;
                }
                if (potSpawner != null)
                {
                    potSpawner.initialInterval           = config.spawnInitialInterval;
                    potSpawner.minInterval               = config.spawnMinInterval;
                    potSpawner.intervalDecreasePerSecond = config.spawnIntervalDecreasePerSecond;
                    potSpawner.targetedSpawnEvery        = config.targetedSpawnEvery;
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
            }

            if (chestSpawner != null)
            {
                chestSpawner.islandGenerator = islandGenerator;
                chestSpawner.bootstrap       = this;
                chestSpawner.chestPool       = chestPool;
            }

            if (hud != null)
                hud.Setup(playerHealth);

            if (modifierChoiceUI != null && modifierPool != null)
                modifierChoiceUI.modifierPool = modifierPool;

            if (playerHealth != null)
            {
                playerHealth.OnDied += OnPlayerDied;
                _playerHealth = playerHealth;
            }
        }

        private void Update()
        {
            _world?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDied -= OnPlayerDied;
            _world?.Destroy();
        }

        private void OnPlayerDied()
        {
            IsGameOver = true;
            Debug.Log("Game Over!");
            // TODO: show Game Over UI, freeze input
        }

        public void Restart()
        {
            IsGameOver = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
