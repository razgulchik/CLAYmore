using CLAYmore.ECS;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Shows "Wave N" when a new wave starts:
    /// scales in from far away → holds → fades out.
    /// Assign label in the Inspector.
    /// </summary>
    public class WaveAnnouncementUI : MonoBehaviour
    {
        public TextMeshProUGUI label;

        [Header("Scale In")]
        [Tooltip("Starting scale (feels like coming from far away)")]
        public float startScale   = 0.05f;
        [Tooltip("Duration of the scale-in animation")]
        public float scaleInDuration = 0.5f;
        public Ease  scaleInEase     = Ease.OutBack;

        [Header("Hold & Fade")]
        [Tooltip("How long the label stays fully visible")]
        public float holdDuration  = 1.2f;
        [Tooltip("Duration of the fade-out")]
        public float fadeDuration  = 0.8f;
        public Ease  fadeEase      = Ease.InQuad;

        private void Awake()
        {
            if (label != null)
            {
                label.alpha = 0f;
                label.transform.localScale = Vector3.one;
            }
        }

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<WaveChangedEvent>(OnWaveChanged);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<WaveChangedEvent>(OnWaveChanged);
        }

        private void OnWaveChanged(WaveChangedEvent evt)
        {
            if (label == null) return;

            label.text = $"Wave {evt.WaveIndex + 1}";

            // Kill any running animation
            label.DOKill();
            label.transform.DOKill();

            // Reset state
            label.alpha = 1f;
            label.transform.localScale = Vector3.one * startScale;

            // Scale in → hold → fade out
            var seq = DOTween.Sequence();
            seq.Append(label.transform
                .DOScale(Vector3.one, scaleInDuration)
                .SetEase(scaleInEase));
            seq.AppendInterval(holdDuration);
            seq.Append(label
                .DOFade(0f, fadeDuration)
                .SetEase(fadeEase));
        }
    }
}
