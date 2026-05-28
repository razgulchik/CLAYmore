using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    public class AbilityIconUI : MonoBehaviour
    {
        public Image           backImage;
        public Image           iconImage;
        public TextMeshProUGUI levelLabel;

        public ModifierConfig Modifier { get; private set; }

        public void Setup(ModifierConfig modifier, int level, Sprite background)
        {
            Modifier = modifier;

            if (backImage != null && background != null)
                backImage.sprite = background;

            if (iconImage != null)
                iconImage.sprite = modifier.icon;

            SetLevel(level);
        }

        public void SetLevel(int level)
        {
            if (levelLabel != null)
                levelLabel.text = $"{level}";
        }
    }
}
