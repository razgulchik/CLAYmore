using System;
using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    [RequireComponent(typeof(Entity))]
    public class PlayerHealth : MonoBehaviour
    {
        [Min(1)] public int maxHp = 3;

        public int Hp => _entity.Get<HealthComponent>().Hp;

        public event Action<int> OnDamaged;
        public event Action OnDied;

        private Entity _entity;
        private HealthSystem _system;

        private void Awake()
        {
            _entity = GetComponent<Entity>();
        }

        private void Start()
        {
            _entity.Add(new HealthComponent { MaxHp = maxHp, Hp = maxHp });
            _system = World.Current?.GetSystem<HealthSystem>();
            var bus = World.Current?.Events;
            if (bus != null)
            {
                bus.Subscribe<EntityDamagedEvent>(OnEntityDamaged);
                bus.Subscribe<EntityDiedEvent>(OnEntityDied);
                bus.Publish(new PlayerHpChangedEvent { Hp = maxHp, MaxHp = maxHp });

                var movement = _entity.Has<MovementComponent>() ? _entity.Get<MovementComponent>() : null;
                bus.Publish(new PlayerStatsChangedEvent
                {
                    MaxHp    = maxHp,
                    Damage   = 1,
                    MoveTime = movement?.MoveTime ?? 0f,
                });
            }
        }

        private void OnDestroy()
        {
            var bus = World.Current?.Events;
            if (bus != null)
            {
                bus.Unsubscribe<EntityDamagedEvent>(OnEntityDamaged);
                bus.Unsubscribe<EntityDiedEvent>(OnEntityDied);
            }
        }

        public void TakeDamage(int amount) => _system?.TakeDamage(_entity, amount);

        public void Heal(int amount)
        {
            _system?.Heal(_entity, amount);
            World.Current?.Events.Publish(new PlayerHpChangedEvent { Hp = Hp, MaxHp = maxHp });
        }

        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (evt.Entity != _entity) return;
            OnDamaged?.Invoke(evt.Hp);
            World.Current?.Events.Publish(new PlayerHpChangedEvent { Hp = evt.Hp, MaxHp = maxHp });
        }

        private void OnEntityDied(EntityDiedEvent evt)
        {
            if (evt.Entity != _entity) return;
            OnDied?.Invoke();
            World.Current?.Events.Publish(new PlayerHpChangedEvent { Hp = 0, MaxHp = maxHp });
        }
    }
}
