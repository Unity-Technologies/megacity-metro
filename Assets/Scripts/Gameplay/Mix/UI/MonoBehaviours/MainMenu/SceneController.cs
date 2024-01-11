using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Unity.MegacityMetro
{
    public static class SceneController
    {
        private static List<string> GameScenes =  new() {"Main", "ThinClientTest"};
        public static bool IsFrontEnd => SceneManager.GetActiveScene().name == "Menu";
        public static bool IsGameScene => GameScenes.Contains(SceneManager.GetActiveScene().name);

        public static bool IsReturningToMainMenu;
        
        public static void LoadGame()
        {
            SceneManager.LoadSceneAsync("Main");
        }
        
        public static void LoadMenu()
        {
            SceneManager.LoadScene("Menu");
            IsReturningToMainMenu = true;
        }
    }
}