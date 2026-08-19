using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalGame.UI
{
    public static class ReferenceMainMenuBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") return;
            if (Object.FindFirstObjectByType<ReferenceMainMenuUI>() != null) return;
            var go = new GameObject("Reference Main Menu - AUTO ATTACHED");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<ReferenceMainMenuUI>();
        }
    }
}
