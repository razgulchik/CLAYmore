using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    /// <summary>
    /// Single row in the acquired-modifiers scroll panel.
    /// Assign iconImage, nameLabel, levelLabel in the Inspector.
    /// descriptionLabel is optional.
    /// </summary>
    public class ModifierAcquiredCardUI : MonoBehaviour
    {
        public Image           iconImage;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI levelLabel;
        public TextMeshProUGUI descriptionLabel; // optional

        public ModifierConfig Modifier { get; private set; }

        public void Setup(ModifierConfig modifier, int level)
        {
            Modifier = modifier;

            if (iconImage != null)
            {
                iconImage.sprite  = modifier.icon;
                iconImage.enabled = modifier.icon != null;
            }

            if (nameLabel != null)
                nameLabel.text = modifier.displayName;

            SetLevel(level);
        }

        public void SetLevel(int level)
        {
            if (levelLabel != null)
                levelLabel.text = $"Lv.{level}";

            if (descriptionLabel != null && Modifier != null)
                descriptionLabel.text = Modifier.GetDescription(level);
        }
    }
}
