using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
    /// <summary>
    /// View: renders the chest sprite and plays the open animation.
    /// State is stored in ChestComponent; no game logic lives here.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Entity))]
    public class Chest : MonoBehaviour
    {
        private ChestComponent _chest;
        private Entity _entity;
        private SpriteRenderer _renderer;
        private PrefabPool _chestPool;

        // ── Init ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by ChestSpawner on every spawn (including re-use from pool).
        /// </summary>
        public void Initialize(ChestConfig config, Vector3 worldPos, Tilemap tilemap, PrefabPool chestPool)
        {
            _chestPool = chestPool;

            _entity = GetComponent<Entity>();

            if (_entity.Has<ChestComponent>())
                _chest = _entity.Get<ChestComponent>();
            else
                _chest = _entity.Add(new ChestComponent());

            _chest.Config   = config;
            _chest.State    = ChestState.Active;
            _chest.LandPos  = worldPos;
            _chest.LandCell = tilemap.WorldToCell(worldPos);

            _renderer = GetComponent<SpriteRenderer>();
            if (config.sprite != null)
                _renderer.sprite = config.sprite;

            World.Current?.RegisterEntity(_entity);
            World.Current?.Events.Subscribe<ChestActivatedEvent>(OnChestActivated);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnChestActivated(ChestActivatedEvent evt)
        {
            if (evt.ChestEntity != _entity) return;

            World.Current?.Events.Unsubscribe<ChestActivatedEvent>(OnChestActivated);
            World.Current?.UnregisterEntity(_entity);

            // Simple open: hide sprite, return to pool after a short delay
            _renderer.enabled = false;
            Invoke(nameof(ReturnToPool), 0.5f);
        }

        private void ReturnToPool()
        {
            _chestPool?.Return(gameObject);
        }
    }
}
