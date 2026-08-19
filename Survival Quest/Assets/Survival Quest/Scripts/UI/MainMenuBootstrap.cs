using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Guarantees that a fresh game session starts at MainMenu, even when SampleScene
    /// is the scene currently open in the Unity Editor.
    /// </summary>
    public static class MainMenuBootstrap
    {
        private const string MainMenuScene = "MainMenu";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OpenMainMenuOnFreshLaunch()
        {
            if (SceneManager.GetActiveScene().name == MainMenuScene)
                return;

            SceneManager.LoadScene(MainMenuScene, LoadSceneMode.Single);
        }
    }
}
