using DG.Tweening;
using UnityEngine;

namespace CLAYmore
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TargetSpotFader : MonoBehaviour
    {
        [SerializeField] private float _delayMs    = 600f;
        [SerializeField] private float _durationMs = 200f;

        private SpriteRenderer _sprite;
        private Tween          _tween;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _tween?.Kill();
            var c = _sprite.color;
            c.a = 1f;
            _sprite.color = c;

            _tween = _sprite
                .DOFade(0, _durationMs / 1000f)
                .SetDelay(_delayMs / 1000f)
                .SetEase(Ease.InQuad);
        }

        private void OnDisable()
        {
            _tween?.Kill();
        }
    }
}
