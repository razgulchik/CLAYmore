using UnityEngine;

namespace CLAYmore
{
    /// <summary>
    /// Main menu controller.
    /// Wire Play/Credits/Quit buttons to the public methods via OnClick in Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        public CreditsUI creditsUI;

        public void OnPlayClicked() => SceneLoader.LoadGame();

        public void OnCreditsClicked()
        {
            creditsUI?.Show();
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
