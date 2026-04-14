using System;
using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade: создаёт CoinComponent-сущность и регистрирует её в World.
    /// Вся логика делегируется EconomySystem, которая сама находит сущность через Query.
    /// </summary>
    public class Economy : MonoBehaviour
    {
        [Min(0)] public int startingCoins = 0;

        public int Coins => _system?.GetCoinCount() ?? 0;

        public event Action<int> OnChanged;

        private Entity _entity;
        private EconomySystem _system;

        private void Awake()
        {
            _entity = gameObject.AddComponent<Entity>();
            _entity.Add(new CoinComponent { Coins = startingCoins, StartingCoins = startingCoins });
        }

        private void Start()
        {
            World.Current?.RegisterEntity(_entity);
            _system = World.Current?.GetSystem<EconomySystem>();
            if (_system != null)
                _system.OnChanged += coins => OnChanged?.Invoke(coins);
            World.Current?.Events.Publish(new CoinBalanceChangedEvent { NewBalance = startingCoins });
        }

        private void OnDestroy()
        {
            World.Current?.UnregisterEntity(_entity);
        }

        public void Add(int amount)      => _system?.Add(amount);
        public bool TrySpend(int amount) => _system?.TrySpend(amount) ?? false;
    }
}
