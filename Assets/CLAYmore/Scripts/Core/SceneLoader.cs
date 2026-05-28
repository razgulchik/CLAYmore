using UnityEngine.SceneManagement;

namespace CLAYmore
{
    public static class SceneLoader
    {
        public const string MainMenu = "Main Menu";
        public const string Game     = "GameScene";

        public static void LoadGame()     { PauseManager.Instance.Reset(); SceneManager.LoadScene(Game); }
        public static void LoadMainMenu() { PauseManager.Instance.Reset(); SceneManager.LoadScene(MainMenu); }
        public static void Restart()      { PauseManager.Instance.Reset(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    }
}
