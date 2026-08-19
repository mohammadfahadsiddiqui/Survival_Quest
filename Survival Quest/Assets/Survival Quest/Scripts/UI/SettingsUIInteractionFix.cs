using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Repairs runtime-created settings controls so Unity's EventSystem can
    /// receive pointer/touch events. MainMenuUI creates the sliders at runtime.
    /// </summary>
    public static class SettingsUIInteractionFix
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var runner = new GameObject("Settings UI Interaction Fix");
            Object.DontDestroyOnLoad(runner);
            runner.AddComponent<Runner>();
        }

        private sealed class Runner : MonoBehaviour
        {
            private void Start()
            {
                StartCoroutine(FixWhenMenuExists());
            }

            private IEnumerator FixWhenMenuExists()
            {
                // MainMenuUI builds its Canvas in Start(), so wait until it exists.
                for (int i = 0; i < 120; i++)
                {
                    yield return null;
                    FixControls();

                    if (FindObjectsByType<Slider>(FindObjectsSortMode.None).Length > 0)
                    {
                        // Run a few more passes because the menu can be rebuilt.
                        for (int j = 0; j < 5; j++)
                        {
                            yield return null;
                            FixControls();
                        }
                        yield break;
                    }
                }
            }

            private static void FixControls()
            {
                Slider[] sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
                foreach (Slider slider in sliders)
                {
                    if (slider == null) continue;

                    slider.interactable = true;
                    slider.minValue = 0f;
                    slider.maxValue = 1f;
                    slider.direction = Slider.Direction.LeftToRight;

                    // The original runtime UI disabled raycasts on the track
                    // and handle, which prevents dragging/clicking the slider.
                    Graphic[] graphics = slider.GetComponentsInChildren<Graphic>(true);
                    foreach (Graphic graphic in graphics)
                        graphic.raycastTarget = true;

                    if (slider.handleRect != null)
                    {
                        Graphic handleGraphic = slider.handleRect.GetComponent<Graphic>();
                        if (handleGraphic != null)
                            handleGraphic.raycastTarget = true;
                    }

                    if (slider.fillRect != null)
                    {
                        Graphic fillGraphic = slider.fillRect.GetComponent<Graphic>();
                        if (fillGraphic != null)
                            fillGraphic.raycastTarget = false;
                    }
                }

                Toggle[] toggles = FindObjectsByType<Toggle>(FindObjectsSortMode.None);
                foreach (Toggle toggle in toggles)
                {
                    if (toggle == null) continue;

                    toggle.interactable = true;

                    // The Toggle's target graphic must receive the pointer event.
                    if (toggle.targetGraphic != null)
                        toggle.targetGraphic.raycastTarget = true;

                    // The checkmark should not block the target graphic.
                    if (toggle.graphic != null)
                        toggle.graphic.raycastTarget = false;
                }

                Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
                foreach (Button button in buttons)
                {
                    if (button == null) continue;
                    button.interactable = true;
                    if (button.targetGraphic != null)
                        button.targetGraphic.raycastTarget = true;
                }
            }
        }
    }
}
