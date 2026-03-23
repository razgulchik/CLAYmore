using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Handles tile-based movement logic for the player.
    /// Reads player input (PlayerMoveInputEvent), validates the move
    /// against the island grid and landed pots, then publishes
    /// PlayerMoveResultEvent for the View (PlayerMovement) to animate.
    ///
    /// Abilities handled here:
    ///   - Dash: keep moving in direction until hitting a wall or pot
    ///   - AoeStrike: also hit the two side cells when attacking a pot
    ///   - GamePaused: ignore input while paused (e.g. modifier choice screen)
    /// </summary>
    public class MovementSystem : ISystem
    {
        private readonly IslandGenerator _island;
        private World _world;
        private DamageSystem _damageSystem;
        private bool _isPaused;

        public MovementSystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world        = world;
            _damageSystem = world.GetSystem<DamageSystem>();
            world.Events.Subscribe<PlayerMoveInputEvent>(OnMoveInput);
            world.Events.Subscribe<GamePausedEvent>(OnGamePaused);
        }

        public void Tick(float deltaTime) { }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnGamePaused(GamePausedEvent evt)
        {
            _isPaused = evt.IsPaused;
        }

        private void OnMoveInput(PlayerMoveInputEvent evt)
        {
            Entity playerEntity = GetPlayerEntity();
            if (playerEntity == null) return;

            var movement = playerEntity.Get<MovementComponent>();
            if (_isPaused || movement.IsMoving) return;

            Vector2Int direction  = evt.Direction;
            movement.FacingDirection = direction;

            var stats = playerEntity.Has<PlayerStatsComponent>()
                ? playerEntity.Get<PlayerStatsComponent>()
                : null;

            Vector3Int currentCell = _island.GetPlayerCell();
            Vector3Int targetCell  = currentCell + new Vector3Int(direction.x, direction.y, 0);
            Vector3    targetWorld = _island.GetCellCenter(targetCell);

            // ── Pot at target cell? → damage pot, then walk or bounce ─────────
            Entity potEntity = GetLandedPotAt(targetCell);
            if (potEntity != null)
            {
                movement.IsMoving = true;

                // AoeStrike: also hit the two perpendicular side cells
                if (stats != null && stats.HasAoeStrike)
                {
                    foreach (Vector3Int sideCell in GetSideCells(targetCell, direction))
                    {
                        Entity sidePot = GetLandedPotAt(sideCell);
                        if (sidePot != null)
                            _damageSystem.PlayerHitPot(sidePot);
                    }
                }

                bool potDied = _damageSystem.PlayerHitPot(potEntity);
                _world.Events.Publish(new PlayerMoveResultEvent
                {
                    Direction = direction,
                    Target    = targetWorld,
                    MoveType  = potDied ? MoveType.Walk : MoveType.Bounce,
                });
                return;
            }

            // ── Check walkability ─────────────────────────────────────────────
            Vector3 currentWorld = _island.GetCellCenter(currentCell);
            Vector3 validTarget  = _island.TryMove(currentWorld, direction);

            if (Vector3.Distance(validTarget, currentWorld) < 0.01f)
            {
                // Wall or water — blocked, no animation
                _world.Events.Publish(new PlayerMoveResultEvent
                {
                    Direction = direction,
                    MoveType  = MoveType.Blocked,
                });
                return;
            }

            // ── Dash: slide until hitting a wall, water, or pot ───────────────
            if (stats != null && stats.HasDash)
            {
                Vector3Int dashCell   = currentCell + new Vector3Int(direction.x, direction.y, 0);
                Entity     dashHitPot = null;

                while (true)
                {
                    Vector3 dashWorld = _island.GetCellCenter(dashCell);

                    // Island edge (water/outside) — stop here, no expansion
                    if (_island.IsBlockedByEdge(dashWorld, direction)) break;

                    Vector3Int nextCell = dashCell + new Vector3Int(direction.x, direction.y, 0);

                    // Next cell has a pot — stop and hit it
                    dashHitPot = GetLandedPotAt(nextCell);
                    if (dashHitPot != null) break;

                    dashCell = nextCell;
                }

                validTarget = _island.GetCellCenter(dashCell);

                // Dash ended on a pot — deal damage and return
                if (dashHitPot != null)
                {
                    Vector3Int potCell = dashCell + new Vector3Int(direction.x, direction.y, 0);

                    if (stats.HasAoeStrike)
                    {
                        foreach (Vector3Int sideCell in GetSideCells(potCell, direction))
                        {
                            Entity sidePot = GetLandedPotAt(sideCell);
                            if (sidePot != null)
                                _damageSystem.PlayerHitPot(sidePot);
                        }
                    }

                    movement.IsMoving = true;
                    bool potDied = _damageSystem.PlayerHitPot(dashHitPot);
                    _world.Events.Publish(new PlayerMoveResultEvent
                    {
                        Direction = direction,
                        Target    = _island.GetCellCenter(potCell),
                        MoveType  = potDied ? MoveType.Walk : MoveType.Bounce,
                    });
                    return;
                }
            }

            // ── Valid move ────────────────────────────────────────────────────
            movement.IsMoving = true;
            _world.Events.Publish(new PlayerMoveResultEvent
            {
                Direction = direction,
                Target    = validTarget,
                MoveType  = MoveType.Walk,
            });
        }

        private Entity GetPlayerEntity()
        {
            foreach (Entity entity in _world.Query<MovementComponent>())
                return entity;
            return null;
        }

        private Entity GetLandedPotAt(Vector3Int cell)
        {
            foreach (Entity entity in _world.Query<PotComponent>())
            {
                var pot = entity.Get<PotComponent>();
                if (pot.State == PotState.Landed && pot.LandCell == cell)
                    return entity;
            }
            return null;
        }

        /// <summary>
        /// Returns the two cells perpendicular to the movement direction,
        /// adjacent to the target cell (for AoeStrike).
        /// </summary>
        private static Vector3Int[] GetSideCells(Vector3Int target, Vector2Int dir)
        {
            Vector3Int perp = dir.x != 0
                ? new Vector3Int(0, 1, 0)
                : new Vector3Int(1, 0, 0);
            return new[] { target + perp, target - perp };
        }
    }
}
