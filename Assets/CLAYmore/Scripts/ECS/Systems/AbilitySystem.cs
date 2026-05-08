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
        private struct PendingImpact
        {
            public Entity Target;
            public int    Damage;
            public float  Timer;
        }

        private readonly IslandGenerator       _island;
        private readonly System.Collections.Generic.List<PendingImpact> _pendingImpacts = new();
        private World        _world;
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
                    PlayerStatsPublisher.Publish(_world);
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

            // ── Pending lightning impacts ─────────────────────────────────────
            for (int i = _pendingImpacts.Count - 1; i >= 0; i--)
            {
                var p = _pendingImpacts[i];
                p.Timer -= deltaTime;
                if (p.Timer <= 0f)
                {
                    _damageSystem.PlayerHitPot(p.Target, p.Damage);
                    _pendingImpacts.RemoveAt(i);
                }
                else
                {
                    _pendingImpacts[i] = p;
                }
            }
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnPlayerTileChanged(PlayerTileChangedEvent evt)
        {
            Entity player = GetPlayerEntity();
            if (player == null) return;

            var stats = player.Get<PlayerStatsComponent>();

            // ── FireBalls ─────────────────────────────────────────────────────
            if (stats.HasFireBalls)
            {
                Vector2Int move = evt.NewIndex - evt.OldIndex;
                bool movedHorizontally = Mathf.Abs(move.x) >= Mathf.Abs(move.y);

                Vector3Int originCell = new Vector3Int(evt.OldIndex.x, evt.OldIndex.y, 0);
                Vector3Int[] perpendicular = movedHorizontally
                    ? new[] { originCell + Vector3Int.up,    originCell + Vector3Int.down }
                    : new[] { originCell + Vector3Int.right, originCell + Vector3Int.left };

                foreach (Vector3Int cell in perpendicular)
                {
                    Entity pot = GetLandedPotAt(cell);
                    if (pot != null)
                        _damageSystem.PlayerHitPot(pot, stats.FireBallsDamage);
                }

                _world.Events.Publish(new OrthoStrikeEvent
                {
                    Origin            = _island.GetCellCenter(originCell),
                    MovedHorizontally = movedHorizontally,
                });
            }

            // ── Shockwave ─────────────────────────────────────────────────────
            if (stats.HasShockwave)
            {
                stats.ShockwaveStepCount++;
                if (stats.ShockwaveStepCount >= 3)
                {
                    stats.ShockwaveStepCount = 0;
                    TriggerShockwave(stats, evt);
                }
            }
        }

        private void TriggerLightning()
        {
            var candidates = new System.Collections.Generic.List<Entity>();
            foreach (Entity e in _world.Query<PotComponent>())
            {
                var pot = e.Get<PotComponent>();
                if (pot.State == PotState.Landed && !pot.Config.isRock)
                    candidates.Add(e);
            }

            if (candidates.Count == 0) return;

            Entity  target   = candidates[Random.Range(0, candidates.Count)];
            Vector3 worldPos = _island.GetCellCenter(target.Get<PotComponent>().LandCell);
            Entity               player = GetPlayerEntity();
            PlayerStatsComponent stats  = player != null ? player.Get<PlayerStatsComponent>() : null;
            int   dmg   = stats != null ? stats.LightningDamage    : 1;
            float delay = stats != null ? stats.LightningImpactDelay : 0f;

            _world.Events.Publish(new LightningStrikeEvent { Target = target, WorldPosition = worldPos });

            if (delay <= 0f)
                _damageSystem.PlayerHitPot(target, dmg);
            else
                _pendingImpacts.Add(new PendingImpact { Target = target, Damage = dmg, Timer = delay });
        }

        private const float ShockwaveFrameInterval = 0.1f;

        private void TriggerShockwave(PlayerStatsComponent stats, PlayerTileChangedEvent evt)
        {
            Vector2Int move = evt.NewIndex - evt.OldIndex;
            Vector2Int dir  = new Vector2Int(
                move.x != 0 ? (int)Mathf.Sign(move.x) : 0,
                move.y != 0 ? (int)Mathf.Sign(move.y) : 0);

            if (dir == Vector2Int.zero) return;

            var waveCells     = new System.Collections.Generic.List<Vector3Int>();
            Vector3Int probe  = new Vector3Int(evt.NewIndex.x, evt.NewIndex.y, 0);

            while (!_island.IsBlockedByEdge(_island.GetCellCenter(probe), dir))
            {
                probe += new Vector3Int(dir.x, dir.y, 0);
                waveCells.Add(probe);
            }

            if (waveCells.Count == 0) return;

            var positions = new Vector3[waveCells.Count];
            var hadPot    = new bool[waveCells.Count];

            for (int i = 0; i < waveCells.Count; i++)
            {
                positions[i] = _island.GetCellCenter(waveCells[i]);
                Entity pot   = GetLandedPotAt(waveCells[i]);
                hadPot[i]    = pot != null;
                if (pot != null)
                    _pendingImpacts.Add(new PendingImpact
                    {
                        Target = pot,
                        Damage = stats.ShockwaveDamage,
                        Timer  = i * ShockwaveFrameInterval,
                    });
            }

            _world.Events.Publish(new ShockwaveEvent
            {
                TilePositions = positions,
                HadPot        = hadPot,
                Direction     = dir,
            });
        }

        private Entity GetPlayerEntity() => _world.QueryFirst<PlayerStatsComponent>();

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
