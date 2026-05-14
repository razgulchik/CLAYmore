using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Handles contact damage between the player and pots.
    /// Two scenarios:
    ///   1. Player walks into a landed pot → pot loses HP.
    ///   2. A pot lands on the player's cell → player loses HP, pot breaks.
    /// </summary>
    public class DamageSystem : ISystem
    {
        private readonly IslandGenerator _island;
        private World _world;
        private HealthSystem _healthSystem;

        public DamageSystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world = world;
            _healthSystem = world.GetSystem<HealthSystem>();
            world.Events.Subscribe<PotLandedEvent>(OnPotLanded);
            world.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
            world.Events.Subscribe<PlayerLandedEvent>(OnPlayerLanded);
        }

        public void Tick(float deltaTime) { }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by MovementSystem when the player attempts to enter a pot's cell.
        /// Returns true if the pot was destroyed (player may enter the cell).
        /// </summary>
        /// <param name="flatDamage">When > 0, bypasses player DamageBonus and deals exactly this much.</param>
        public bool PlayerHitPot(Entity potEntity, int flatDamage = 0)
        {
            if (potEntity.Get<PotComponent>().Config.isRock) return false;

            int dmg;
            if (flatDamage > 0)
            {
                dmg = flatDamage;
            }
            else
            {
                dmg = 1;
                Entity player = GetPlayerEntity();
                if (player != null && player.Has<PlayerStatsComponent>())
                    dmg += player.Get<PlayerStatsComponent>().DamageBonus;
            }

            _healthSystem.TakeDamage(potEntity, dmg);
            return potEntity.Get<HealthComponent>().Hp <= 0;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnPotLanded(PotLandedEvent evt)
        {
            var pot = evt.PotEntity.Get<PotComponent>();
            if (_island.GetPlayerCell() != pot.LandCell) return;

            // Player is on the landing cell — deal damage and force-break the pot
            Entity playerEntity = GetPlayerEntity();
            if (playerEntity == null) return;

            // If the player started moving away before the pot landed, let them escape.
            // OnPlayerTileChanged handles the inverse case (player walks into a pot that already landed).
            if (playerEntity.Get<MovementComponent>().IsMoving) return;

            if (!TryAbsorbWithShield(playerEntity))
                _healthSystem.TakeDamage(playerEntity, 1);

            // Drain remaining HP to trigger EntityDiedEvent → Pot.BreakVisual (skip rocks)
            if (!pot.Config.isRock)
            {
                var h = evt.PotEntity.Get<HealthComponent>();
                _healthSystem.TakeDamage(evt.PotEntity, h.Hp);
            }
        }

        private void OnPlayerTileChanged(PlayerTileChangedEvent evt) => CheckPotOnPlayerCell();

        // Fires when movement animation ends, including Bounce (player returns to the same cell).
        // PlayerTileChangedEvent is NOT published for Bounce, so this catches the gap.
        private void OnPlayerLanded(PlayerLandedEvent evt) => CheckPotOnPlayerCell();

        private void CheckPotOnPlayerCell()
        {
            Vector3Int playerCell = _island.GetPlayerCell();
            foreach (Entity entity in _world.Query<PotComponent>())
            {
                var pot = entity.Get<PotComponent>();
                if (pot.State != PotState.Landed || pot.LandCell != playerCell) continue;

                Entity playerEntity = GetPlayerEntity();
                if (playerEntity != null && !TryAbsorbWithShield(playerEntity))
                    _healthSystem.TakeDamage(playerEntity, 1);

                if (!pot.Config.isRock)
                {
                    var h = entity.Get<HealthComponent>();
                    _healthSystem.TakeDamage(entity, h.Hp);
                }
                return;
            }
        }

        private bool TryAbsorbWithShield(Entity playerEntity)
        {
            if (!playerEntity.Has<PlayerStatsComponent>()) return false;
            var stats = playerEntity.Get<PlayerStatsComponent>();
            if (stats.ShieldCurrent <= 0 || stats.ShieldCooldown > 0f) return false;

            stats.ShieldCurrent--;
            if (stats.ShieldCurrent <= 0)
                stats.ShieldCooldown = stats.ShieldCooldownMax;

            _world.Events.Publish(new ShieldAbsorbedEvent());
            return true;
        }

        private Entity GetPlayerEntity() => _world.QueryFirst<MovementComponent>();
    }
}
