using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SurvivalGame.UI
{
    internal sealed class MainMenuHoverGlowFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private CanvasGroup glow;
        private float targetAlpha;
        private float currentAlpha;

        public static void AttachTo(GameObject buttonObject)
        {
            if (buttonObject == null || buttonObject.GetComponent<MainMenuHoverGlowFix>() != null) return;
            buttonObject.AddComponent<MainMenuHoverGlowFix>();
        }

        private void Awake()
        {
            BuildGlow();
            currentAlpha = 0f;
            targetAlpha = 0f;
            if (glow != null) glow.alpha = 0f;
        }

        private void Update()
        {
            if (glow == null) return;
            float pulse = targetAlpha > 0f ? 0.88f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f : 0f;
            float desired = targetAlpha * pulse;
            currentAlpha = Mathf.MoveTowards(currentAlpha, desired, Time.unscaledDeltaTime * 7f);
            glow.alpha = currentAlpha;
        }

        public void OnPointerEnter(PointerEventData eventData) => targetAlpha = 1f;
        public void OnPointerExit(PointerEventData eventData) => targetAlpha = 0f;

        private void BuildGlow()
        {
            GameObject root = new GameObject("Hover Glow", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(transform, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.SetAsLastSibling();

            glow = root.GetComponent<CanvasGroup>();
            glow.interactable = false;
            glow.blocksRaycasts = false;

            CreateBorderLayer(root.transform, "Outer Glow", -6f, 4f, new Color(1f, 0.28f, 0.02f, 0.16f));
            CreateBorderLayer(root.transform, "Middle Glow", -3f, 3f, new Color(1f, 0.48f, 0.04f, 0.34f));
            CreateBorderLayer(root.transform, "Core Glow", 0f, 2f, new Color(1f, 0.72f, 0.16f, 0.95f));
        }

        private static void CreateBorderLayer(Transform parent, string name, float expand, float thickness, Color color)
        {
            GameObject layer = new GameObject(name, typeof(RectTransform));
            RectTransform lr = layer.GetComponent<RectTransform>();
            lr.SetParent(parent, false);
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(expand, expand);
            lr.offsetMax = new Vector2(-expand, -expand);

            CreateEdge(layer.transform, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -thickness), Vector2.zero, color);
            CreateEdge(layer.transform, "Bottom", Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, thickness), color);
            CreateEdge(layer.transform, "Left", Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, 0f), color);
            CreateEdge(layer.transform, "Right", new Vector2(1f, 0f), Vector2.one, new Vector2(-thickness, 0f), Vector2.zero, color);
        }

        private static void CreateEdge(Transform parent, string name, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform r = edge.GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = min;
            r.anchorMax = max;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
            Image image = edge.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }
    }

    internal static class MainMenuHoverGlowBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start()
        {
            GameObject runner = new GameObject("Main Menu Hover Glow Bootstrap");
            Object.DontDestroyOnLoad(runner);
            runner.AddComponent<BootstrapRunner>();
        }

        private sealed class BootstrapRunner : MonoBehaviour
        {
            private IEnumerator Start()
            {
                yield return null;
                yield return null;
                yield return new WaitForSecondsRealtime(0.1f);
                Attach();
                Destroy(gameObject);
            }

            private static void Attach()
            {
                Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
                foreach (Button button in buttons)
                {
                    if (button == null || !button.interactable) continue;
                    if (!button.gameObject.name.EndsWith(" Click")) continue;
                    MainMenuHoverGlowFix.AttachTo(button.gameObject);
                }
            }
        }
    }
}
