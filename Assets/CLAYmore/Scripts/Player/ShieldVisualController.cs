using DG.Tweening;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Показывает/скрывает визуальный щит на игроке.
    /// Повесить на объект ShieldVisual (дочерний объект игрока).
    /// Шейдер должен иметь свойство _DissolveThreshold (0=скрыт, 1=виден)
    /// и читать Vertex Color Alpha.
    /// </summary>
    public class ShieldVisualController : MonoBehaviour
    {
        [Header("Animation")]
        public float appearDuration  = 0.4f;
        public float dissolveDuration = 0.6f;
        public float hitPunchScale   = 0.25f;
        public float hitPunchTime    = 0.2f;

        private static readonly int DissolveThreshold = Shader.PropertyToID("_DissolveThreshold");

        private SpriteRenderer _renderer;
        private Material       _material;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _material = _renderer.material;
            _material.SetFloat(DissolveThreshold, 0f);
            _renderer.enabled = false;
        }

        private void Start()
        {
            World.Current?.Events.Subscribe<ShieldAbsorbedEvent>(OnShieldAbsorbed);
            World.Current?.Events.Subscribe<PlayerStatsChangedEvent>(OnStatsChanged);

            var stats = GetPlayerStats();
            if (stats != null && stats.ShieldMax > 0 && stats.ShieldCurrent > 0 && stats.ShieldCooldown <= 0f)
                AnimateAppear();
        }

        private void OnDestroy()
        {
            World.Current?.Events.Unsubscribe<ShieldAbsorbedEvent>(OnShieldAbsorbed);
            World.Current?.Events.Unsubscribe<PlayerStatsChangedEvent>(OnStatsChanged);
        }

        // ── Handlers ──────────────────────────────────────────────────────────

        private void OnShieldAbsorbed(ShieldAbsorbedEvent _)
        {
            var stats = GetPlayerStats();
            if (stats == null) return;

            if (stats.ShieldCurrent > 0)
            {
                transform.DOKill();
                transform.DOPunchScale(Vector3.one * hitPunchScale, hitPunchTime, vibrato: 6);
            }
            else
            {
                AnimateDissolve();
            }
        }

        private void OnStatsChanged(PlayerStatsChangedEvent _)
        {
            var stats = GetPlayerStats();
            if (stats == null) return;

            bool shieldActive = stats.ShieldMax > 0 && stats.ShieldCurrent > 0 && stats.ShieldCooldown <= 0f;

            if (shieldActive && !_renderer.enabled)
                AnimateAppear();
        }

        // ── Animation ─────────────────────────────────────────────────────────

        private void AnimateAppear()
        {
            DOTween.Kill(_material);
            _renderer.enabled = true;
            _material.SetFloat(DissolveThreshold, 0f);
            // снизу вверх: порог растёт от 0 до 1
            DOVirtual.Float(0f, 1f, appearDuration, v => _material.SetFloat(DissolveThreshold, v))
                .SetEase(Ease.OutCubic)
                .SetTarget(_material);
        }

        private void AnimateDissolve()
        {
            DOTween.Kill(_material);
            // сверху вниз: порог падает от 1 до 0
            DOVirtual.Float(1f, 0f, dissolveDuration, v => _material.SetFloat(DissolveThreshold, v))
                .SetEase(Ease.InCubic)
                .SetTarget(_material)
                .OnComplete(() => _renderer.enabled = false);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private PlayerStatsComponent GetPlayerStats()
        {
            if (World.Current == null) return null;
            foreach (var e in World.Current.Query<PlayerStatsComponent>())
                return e.Get<PlayerStatsComponent>();
            return null;
        }
    }
}
