using UnityEngine;
using UnityEngine.UI;

namespace CLAYmore
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] GameObject mainMenuPanel;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] Button quitButton;

        private void Awake()
        {
            mainMenuPanel.SetActive(true);
            creditsPanel.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR
            if (quitButton != null)
                quitButton.enabled = false;
#endif
        }

        public void OnPlayClicked() => SceneLoader.LoadGame();

        public void OnCreditsClicked()
        {
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void OnBackClicked()
        {
            creditsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
