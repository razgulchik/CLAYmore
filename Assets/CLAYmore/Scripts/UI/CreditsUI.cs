using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Credits panel. Call Show() to open, Hide() (or the Close button) to close.
    /// </summary>
    public class CreditsUI : MonoBehaviour
    {
        public GameObject panel;

        private void Awake()
        {
            panel?.SetActive(false);
        }

        public void Show() => panel?.SetActive(true);

        public void Hide() => panel?.SetActive(false);
    }
}
