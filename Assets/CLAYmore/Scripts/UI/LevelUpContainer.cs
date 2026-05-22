using CLAYmore.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    public class LevelUpContainer : MonoBehaviour
    {
        public Slider           slider;
        public Image            fillImage;
        public TextMeshProUGUI  coinsToNextText;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ChestProgressEvent>(OnChestProgress);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ChestProgressEvent>(OnChestProgress);
        }

        private void OnChestProgress(ChestProgressEvent e)
        {
            float progress = e.Threshold > 0 ? (float)e.CoinsCollected / e.Threshold : 0f;
            slider.value = progress;

            int remaining = e.Threshold - e.CoinsCollected;
            coinsToNextText.SetText(remaining.ToString());

            if (fillImage != null)
                fillImage.enabled = e.CoinsCollected > 0;
        }
    }
}
