using UnityEngine;
using UnityEngine.UI;
using SurvivalGame;

namespace SurvivalQuest.UI
{
    /// <summary>
    /// Creates and updates a simple on-screen health bar for the player.
    /// No manual Canvas setup is required.
    /// </summary>
    public class PlayerHealthBar : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(280f, 26f);
        [SerializeField] private Vector2 screenOffset = new Vector2(24f, -24f);

        private Slider slider;
        private Player player;
        private Text healthText;

        private void Awake()
        {
            BuildUI();
        }

        private void Start()
        {
            FindPlayer();
            Refresh();
        }

        private void Update()
        {
            if (player == null)
                FindPlayer();

            Refresh();
        }

        private void FindPlayer()
        {
            player = Player.m_Current;
            if (player == null)
                player = FindObjectOfType<Player>();
        }

        private void Refresh()
        {
            if (slider == null || player == null)
                return;

            slider.value = Mathf.Clamp(player.m_Health, 0f, 100f);

            if (healthText != null)
            {
                healthText.text = Mathf.CeilToInt(Mathf.Clamp(player.m_Health, 0f, 100f)) + "/100";
            }
        }

        private void BuildUI()
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Health Bar Canvas");
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject root = new GameObject("Player Health Bar");
            root.transform.SetParent(canvas.transform, false);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = screenOffset;
            rootRect.sizeDelta = size;

            Image frame = root.AddComponent<Image>();
            frame.color = new Color(0.04f, 0.04f, 0.04f, 0.9f);

            GameObject sliderObject = new GameObject("Health Fill");
            sliderObject.transform.SetParent(root.transform, false);
            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.02f, 0.15f);
            sliderRect.anchorMax = new Vector2(0.98f, 0.85f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(sliderObject.transform, false);
            RectTransform bgRect = backgroundObject.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0.15f, 0.02f, 0.02f, 1f);

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(sliderObject.transform, false);
            RectTransform fillRect = fillObject.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color(0.15f, 0.85f, 0.25f, 1f);

            slider.targetGraphic = fill;
            slider.fillRect = fillRect;
            slider.handleRect = null;

            GameObject textObject = new GameObject("Health Text");
            textObject.transform.SetParent(root.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            healthText = textObject.AddComponent<Text>();
            healthText.text = "100/100";
            healthText.alignment = TextAnchor.MiddleCenter;
            healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            healthText.fontSize = 16;
            healthText.fontStyle = FontStyle.Bold;
            healthText.color = Color.white;
        }
    }
}
