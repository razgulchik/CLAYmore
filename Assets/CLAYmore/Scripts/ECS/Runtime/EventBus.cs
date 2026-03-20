using System;
using System.Collections.Generic;

namespace CLAYmore.ECS
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // snapshot — safe if a handler unsubscribes during publish
            var snapshot = list.ToArray();
            foreach (var h in snapshot)
                ((Action<T>)h).Invoke(evt);
        }

        public void Clear() => _handlers.Clear();
    }
}
