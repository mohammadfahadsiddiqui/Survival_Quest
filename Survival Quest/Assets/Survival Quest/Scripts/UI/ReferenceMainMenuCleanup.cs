using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalGame.UI
{
    public static class ReferenceMainMenuCleanup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu") return;
            var menu = Object.FindFirstObjectByType<ReferenceMainMenuUI>();
            if (menu != null) Object.Destroy(menu.gameObject);
        }
    }
}
