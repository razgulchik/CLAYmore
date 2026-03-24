using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Handles tick-based ability logic:
    ///   - Shield cooldown recovery
    ///   - Lightning periodic strike
    ///   - OrthoStrike when player arrives at a new tile
    /// </summary>
    public class AbilitySystem : ISystem
    {
        private readonly IslandGenerator _island;
        private World _world;
        private HealthSystem _healthSystem;
        private DamageSystem _damageSystem;

        public AbilitySystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world         = world;
            _healthSystem  = world.GetSystem<HealthSystem>();
            _damageSystem  = world.GetSystem<DamageSystem>();
            world.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
        }

        public void Tick(float deltaTime)
        {
            Entity player = GetPlayerEntity();
            if (player == null) return;

            var stats = player.Get<PlayerStatsComponent>();

            // ── Shield cooldown ───────────────────────────────────────────────
            if (stats.ShieldCooldown > 0f)
            {
                stats.ShieldCooldown -= deltaTime;
                if (stats.ShieldCooldown <= 0f)
                {
                    stats.ShieldCooldown = 0f;
                    stats.ShieldCurrent  = stats.ShieldMax;
                    _world.Events.Publish(new PlayerStatsChangedEvent());
                }
            }

            // ── Lightning strike ──────────────────────────────────────────────
            if (stats.HasLightning && stats.LightningInterval > 0f)
            {
                stats.LightningTimer -= deltaTime;
                if (stats.LightningTimer <= 0f)
                {
                    stats.LightningTimer = stats.LightningInterval;
                    TriggerLightning();
                }
            }
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnPlayerTileChanged(PlayerTileChangedEvent evt)
        {
            Entity player = GetPlayerEntity();
            if (player == null) return;

            var stats = player.Get<PlayerStatsComponent>();
            if (!stats.HasOrthoStrike) return;

            Vector3Int playerCell = _island.GetPlayerCell();
            Vector3Int[] neighbours =
            {
                playerCell + Vector3Int.right,
                playerCell + Vector3Int.left,
                playerCell + Vector3Int.up,
                playerCell + Vector3Int.down,
            };

            foreach (Vector3Int cell in neighbours)
            {
                Entity pot = GetLandedPotAt(cell);
                if (pot != null)
                    _damageSystem.PlayerHitPot(pot);
            }
        }

        private void TriggerLightning()
        {
            // Collect all landed pots, pick one at random
            var candidates = new System.Collections.Generic.List<Entity>();
            foreach (Entity e in _world.Query<PotComponent>())
            {
                if (e.Get<PotComponent>().State == PotState.Landed)
                    candidates.Add(e);
            }

            if (candidates.Count == 0) return;

            Entity  target   = candidates[Random.Range(0, candidates.Count)];
            Vector3 worldPos = _island.GetCellCenter(target.Get<PotComponent>().LandCell);
            var h = target.Get<HealthComponent>();
            _healthSystem.TakeDamage(target, h.Hp);
            _world.Events.Publish(new LightningStrikeEvent { Target = target, WorldPosition = worldPos });
        }

        private Entity GetPlayerEntity()
        {
            foreach (Entity e in _world.Query<PlayerStatsComponent>())
                return e;
            return null;
        }

        private Entity GetLandedPotAt(Vector3Int cell)
        {
            foreach (Entity e in _world.Query<PotComponent>())
            {
                var pot = e.Get<PotComponent>();
                if (pot.State == PotState.Landed && pot.LandCell == cell)
                    return e;
            }
            return null;
        }
    }
}
