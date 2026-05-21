using System;
using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade: создаёт сущности для монет и расширителей, регистрирует их в World.
    /// Вся логика делегируется EconomySystem и ExpanderSystem.
    /// </summary>
    public class Economy : MonoBehaviour
    {
        public int Coins     => _economySystem?.GetCoinCount() ?? 0;
        public int Expanders => _expanderSystem?.GetCount()    ?? 0;

        public event Action<int> OnChanged;

        private int _startingCoins;

        private Entity _coinEntity;
        private Entity _expanderEntity;

        private EconomySystem _economySystem;
        private ExpanderSystem _expanderSystem;

        public void Init(int startingCoins) => _startingCoins = startingCoins;

        private void Awake()
        {
            _coinEntity = gameObject.AddComponent<Entity>();
            _coinEntity.Add(new CoinComponent { Coins = _startingCoins, StartingCoins = _startingCoins });

            _expanderEntity = gameObject.AddComponent<Entity>();
            _expanderEntity.Add(new ExpanderComponent());
        }

        private void Start()
        {
            World.Current?.RegisterEntity(_coinEntity);
            World.Current?.RegisterEntity(_expanderEntity);

            _economySystem = World.Current?.GetSystem<EconomySystem>();
            _expanderSystem = World.Current?.GetSystem<ExpanderSystem>();

            if (_economySystem != null)
                _economySystem.OnChanged += coins => OnChanged?.Invoke(coins);

            World.Current?.Events.Publish(new CoinBalanceChangedEvent { NewBalance = _startingCoins });
            World.Current?.Events.Publish(new ExpanderBalanceChangedEvent { NewBalance = 0 });
        }

        private void OnDestroy()
        {
            World.Current?.UnregisterEntity(_coinEntity);
            World.Current?.UnregisterEntity(_expanderEntity);
        }

        public void Add(int amount)                   => _economySystem?.Add(amount);
        public bool TrySpend(int amount)              => _economySystem?.TrySpend(amount) ?? false;
        public void AddExpanders(int amount)          => _expanderSystem?.Add(amount);
        public bool TrySpendExpanders(int amount)     => _expanderSystem?.TrySpend(amount) ?? false;
    }
}
