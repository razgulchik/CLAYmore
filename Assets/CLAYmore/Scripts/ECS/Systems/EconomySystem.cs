using System;
using CLAYmore.ECS;

namespace CLAYmore
{
    public class EconomySystem : ISystem
    {
        public event Action<int> OnChanged;

        private World _world;

        public void Initialize(World world)
        {
            _world = world;
        }

        public void Tick(float deltaTime) { }

        public void Add(Entity entity, int amount)
        {
            if (amount <= 0) return;
            var c = entity.Get<CoinComponent>();
            c.Coins += amount;
            OnChanged?.Invoke(c.Coins);
        }

        public bool TrySpend(Entity entity, int amount)
        {
            var c = entity.Get<CoinComponent>();
            if (c.Coins < amount) return false;
            c.Coins -= amount;
            OnChanged?.Invoke(c.Coins);
            return true;
        }

        public int GetCoins(Entity entity) => entity.Get<CoinComponent>().Coins;
    }
}
