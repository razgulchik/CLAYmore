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
        public void Heal(int amount) => _system?.Heal(_entity, amount);

        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (evt.Entity == _entity) OnDamaged?.Invoke(evt.Hp);
        }

        private void OnEntityDied(EntityDiedEvent evt)
        {
            if (evt.Entity == _entity) OnDied?.Invoke();
        }
    }
}
