using DG.Tweening;
using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Attach to the whirl sprite child of the player prefab.
    /// Call Play() each time the player takes a walk step with Whirlwind active.
    /// </summary>
    public class WhirlVFXController : MonoBehaviour
    {
        [Header("Timing")]
        public float fadeInTime  = 0.05f;
        public float visibleTime = 0.15f;
        public float fadeOutTime = 0.25f;

        [Header("Shake")]
        public float shakeStrength = 0.05f;
        public float shakeDuration = 0.35f;
        public int   shakeVibrato  = 16;

        private SpriteRenderer _sr;
        private Sequence       _seq;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            SetAlpha(0f);
        }

        public void Play()
        {
            _seq?.Kill(complete: false);
            transform.DOKill();

            SetAlpha(0f);

            _seq = DOTween.Sequence()
                .Append(_sr.DOFade(1f, fadeInTime))
                .AppendInterval(visibleTime)
                .Append(_sr.DOFade(0f, fadeOutTime));

            // Side-to-side shake (x axis only)
            transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, 0f), shakeVibrato, 0f, false, true);
        }

        private void SetAlpha(float a)
        {
            if (_sr == null) return;
            var c = _sr.color;
            c.a = a;
            _sr.color = c;
        }
    }
}
