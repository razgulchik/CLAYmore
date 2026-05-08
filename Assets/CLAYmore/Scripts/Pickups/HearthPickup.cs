using DG.Tweening;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class HearthPickup : MonoBehaviour
    {
        [SerializeField] private float _spawnHeight  = 8f;
        [SerializeField] private float _fallDuration = 0.8f;

        private Vector2Int      _tileIndex;
        private IslandGenerator _islandGenerator;
        private PrefabPool      _hearthPool;
        private bool            _landed;
        private bool            _collected;

        public void Initialize(Vector3 landPos, IslandGenerator islandGenerator, PrefabPool hearthPool)
        {
            _islandGenerator = islandGenerator;
            _hearthPool      = hearthPool;
            _landed          = false;
            _collected       = false;

            var cell   = islandGenerator.tilemap.WorldToCell(landPos);
            _tileIndex = new Vector2Int(cell.x, cell.y);

            transform.position = landPos + Vector3.up * _spawnHeight;

            transform.DOKill();
            transform.DOMove(landPos, _fallDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(OnLanded);
        }

        private void OnLanded()
        {
            _landed = true;
            transform.DOKill();
            World.Current?.Events.Subscribe<PlayerTileChangedEvent>(OnPlayerMoved);
        }

        private void OnPlayerMoved(PlayerTileChangedEvent evt)
        {
            if (!_landed || _collected) return;
            if (evt.NewIndex != _tileIndex) return;
            Collect();
        }

        private void Collect()
        {
            _collected = true;
            World.Current?.Events.Unsubscribe<PlayerTileChangedEvent>(OnPlayerMoved);
            World.Current?.Events.Publish(new HearthCollectedEvent());
            _islandGenerator?.ClearCell(transform.position);
            _hearthPool?.Return(gameObject);
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<PlayerTileChangedEvent>(OnPlayerMoved);
            if (_landed && !_collected && _islandGenerator != null && _islandGenerator.tilemap != null)
                _islandGenerator.ClearCell(transform.position);
        }
    }
}
