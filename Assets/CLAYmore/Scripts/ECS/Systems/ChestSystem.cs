using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Detects when the player steps onto a chest cell and publishes ChestActivatedEvent.
    /// </summary>
    public class ChestSystem : ISystem
    {
        private readonly IslandGenerator _island;
        private World _world;

        public ChestSystem(IslandGenerator island)
        {
            _island = island;
        }

        public void Initialize(World world)
        {
            _world = world;
            world.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerTileChanged);
        }

        public void Tick(float deltaTime) { }

        private void OnPlayerTileChanged(PlayerTileChangedEvent evt)
        {
            Vector3Int playerCell = _island.GetPlayerCell();
            foreach (Entity entity in _world.Query<ChestComponent>())
            {
                var chest = entity.Get<ChestComponent>();
                if (chest.State != ChestState.Active || chest.LandCell != playerCell) continue;

                chest.State = ChestState.Opened;
                _world.Events.Publish(new ChestActivatedEvent { ChestEntity = entity });
                return;
            }
        }
    }
}
