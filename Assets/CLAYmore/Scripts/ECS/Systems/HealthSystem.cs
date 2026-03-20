using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class HealthSystem : ISystem
    {
        private World _world;

        public void Initialize(World world)
        {
            _world = world;
        }

        public void Tick(float deltaTime) { }

        public void TakeDamage(Entity entity, int amount)
        {
            var h = entity.Get<HealthComponent>();
            if (h.Hp <= 0) return;
            h.Hp = Mathf.Max(0, h.Hp - amount);
            _world.Events.Publish(new EntityDamagedEvent { Entity = entity, Hp = h.Hp });
            if (h.Hp == 0)
                _world.Events.Publish(new EntityDiedEvent { Entity = entity });
        }

        public void Heal(Entity entity, int amount)
        {
            var h = entity.Get<HealthComponent>();
            h.Hp = Mathf.Min(h.MaxHp, h.Hp + amount);
            _world.Events.Publish(new EntityDamagedEvent { Entity = entity, Hp = h.Hp });
        }

        public int GetHp(Entity entity) => entity.Get<HealthComponent>().Hp;
    }
}
