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
        public TextMeshProUGUI  priceLabel;
        public Button           button;

        public void Setup(ModifierConfig modifier, int newLevel, int price, bool canAfford, Action<ModifierConfig> onChosen)
        {
            if (iconImage        != null) iconImage.sprite = modifier.icon;
            if (nameLabel        != null) nameLabel.text   = modifier.displayName;
            if (descriptionLabel != null) descriptionLabel.text = modifier.GetDescription(newLevel);
            if (levelLabel       != null)
            {
                levelLabel.gameObject.SetActive(modifier.maxLevel > 0);
                levelLabel.text = $"{newLevel}"; ///{modifier.maxLevel}
            }
            if (priceLabel != null)
                priceLabel.text = price > 0 ? $"-{price}" : "Free";

            button.interactable = canAfford;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onChosen(modifier));
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (button != null && button.interactable)
                button.onClick.Invoke();
        }
    }
}
