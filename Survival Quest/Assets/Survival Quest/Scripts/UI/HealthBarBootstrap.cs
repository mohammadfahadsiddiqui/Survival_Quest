using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Automatically installs the reference-style health bar into the gameplay UI.
    /// It does not run in MainMenu and does not require a scene hierarchy change.
    /// </summary>
    public sealed class HealthBarBootstrap : MonoBehaviour
    {
        private HealthBarUI m_HealthBar;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                return;
            }

            if (FindFirstObjectByType<HealthBarBootstrap>() != null)
            {
                return;
            }

            GameObject go = new GameObject("HealthBarBootstrap");
            DontDestroyOnLoad(go);
            go.AddComponent<HealthBarBootstrap>();
        }

        private void Start()
        {
            EnsureHealthBar();
        }

        private void Update()
        {
            if (SurvivalGame.Player.m_Current == null)
            {
                return;
            }

            EnsureHealthBar();

            if (m_HealthBar != null)
            {
                m_HealthBar.SetHealth(SurvivalGame.Player.m_Current.m_Health);
            }
        }

        private void EnsureHealthBar()
        {
            if (m_HealthBar != null)
            {
                return;
            }

            Canvas canvas = FindGameplayCanvas();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(
                    "GameplayCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1184f, 585f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            m_HealthBar = HealthBarUI.Create(canvas);

            // Hide the old numeric health label so the new reference bar is the only health display.
            InGameUI legacyUI = FindFirstObjectByType<InGameUI>();
            if (legacyUI != null)
            {
                Text[] texts = legacyUI.GetComponentsInChildren<Text>(true);
                foreach (Text text in texts)
                {
                    if (text == null)
                    {
                        continue;
                    }

                    string n = text.gameObject.name.ToLowerInvariant();
                    if (n.Contains("life") || n.Contains("health"))
                    {
                        text.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static Canvas FindGameplayCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Canvas best = null;
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }

                if (best == null || canvas.sortingOrder > best.sortingOrder)
                {
                    best = canvas;
                }
            }

            return best;
        }
    }
}
