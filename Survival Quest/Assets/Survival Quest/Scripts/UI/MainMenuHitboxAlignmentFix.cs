using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SurvivalGame.UI
{
    /// <summary>
    /// Aligns the invisible clickable rectangles with the actual button artwork.
    /// The menu is built from the artwork at runtime, so this fixes the hit/glow
    /// position without changing the artwork or button logic.
    /// </summary>
    internal static class MainMenuHitboxAlignmentFix
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject runner = new GameObject("Main Menu Hitbox Alignment Runner");
            Object.DontDestroyOnLoad(runner);
            runner.AddComponent<AlignmentRunner>();
        }

        private sealed class AlignmentRunner : MonoBehaviour
        {
            private IEnumerator Start()
            {
                // ReferenceMainMenuUI builds the menu in Start(). Wait until it exists.
                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.15f);
                Apply();
                Destroy(gameObject);
            }

            private static void Apply()
            {
                Set("PLAY Click",       .048f, .525f, .314f, .615f);
                Set("CONTINUE Click",   .048f, .425f, .314f, .515f);
                Set("NEW GAME Click",   .048f, .335f, .314f, .425f);
                Set("SETTINGS Click",   .048f, .235f, .314f, .335f);
                Set("EXIT Click",       .048f, .135f, .314f, .235f);
            }

            private static void Set(string objectName, float xMin, float yMin, float xMax, float yMax)
            {
                GameObject go = GameObject.Find(objectName);
                if (go == null)
                    return;

                RectTransform rect = go.GetComponent<RectTransform>();
                if (rect == null)
                    return;

                rect.anchorMin = new Vector2(xMin, yMin);
                rect.anchorMax = new Vector2(xMax, yMax);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                // Keep the transparent raycast graphic and both hover-glow systems
                // perfectly locked to the corrected button rectangle.
                Graphic graphic = go.GetComponent<Graphic>();
                if (graphic != null)
                    graphic.SetVerticesDirty();

                LayoutRebuilder.MarkLayoutForRebuild(rect);
            }
        }
    }
}
