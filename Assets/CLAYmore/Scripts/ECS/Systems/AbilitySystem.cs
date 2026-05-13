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
            public bool   SpawnBallLightningOnKill;
        }

        private struct BurningCell
        {
            public Vector2Int Cell;
            public int        Damage;
            public float      TimeRemaining;
            public float      DamageCooldown;
        }

        private readonly IslandGenerator       _island;
        private readonly System.Collections.Generic.List<PendingImpact>         _pendingImpacts  = new();
        private readonly System.Collections.Generic.List<BurningCell>           _burningCells    = new();
        private readonly System.Collections.Generic.Dictionary<Vector2Int, Vector3> _ballLightnings = new();
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
            world.Events.Subscribe<BallLightningExpiredEvent>(OnBallLightningExpired);
            world.Events.Subscribe<BallLightningDetonateEvent>(OnBallLightningDetonate);
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

            // ── Fire trail ticks ──────────────────────────────────────────────
            for (int i = _burningCells.Count - 1; i >= 0; i--)
            {
                var cell = _burningCells[i];
                cell.TimeRemaining  -= deltaTime;
                cell.DamageCooldown -= deltaTime;

                if (cell.DamageCooldown <= 0f)
                {
                    cell.DamageCooldown += 1f;
                    Entity pot = GetLandedPotAt(new Vector3Int(cell.Cell.x, cell.Cell.y, 0));
                    if (pot != null)
                        _damageSystem.PlayerHitPot(pot, cell.Damage);
                    CheckBallLightningAt(cell.Cell);
                }

                if (cell.TimeRemaining <= 0f)
                    _burningCells.RemoveAt(i);
                else
                    _burningCells[i] = cell;
            }

            // ── Pending lightning impacts ─────────────────────────────────────
            for (int i = _pendingImpacts.Count - 1; i >= 0; i--)
            {
                var p = _pendingImpacts[i];
                p.Timer -= deltaTime;
                if (p.Timer <= 0f)
                {
                    bool killed = _damageSystem.PlayerHitPot(p.Target, p.Damage);
                    if (killed && p.SpawnBallLightningOnKill)
                    {
                        Vector3Int lc = p.Target.Get<PotComponent>().LandCell;
                        SpawnBallLightning(new Vector2Int(lc.x, lc.y), _island.GetCellCenter(lc));
                    }
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
                    CheckBallLightningAt(new Vector2Int(cell.x, cell.y));
                }

                _world.Events.Publish(new OrthoStrikeEvent
                {
                    Origin            = _island.GetCellCenter(originCell),
                    MovedHorizontally = movedHorizontally,
                });
            }

            // ── Fire Trail ────────────────────────────────────────────────────
            if (stats.HasFireTrail && evt.OldIndex.x != int.MinValue)
            {
                bool alreadyBurning = false;
                for (int i = 0; i < _burningCells.Count; i++)
                {
                    if (_burningCells[i].Cell == evt.OldIndex)
                    {
                        alreadyBurning = true;
                        break;
                    }
                }

                if (!alreadyBurning)
                {
                    _burningCells.Add(new BurningCell
                    {
                        Cell           = evt.OldIndex,
                        Damage         = stats.FireTrailDamage,
                        TimeRemaining  = 10f,
                        DamageCooldown = 1f,
                    });

                    _world.Events.Publish(new ECS.FireTrailEvent
                    {
                        WorldPosition = _island.GetCellCenter(new Vector3Int(evt.OldIndex.x, evt.OldIndex.y, 0)),
                        Cell          = evt.OldIndex,
                    });
                }
            }

            // ── Shockwave ─────────────────────────────────────────────────────
            if (stats.HasShockwave)
            {
                stats.ShockwaveStepCount++;
                if (stats.ShockwaveStepCount >= stats.ShockwaveStepsRequired)
                {
                    stats.ShockwaveStepCount = 0;
                    TriggerShockwave(stats, evt);
                }
            }

            // ── Ball lightning — player steps onto cell ───────────────────────
            CheckBallLightningAt(evt.NewIndex);
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

            bool spawnBall = stats != null && stats.HasBallLightning;
            if (delay <= 0f)
            {
                bool killed = _damageSystem.PlayerHitPot(target, dmg);
                if (killed && spawnBall)
                {
                    Vector3Int lc = target.Get<PotComponent>().LandCell;
                    SpawnBallLightning(new Vector2Int(lc.x, lc.y), worldPos);
                }
            }
            else
            {
                _pendingImpacts.Add(new PendingImpact
                {
                    Target                 = target,
                    Damage                 = dmg,
                    Timer                  = delay,
                    SpawnBallLightningOnKill = spawnBall,
                });
            }
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
                CheckBallLightningAt(new Vector2Int(waveCells[i].x, waveCells[i].y));
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

        // ── Ball Lightning ────────────────────────────────────────────────────

        public void SpawnBallLightning(Vector2Int cell, Vector3 worldPos)
        {
            if (_ballLightnings.ContainsKey(cell)) return;
            if (!_island.MarkBallLightning(worldPos)) return;
            _ballLightnings[cell] = worldPos;
            _world.Events.Publish(new BallLightningSpawnedEvent { Cell = cell, WorldPosition = worldPos });
        }

        public void CheckBallLightningAt(Vector2Int cell)
        {
            if (!_ballLightnings.TryGetValue(cell, out Vector3 worldPos)) return;
            TriggerBallLightningExplosion(cell, worldPos);
        }

        private void TriggerBallLightningExplosion(Vector2Int cell, Vector3 worldPos)
        {
            _ballLightnings.Remove(cell);
            _island.ClearBallLightning(worldPos);
            _world.Events.Publish(new BallLightningExplodedEvent { Cell = cell, WorldPosition = worldPos });
        }

        private void OnBallLightningDetonate(BallLightningDetonateEvent evt)
        {
            Entity player = GetPlayerEntity();
            PlayerStatsComponent stats = player != null ? player.Get<PlayerStatsComponent>() : null;
            int dmg    = stats != null ? stats.BallLightningDamage : 1;
            int radius = stats != null ? stats.BallLightningRadius : 1;

            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                var target = new Vector2Int(evt.Cell.x + dx, evt.Cell.y + dy);
                Entity pot = GetLandedPotAt(new Vector3Int(target.x, target.y, 0));
                if (pot != null)
                    _damageSystem.PlayerHitPot(pot, dmg);

                CheckBallLightningAt(target);
            }
        }

        private void OnBallLightningExpired(BallLightningExpiredEvent evt)
        {
            if (_ballLightnings.TryGetValue(evt.Cell, out Vector3 worldPos))
                TriggerBallLightningExplosion(evt.Cell, worldPos);
        }
    }
}
