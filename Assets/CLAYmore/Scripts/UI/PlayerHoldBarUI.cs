using CLAYmore.ECS;
using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    public class PlayerHoldBarUI : MonoBehaviour
    {
        public Image fillImage;
        public Image background;

        private void OnEnable()
        {
            World.Current?.Events.Subscribe<ExpansionHoldProgressEvent>(OnProgress);
            SetVisible(false);
        }

        private void OnDisable()
        {
            World.Current?.Events.Unsubscribe<ExpansionHoldProgressEvent>(OnProgress);
        }

        private void OnProgress(ExpansionHoldProgressEvent evt)
        {
            if (evt.Progress < 0f)
            {
                SetVisible(false);
            }
            else
            {
                SetVisible(true);
                fillImage.fillAmount = evt.Progress;
            }
        }

        private void SetVisible(bool visible)
        {
            if (fillImage  != null) fillImage.gameObject.SetActive(visible);
            if (background != null) background.gameObject.SetActive(visible);
        }
    }
}
