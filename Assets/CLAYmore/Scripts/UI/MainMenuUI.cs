using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLAYmore
{
    /// <summary>
    /// Main menu controller.
    /// Wire Play/Credits/Quit buttons to the public methods via OnClick in Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Tooltip("Exact name of the game scene to load")]
        public string gameSceneName = "SampleScene";

        public CreditsUI creditsUI;

        public void OnPlayClicked()
        {
            SceneManager.LoadScene(gameSceneName);
        }

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
