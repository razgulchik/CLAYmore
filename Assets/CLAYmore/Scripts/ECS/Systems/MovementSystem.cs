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
        private Vector2Int _heldDirection;

        public MovementSystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world        = world;
            _damageSystem = world.GetSystem<DamageSystem>();
            world.Events.Subscribe<PlayerMoveInputEvent>(OnMoveInput);
            world.Events.Subscribe<PlayerMoveHeldEvent>(OnMoveHeld);
            world.Events.Subscribe<GamePausedEvent>(OnGamePaused);
        }

        public void Tick(float deltaTime)
        {
            if (_isPaused || _heldDirection == Vector2Int.zero) return;

            Entity playerEntity = GetPlayerEntity();
            if (playerEntity == null) return;

            var movement = playerEntity.Get<MovementComponent>();
            if (movement.IsMoving) return;

            var stats = playerEntity.Has<PlayerStatsComponent>()
                ? playerEntity.Get<PlayerStatsComponent>()
                : null;
            if (stats == null || !stats.HasDash) return;

            OnMoveInput(new PlayerMoveInputEvent { Direction = _heldDirection });
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnGamePaused(GamePausedEvent evt)
        {
            _isPaused = evt.IsPaused;
            if (!_isPaused)
                _heldDirection = Vector2Int.zero;
        }

        private void OnMoveHeld(PlayerMoveHeldEvent evt)
        {
            _heldDirection = evt.Direction;
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

            // ── Valid move ────────────────────────────────────────────────────
            movement.IsMoving = true;
            _world.Events.Publish(new PlayerMoveResultEvent
            {
                Direction = direction,
                Target    = validTarget,
                MoveType  = MoveType.Walk,
            });
        }

        private Entity GetPlayerEntity() => _world.QueryFirst<MovementComponent>();

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

        private Entity GetActiveChestAt(Vector3Int cell)
        {
            foreach (Entity entity in _world.Query<ChestComponent>())
            {
                var chest = entity.Get<ChestComponent>();
                if (chest.State == ChestState.Active && chest.LandCell == cell)
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
