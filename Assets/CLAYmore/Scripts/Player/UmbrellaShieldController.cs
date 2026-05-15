using DG.Tweening;
using CLAYmore.ECS;
using UnityEngine;

namespace CLAYmore
{
    public class UmbrellaShieldController : MonoBehaviour
    {
        [Header("Animation")]
        public float appearDuration   = 0.4f;
        public float dissolveDuration = 0.6f;
        public float hitPunchScale    = 0.25f;
        public float hitPunchTime     = 0.2f;

        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
            transform.localScale = Vector3.zero;
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

        private void AnimateAppear()
        {
            transform.DOKill();
            _renderer.enabled = true;
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, appearDuration).SetEase(Ease.OutBack);
        }

        private void AnimateDissolve()
        {
            transform.DOKill();
            transform.DOScale(Vector3.zero, dissolveDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() => _renderer.enabled = false);
        }

        private PlayerStatsComponent GetPlayerStats()
        {
            if (World.Current == null) return null;
            foreach (var e in World.Current.Query<PlayerStatsComponent>())
                return e.Get<PlayerStatsComponent>();
            return null;
        }
    }
}
