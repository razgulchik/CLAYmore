using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace CLAYmore
{
    public class ModifierCardUI : MonoBehaviour, IPointerClickHandler
    {
        public Image            iconImage;
        public TextMeshProUGUI  nameLabel;
        public TextMeshProUGUI  descriptionLabel;
        public TextMeshProUGUI  levelLabel;
        public Image            backgroundImage;

        private Action<ModifierConfig> _onChosen;
        private ModifierConfig         _modifier;

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

        public void Select() => _onChosen?.Invoke(_modifier);

        public void OnPointerClick(PointerEventData _) => Select();
    }
}
