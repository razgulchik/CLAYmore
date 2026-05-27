using UnityEngine.SceneManagement;

namespace CLAYmore
{
    public static class SceneLoader
    {
        public const string MainMenu = "Main Menu";
        public const string Game     = "GameScene";

        public static void LoadGame()     => SceneManager.LoadScene(Game);
        public static void LoadMainMenu() => SceneManager.LoadScene(MainMenu);
        public static void Restart()      => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
