using System;
using UnityEngine;
using CLAYmore.ECS;

namespace CLAYmore
{
    /// <summary>
    /// MonoBehaviour facade over EconomySystem + CoinComponent.
    /// Bootstraps the entity/component, then delegates all logic to EconomySystem.
    /// </summary>
    public class Economy : MonoBehaviour
    {
        [Min(0)] public int startingCoins = 0;

        public int Coins => _entity.Get<CoinComponent>().Coins;

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
            // World is created in GameManager.Awake() — safe to access here.
            _system = World.Current?.GetSystem<EconomySystem>();
            if (_system != null)
                _system.OnChanged += coins => OnChanged?.Invoke(coins);
        }

        public void Add(int amount) => _system?.Add(_entity, amount);

        public bool TrySpend(int amount) => _system != null && _system.TrySpend(_entity, amount);
    }
}
