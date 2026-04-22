using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Activates the correct HUD based on platform.
    /// In the Editor: toggle Force Mobile in the Inspector to preview each layout.
    /// </summary>
    public class HUDSwitcher : MonoBehaviour
    {
        public GameObject desktopHUD;
        public GameObject mobileHUD;

        [Tooltip("Enable in Editor to preview mobile HUD without building")]
        public bool forceMobile;

        private void Awake()
        {
            Apply(forceMobile || IsMobilePlatform());
        }

        private void OnValidate()
        {
            Apply(forceMobile || IsMobilePlatform());
        }

        private void Apply(bool mobile)
        {
            if (desktopHUD != null) desktopHUD.SetActive(!mobile);
            if (mobileHUD  != null) mobileHUD.SetActive(mobile);
        }

        private static bool IsMobilePlatform() =>
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;
    }
}
