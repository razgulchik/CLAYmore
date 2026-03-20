using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Drives spawn timing logic for all entities with a SpawnerComponent.
    /// Actual GameObject instantiation stays in PotSpawner (needs Unity APIs).
    /// </summary>
    public class SpawnerSystem : ISystem
    {
        private World _world;

        public void Initialize(World world)
        {
            _world = world;
        }

        public void Tick(float deltaTime)
        {
            foreach (var entity in _world.Query<SpawnerComponent>())
            {
                var s = entity.Get<SpawnerComponent>();
                s.CurrentInterval = Mathf.Max(s.MinInterval,
                    s.CurrentInterval - s.IntervalDecreasePerSecond * deltaTime);

                s.Timer -= deltaTime;
                if (s.Timer <= 0f)
                {
                    _world.Events.Publish(new SpawnRequestedEvent { SpawnerEntity = entity });
                    s.Timer = s.CurrentInterval;
                }
            }
        }
    }
}
