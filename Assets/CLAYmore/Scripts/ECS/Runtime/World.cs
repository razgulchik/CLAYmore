using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CLAYmore.ECS
{
    /// <summary>
    /// Registry of all entities and systems. Acts as the ECS runtime.
    /// Bootstrap creates and owns the World instance.
    /// </summary>
    public class World
    {
        public static World Current { get; private set; }

        public EventBus Events { get; } = new EventBus();

        private readonly List<Entity> _entities = new();
        private readonly List<ISystem> _systems = new();

        public World()
        {
            Current = this;
        }

        // ── Systems ──────────────────────────────────────────────────────────

        public T RegisterSystem<T>(T system) where T : ISystem
        {
            system.Initialize(this);
            _systems.Add(system);
            return system;
        }

        public T GetSystem<T>() where T : class, ISystem
        {
            return _systems.OfType<T>().FirstOrDefault();
        }

        /// <summary>Called every frame from a MonoBehaviour.Update.</summary>
        public void Tick(float deltaTime)
        {
            foreach (var system in _systems)
                system.Tick(deltaTime);
        }

        // ── Entities ─────────────────────────────────────────────────────────

        public void RegisterEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
                _entities.Add(entity);
        }

        public void UnregisterEntity(Entity entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>Returns all live entities that have component T.</summary>
        public IEnumerable<Entity> Query<T>() where T : IComponent
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var e = _entities[i];
                if (e == null) { _entities.RemoveAt(i); continue; }
                if (e.Has<T>()) yield return e;
            }
        }

        public void Destroy()
        {
            _entities.Clear();
            _systems.Clear();
            Events.Clear();
            if (Current == this) Current = null;
        }
    }
}
