using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CLAYmore
{
    public class PressableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("How far down the button moves (should match shadow offset)")]
        public float pressOffset = 4f;
        [Tooltip("Duration of press/release animation")]
        public float duration = 0.06f;

        private RectTransform _rect;
        private Vector2       _restPosition;

        private void Awake()
        {
            _rect         = GetComponent<RectTransform>();
            _restPosition = _rect.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData _)
        {
            _rect.DOKill();
            _rect.DOAnchorPos(_restPosition + Vector2.down * pressOffset, duration).SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(PointerEventData _)
        {
            _rect.DOKill();
            _rect.DOAnchorPos(_restPosition, duration).SetEase(Ease.OutQuad);
        }
    }
}
