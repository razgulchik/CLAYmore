using CLAYmore.ECS;

namespace CLAYmore
{
    public class ExpanderSystem : ISystem
    {
        private World _world;

        public void Initialize(World world) => _world = world;
        public void Tick(float deltaTime) { }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            var c = GetExpanders();
            if (c == null) return;
            c.Expanders += amount;
            _world.Events.Publish(new ExpanderBalanceChangedEvent { NewBalance = c.Expanders });
        }

        public bool TrySpend(int amount)
        {
            var c = GetExpanders();
            if (c == null || c.Expanders < amount) return false;
            c.Expanders -= amount;
            _world.Events.Publish(new ExpanderBalanceChangedEvent { NewBalance = c.Expanders });
            return true;
        }

        public int GetCount()
        {
            var c = GetExpanders();
            return c?.Expanders ?? 0;
        }

        private ExpanderComponent GetExpanders()
        {
            foreach (Entity e in _world.Query<ExpanderComponent>())
                return e.Get<ExpanderComponent>();
            return null;
        }
    }
}
