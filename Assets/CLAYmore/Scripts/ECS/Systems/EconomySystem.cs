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

        public void Add(int amount)
        {
            if (amount <= 0) return;
            var c = GetCoins();
            if (c == null) return;
            c.Coins += amount;
            OnChanged?.Invoke(c.Coins);
        }

        public bool TrySpend(int amount)
        {
            var c = GetCoins();
            if (c == null || c.Coins < amount) return false;
            c.Coins -= amount;
            OnChanged?.Invoke(c.Coins);
            return true;
        }

        public int GetCoinCount()
        {
            var c = GetCoins();
            return c?.Coins ?? 0;
        }

        private CoinComponent GetCoins()
        {
            foreach (Entity e in _world.Query<CoinComponent>())
                return e.Get<CoinComponent>();
            return null;
        }
    }
}
