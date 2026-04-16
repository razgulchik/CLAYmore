using DG.Tweening;
using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CLAYmore
{
    /// <summary>
    /// View: анимация, рендер, тень. Никакой логики урона здесь нет.
    /// Состояние горшка хранится в PotComponent.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Pot : MonoBehaviour
    {
        public GameObject coinPrefab;

        private PotComponent _pot;
        private Entity _entity;
        private SpriteRenderer _renderer;

        private const float CellHalfSize = 0.5f;
        private const string LayerPots       = "Pots";
        private const string LayerPotsFlight = "PotsFlight";

        private Economy _economy;
        private IslandGenerator _islandGenerator;

        private PrefabPool _potPool;
        private PrefabPool _shadowPool;
        private GameObject _currentShadow;

        // ── Init ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by PotSpawner on every spawn (including re-use from pool).
        /// </summary>
        public void Initialize(PotConfig config, Vector3 landPos, Tilemap tilemap,
                               Economy economy,
                               IslandGenerator islandGenerator,
                               PrefabPool potPool, PrefabPool shadowPool,
                               float fallDurationMultiplier = 1f)
        {
            _economy         = economy;
            _islandGenerator = islandGenerator;
            _potPool         = potPool;
            _shadowPool      = shadowPool;

            transform.DOKill();

            _entity = GetComponent<Entity>() ?? gameObject.AddComponent<Entity>();

            if (_entity.Has<PotComponent>())
                _pot = _entity.Get<PotComponent>();
            else
                _pot = _entity.Add(new PotComponent());

            _pot.Config   = config;
            _pot.State    = PotState.InFlight;
            _pot.LandPos  = landPos;
            _pot.LandCell = tilemap.WorldToCell(landPos);

            if (_entity.Has<HealthComponent>())
            {
                var h = _entity.Get<HealthComponent>();
                h.MaxHp = config.maxHp;
                h.Hp    = config.maxHp;
            }
            else
            {
                _entity.Add(new HealthComponent { MaxHp = config.maxHp, Hp = config.maxHp });
            }

            World.Current?.RegisterEntity(_entity);
            World.Current?.Events.Subscribe<EntityDiedEvent>(OnEntityDied);
            World.Current?.Events.Subscribe<EntityDamagedEvent>(OnEntityDamaged);

            _renderer = GetComponent<SpriteRenderer>();
            if (config.sprite != null)
                _renderer.sprite = config.sprite;
            _renderer.sortingLayerName = LayerPotsFlight;

            _currentShadow = _shadowPool?.Get(new Vector3(landPos.x, landPos.y, transform.position.z));

            transform.DOMove(landPos, config.fallDuration * fallDurationMultiplier)
                .SetEase(config.fallEase)
                .OnComplete(OnTweenLanded);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Немедленно разбить горшок (вызывается снаружи — системой урона).
        /// </summary>
        public void BreakVisual()
        {
            if (_pot.State == PotState.Breaking) return;
            _pot.State = PotState.Breaking;
            transform.DOKill();
            transform.DOShakePosition(0.12f, new Vector3(0.08f, 0.08f, 0f), vibrato: 15)
                .OnComplete(BreakCleanup);
        }

        /// <summary>
        /// Визуальная реакция на удар (горшок выжил).
        /// </summary>
        public void HitVisual()
        {
            transform.DOPunchScale(Vector3.one * 0.2f, 0.15f, vibrato: 8);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void Update()
        {
            if (_pot == null || _pot.State != PotState.InFlight) return;
            if (transform.position.y - _pot.LandPos.y <= CellHalfSize)
                RegisterLanded();
        }

        /// <summary>
        /// Горшок пересёк границу клетки — регистрируем как Landed и публикуем событие.
        /// </summary>
        private void RegisterLanded()
        {
            _pot.State = PotState.Landed;
            World.Current?.Events.Publish(new PotLandedEvent { PotEntity = _entity });
        }

        /// <summary>
        /// DOTween OnComplete — горшок долетел до позиции.
        /// Синхронизируем position, меняем слой, убираем тень.
        /// </summary>
        private void OnTweenLanded()
        {
            transform.position = _pot.LandPos;
            _renderer.sortingLayerName = LayerPots;
            ReturnShadow();
            _islandGenerator?.MarkPotLanded(_pot.LandPos);
        }

        private void OnEntityDamaged(EntityDamagedEvent evt)
        {
            if (evt.Entity == _entity && evt.Hp > 0) HitVisual();
        }

        private void OnEntityDied(EntityDiedEvent evt)
        {
            if (evt.Entity == _entity) BreakVisual();
        }

        private void BreakCleanup()
        {
            World.Current?.Events.Unsubscribe<EntityDamagedEvent>(OnEntityDamaged);
            World.Current?.Events.Unsubscribe<EntityDiedEvent>(OnEntityDied);
            ReturnShadow();
            if (_islandGenerator != null) _islandGenerator.ClearCell(_pot.LandPos);

            int count = Random.Range(_pot.Config.coinDropMin, _pot.Config.coinDropMax + 1);
            SpawnCoins(count);

            World.Current?.UnregisterEntity(_entity);
            _potPool.Return(gameObject);
        }

        private void ReturnShadow()
        {
            if (_currentShadow == null) return;
            _shadowPool?.Return(_currentShadow);
            _currentShadow = null;
        }

        private void SpawnCoins(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 targetPos = transform.position + new Vector3(
                    Random.Range(-0.8f, 0.8f),
                    Random.Range(0.3f, 1.2f),
                    0f);

                if (coinPrefab != null)
                {
                    GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                    var captured = coin;
                    coin.transform.DOJump(targetPos, 0.5f, 1, 0.4f)
                        .OnComplete(() =>
                        {
                            _economy?.Add(1);
                            Destroy(captured);
                        });
                }
                else
                {
                    _economy?.Add(1);
                }
            }
        }
    }
}
