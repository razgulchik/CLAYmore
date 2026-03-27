using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CLAYmore
{
    /// <summary>
    /// One card in the modifier choice panel.
    /// Assign icon, name label, description label, and level label in the inspector.
    /// </summary>
    public class ModifierCardUI : MonoBehaviour
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
                levelLabel.gameObject.SetActive(modifier.maxLevel > 1);
                levelLabel.text = $"Level {newLevel}/{modifier.maxLevel}";
            }
            if (priceLabel != null)
                priceLabel.text = price > 0 ? $"-{price}" : "Free";

            button.interactable = canAfford;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onChosen(modifier));
        }
    }
}
