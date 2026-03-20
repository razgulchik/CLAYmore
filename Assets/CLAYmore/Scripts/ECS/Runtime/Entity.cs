using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLAYmore.ECS
{
    /// <summary>
    /// MonoBehaviour container for ECS components.
    /// Attach this to any GameObject that participates in the ECS world.
    /// </summary>
    public class Entity : MonoBehaviour
    {
        private readonly Dictionary<Type, IComponent> _components = new();

        public T Add<T>(T component) where T : IComponent
        {
            _components[typeof(T)] = component;
            return component;
        }

        public T Get<T>() where T : IComponent
        {
            return (T)_components[typeof(T)];
        }

        public bool Has<T>() where T : IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool TryGet<T>(out T component) where T : IComponent
        {
            if (_components.TryGetValue(typeof(T), out var raw))
            {
                component = (T)raw;
                return true;
            }
            component = default;
            return false;
        }

        public void Remove<T>() where T : IComponent
        {
            _components.Remove(typeof(T));
        }
    }
}
