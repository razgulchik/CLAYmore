using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class ChestSpawner : MonoBehaviour
    {
        private IslandGenerator _islandGenerator;
        private PrefabPool      _chestPool;

        private int   _firstThreshold;
        private float _multiplier;
        private int   _additive;

        private int _coinsCollected;
        private int _currentThreshold;

        private bool _isGameOver;

        public void Init(IslandGenerator islandGenerator, PrefabPool chestPool,
                         int firstThreshold, float multiplier, int additive)
        {
            _islandGenerator  = islandGenerator;
            _chestPool        = chestPool;
            _firstThreshold   = firstThreshold;
            _multiplier       = multiplier;
            _additive         = additive;
            _currentThreshold = firstThreshold;
        }

        private void Start()
        {
            if (_chestPool == null || _islandGenerator == null)
            {
                Debug.LogWarning("ChestSpawner: missing references — disabled.");
                enabled = false;
                return;
            }

            World.Current?.Events.Subscribe<CoinsAddedEvent>(OnCoinsAdded);
            World.Current?.Events.Subscribe<GameOverEvent>(OnGameOver);

            World.Current?.Events.Publish(new ChestProgressEvent { CoinsCollected = 0, Threshold = _currentThreshold });
            Debug.Log($"[Chest] До первого сундука: {_currentThreshold} монет");
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<CoinsAddedEvent>(OnCoinsAdded);
            World.Current?.Events.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void OnGameOver(GameOverEvent e) => _isGameOver = true;

        private void OnCoinsAdded(CoinsAddedEvent e)
        {
            if (_isGameOver) return;

            _coinsCollected += e.Amount;

            if (_coinsCollected >= _currentThreshold)
            {
                _coinsCollected -= _currentThreshold;
                _currentThreshold = Mathf.RoundToInt(_currentThreshold * _multiplier) + _additive;
                SpawnChest();
                World.Current?.Events.Publish(new ChestProgressEvent { CoinsCollected = _coinsCollected, Threshold = _currentThreshold });
                Debug.Log($"[Chest] Сундук! До следующего: {_currentThreshold - _coinsCollected} / {_currentThreshold}");
            }
            else
            {
                World.Current?.Events.Publish(new ChestProgressEvent { CoinsCollected = _coinsCollected, Threshold = _currentThreshold });
                Debug.Log($"[Chest] До сундука: {_currentThreshold - _coinsCollected} / {_currentThreshold}");
            }
        }

        private void SpawnChest()
        {
            if (!_islandGenerator.TryGetRandomWalkableCellCenter(out Vector3 landPos, avoidPlayerNeighbours: true)) return;
            if (!_islandGenerator.TryMarkChestLanded(landPos)) return;

            GameObject chestGO = _chestPool.Get(landPos);
            if (!chestGO.TryGetComponent<Chest>(out Chest chest))
            {
                Debug.LogWarning("ChestSpawner: chest prefab is missing a Chest component.");
                _islandGenerator.ClearChest(landPos);
                _chestPool.Return(chestGO);
                return;
            }

            chest.Initialize(landPos, _islandGenerator.tilemap, _chestPool, _islandGenerator);
        }
    }
}
