using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Handles tile-based movement logic for the player.
    /// Reads player input (PlayerMoveInputEvent), validates the move
    /// against the island grid and landed pots, then publishes
    /// PlayerMoveResultEvent for the View (PlayerMovement) to animate.
    /// </summary>
    public class MovementSystem : ISystem
    {
        private readonly IslandGenerator _island;
        private World _world;
        private DamageSystem _damageSystem;

        public MovementSystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world = world;
            _damageSystem = world.GetSystem<DamageSystem>();
            world.Events.Subscribe<PlayerMoveInputEvent>(OnMoveInput);
        }

        public void Tick(float deltaTime) { }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnMoveInput(PlayerMoveInputEvent evt)
        {
            Entity playerEntity = GetPlayerEntity();
            if (playerEntity == null) return;

            var movement = playerEntity.Get<MovementComponent>();
            if (movement.IsMoving) return;

            Vector2Int direction  = evt.Direction;
            movement.FacingDirection = direction;

            Vector3Int currentCell = _island.GetPlayerCell();
            Vector3Int targetCell  = currentCell + new Vector3Int(direction.x, direction.y, 0);
            Vector3    targetWorld = _island.GetCellCenter(targetCell);

            // ── Pot at target cell? → damage pot, then walk or bounce ─────────
            Entity potEntity = GetLandedPotAt(targetCell);
            if (potEntity != null)
            {
                movement.IsMoving = true;
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
    }
}
