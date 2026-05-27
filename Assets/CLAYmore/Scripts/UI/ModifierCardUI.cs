using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace CLAYmore
{
    public class ModifierCardUI : MonoBehaviour, IPointerClickHandler
    {
        public Image            iconImage;
        public TextMeshProUGUI  nameLabel;
        public TextMeshProUGUI  descriptionLabel;
        public TextMeshProUGUI  levelLabel;
        public Image            backgroundImage;
        public Image            arrow;
        public Image            holdFill;

        private const float HoldDelay  = 0.5f;
        private const float PulseMax   = 1.15f;
        private const float PulseSpeed = 15f;

        private Action<ModifierConfig> _onChosen;
        private ModifierConfig         _modifier;
        private Coroutine              _holdCoroutine;
        private Vector2                _arrowOrigin;

        private void Awake()
        {
            if (arrow != null) _arrowOrigin = arrow.rectTransform.anchoredPosition;
        }

        public void Setup(ModifierConfig modifier, int newLevel, Action<ModifierConfig> onChosen, Sprite background = null)
        {
            _modifier = modifier;
            _onChosen = onChosen;

            if (backgroundImage != null && background != null)
                backgroundImage.sprite = background;

            if (iconImage        != null) iconImage.sprite = modifier.icon;
            if (nameLabel        != null) nameLabel.text   = modifier.displayName;
            if (descriptionLabel != null) descriptionLabel.text = modifier.GetDescription(newLevel);
            if (levelLabel       != null)
            {
                levelLabel.gameObject.SetActive(modifier.maxLevel > 0);
                levelLabel.text = $"{newLevel}";
            }
        }

        public void StartHold(Action onComplete)
        {
            StopHold();
            _holdCoroutine = StartCoroutine(HoldCoroutine(onComplete));
            if (arrow != null)
                arrow.transform.DOScale(PulseMax, Mathf.PI / PulseSpeed)
                    .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
        }

        public void StopHold()
        {
            if (_holdCoroutine != null) { StopCoroutine(_holdCoroutine); _holdCoroutine = null; }
            if (holdFill != null) holdFill.fillAmount = 0f;
            if (arrow    != null)
            {
                DOTween.Kill(arrow.transform);
                arrow.transform.localScale = Vector3.one;
                DOTween.Kill(arrow.rectTransform);
                arrow.rectTransform.anchoredPosition = _arrowOrigin;
            }
        }

        public void Select() => _onChosen?.Invoke(_modifier);

        public void OnPointerClick(PointerEventData _) => Select();

        private IEnumerator HoldCoroutine(Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < HoldDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                if (holdFill != null) holdFill.fillAmount = Mathf.Clamp01(elapsed / HoldDelay);
                yield return null;
            }
            _holdCoroutine = null;
            if (holdFill != null) holdFill.fillAmount = 0f;
            onComplete();
        }
    }
}
