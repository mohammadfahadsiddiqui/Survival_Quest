using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalGame.ScriptableObjects;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    /// <summary>Runtime-generated main menu for Survival Quest.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;

        private static MainMenuUI s_Instance;
        private Canvas m_Canvas;
        private GameObject m_MenuRoot;
        private GameObject m_SettingsRoot;
        private Text m_Status;
        private Button m_ContinueButton;
        private Slider m_VolumeSlider;
        private bool m_GameStarted;

        private const string SaveKey = "SurvivalQuest.SaveExists";
        private const string VolumeKey = "SurvivalQuest.Volume";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "SampleScene") return;
            if (FindFirstObjectByType<MainMenuUI>() != null) return;

            var host = new GameObject("Survival Quest - Main Menu");
            DontDestroyOnLoad(host);
            host.AddComponent<MainMenuUI>();
        }

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() => BuildMenu();

        private void BuildMenu()
        {
            if (m_Canvas != null) return;
            EnsureEventSystem();

            m_Canvas = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            m_Canvas.transform.SetParent(transform, false);
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 500;

            var scaler = m_Canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            m_MenuRoot = FullRect("Menu", m_Canvas.transform);
            AddImage(m_MenuRoot.transform, "Left Shade", new Color(0.02f, 0.07f, 0.03f, 0.62f), new Vector2(0, 0), new Vector2(0.48f, 1), Vector2.zero, Vector2.zero);
            AddImage(m_MenuRoot.transform, "Right Shade", new Color(0.01f, 0.025f, 0.01f, 0.45f), new Vector2(0.58f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            BuildLeftMenu();
            BuildInfoCards();
            BuildFooter();
            BuildSettings();

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void BuildLeftMenu()
        {
            var title = AddText(m_MenuRoot.transform, "Title", "SURVIVAL QUEST", 64, FontStyle.Bold, new Color(0.98f, 0.96f, 0.82f), TextAnchor.MiddleLeft,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(58, -38), new Vector2(650, 78));
            AddShadow(title.gameObject, 5, 5, 0.9f);

            var subtitle = AddText(m_MenuRoot.transform, "Subtitle", "SURVIVE • EXPLORE • CRAFT • CONQUER", 18, FontStyle.Bold, new Color(0.73f, 0.92f, 0.38f), TextAnchor.MiddleLeft,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(62, -104), new Vector2(560, 32));
            AddShadow(subtitle.gameObject, 2, 2, 0.8f);

            AddImage(m_MenuRoot.transform, "Title Line", new Color(0.56f, 0.35f, 0.12f, 0.9f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(60, -132), new Vector2(430, 4));

            var menu = Rect("Menu Buttons", m_MenuRoot.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(55, -20), new Vector2(530, 485));
            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };

            for (int i = 0; i < labels.Length; i++)
            {
                var button = WoodButton(menu.transform, labels[i], new Vector2(265, 190 - i * 90), new Vector2(500, 72));
                if (i == 0 || i == 2) button.onClick.AddListener(StartNewJourney);
                else if (i == 1)
                {
                    m_ContinueButton = button;
                    button.onClick.AddListener(ContinueJourney);
                }
                else if (i == 3) button.onClick.AddListener(OpenSettings);
                else button.onClick.AddListener(QuitGame);
            }

            m_Status = AddText(menu.transform, "Status", "", 17, FontStyle.Bold, new Color(1f, 0.78f, 0.25f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(10, 8), new Vector2(500, 28));
            RefreshContinueState();
        }

        private void BuildInfoCards()
        {
            var top = Card(new Vector2(-55, 125));
            AddText(top.transform, "Title", "SURVIVE THE UNKNOWN", 27, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(26, 30), new Vector2(480, 42));
            AddText(top.transform, "Body", "Explore • Gather • Craft • Survive", 17, FontStyle.Normal, new Color(0.9f, 0.95f, 0.78f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(26, 8), new Vector2(480, 30));
            WorldIcon(top.transform, new Vector2(445, 175), false);

            var bottom = Card(new Vector2(-55, -190));
            AddText(bottom.transform, "Title", "YOUR JOURNEY", 27, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(26, 32), new Vector2(480, 42));
            AddText(bottom.transform, "Save", HasSave() ? "SAVE DATA FOUND" : "NO SAVE DATA", 18, FontStyle.Bold,
                HasSave() ? new Color(0.72f, 0.93f, 0.38f) : new Color(1f, 0.72f, 0.24f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(26, -8), new Vector2(300, 30));
            AddText(bottom.transform, "Body", "Begin a new survival expedition.", 17, FontStyle.Normal, new Color(0.88f, 0.91f, 0.82f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(26, -42), new Vector2(430, 30));
            WorldIcon(bottom.transform, new Vector2(445, 165), true);
        }

        private GameObject Card(Vector2 position)
        {
            var go = Rect("Info Card", m_MenuRoot.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), position, new Vector2(545, 270));
            var image = go.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.13f, 0.17f, 0.09f), new Color(0.05f, 0.07f, 0.035f));
            image.type = Image.Type.Sliced;
            image.color = new Color(1, 1, 1, 0.94f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.37f, 0.16f, 0.9f);
            outline.effectDistance = new Vector2(2, -2);
            AddShadow(go, 7, -7, 0.75f);
            return go;
        }

        private void BuildFooter()
        {
            AddText(m_MenuRoot.transform, "Version", "VERSION 0.1", 14, FontStyle.Normal, new Color(0.8f, 0.9f, 0.62f, 0.85f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(34, 32), new Vector2(180, 25));
            AddText(m_MenuRoot.transform, "Copyright", "© SURVIVAL QUEST", 14, FontStyle.Normal, new Color(0.8f, 0.9f, 0.62f, 0.7f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(34, 8), new Vector2(220, 25));

            var audio = SmallButton("♪", new Vector2(-135, 35));
            audio.onClick.AddListener(ToggleMute);
            var fullscreen = SmallButton("⛶", new Vector2(-55, 35));
            fullscreen.onClick.AddListener(ToggleFullscreen);
        }

        private void BuildSettings()
        {
            m_SettingsRoot = Rect("Settings Panel", m_MenuRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 520));
            var bg = m_SettingsRoot.AddComponent<Image>();
            bg.sprite = WoodSprite(new Color(0.09f, 0.13f, 0.06f), new Color(0.025f, 0.04f, 0.02f));
            bg.type = Image.Type.Sliced;
            var outline = m_SettingsRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.42f, 0.18f, 0.95f);
            outline.effectDistance = new Vector2(3, -3);

            AddText(m_SettingsRoot.transform, "Title", "SETTINGS", 40, FontStyle.Bold, new Color(0.96f, 0.9f, 0.7f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -55), new Vector2(600, 60));
            AddText(m_SettingsRoot.transform, "VolumeLabel", "MASTER VOLUME", 18, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, 55), new Vector2(500, 35));

            var sliderGo = Rect("Volume Slider", m_SettingsRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(500, 36));
            m_VolumeSlider = sliderGo.AddComponent<Slider>();
            m_VolumeSlider.minValue = 0;
            m_VolumeSlider.maxValue = 1;
            m_VolumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, 1);
            m_VolumeSlider.onValueChanged.AddListener(SetVolume);

            var track = Rect("Track", sliderGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).AddComponent<Image>();
            track.color = new Color(0.12f, 0.08f, 0.04f);
            track.raycastTarget = false;

            var fillArea = Rect("Fill Area", sliderGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(4, 8);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-4, -8);
            var fill = Rect("Fill", fillArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).AddComponent<Image>();
            fill.color = new Color(0.72f, 0.55f, 0.2f);

            var handleGo = Rect("Handle", sliderGo.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(30, 30));
            var handle = handleGo.AddComponent<Image>();
            handle.color = new Color(0.94f, 0.82f, 0.48f);
            m_VolumeSlider.fillRect = fill.GetComponent<RectTransform>();
            m_VolumeSlider.handleRect = handleGo.GetComponent<RectTransform>();
            handleGo.transform.SetAsLastSibling();

            var back = WoodButton(m_SettingsRoot.transform, "BACK", new Vector2(0, -155), new Vector2(320, 68));
            back.onClick.AddListener(CloseSettings);
            m_SettingsRoot.SetActive(false);
        }

        private void OpenSettings()
        {
            m_SettingsRoot.SetActive(true);
            m_MenuRoot.transform.Find("Menu Buttons").gameObject.SetActive(false);
            SetStatus("");
        }

        private void CloseSettings()
        {
            m_SettingsRoot.SetActive(false);
            m_MenuRoot.transform.Find("Menu Buttons").gameObject.SetActive(true);
        }

        private void StartNewJourney()
        {
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
            BeginGameplay();
        }

        private void ContinueJourney()
        {
            if (!HasSave())
            {
                SetStatus("NO SAVE DATA — START A NEW JOURNEY");
                return;
            }
            BeginGameplay();
        }

        private void BeginGameplay()
        {
            if (m_GameStarted) return;
            m_GameStarted = true;
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (m_Canvas != null) Destroy(m_Canvas.gameObject);
            Destroy(gameObject);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ToggleMute()
        {
            AudioListener.volume = AudioListener.volume > 0.01f ? 0 : PlayerPrefs.GetFloat(VolumeKey, 1);
        }

        private void ToggleFullscreen() => Screen.fullScreen = !Screen.fullScreen;

        private void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumeKey, value);
            PlayerPrefs.Save();
        }

        private void RefreshContinueState()
        {
            if (m_ContinueButton == null) return;
            m_ContinueButton.interactable = HasSave();
        }

        private bool HasSave() => PlayerPrefs.GetInt(SaveKey, 0) == 1;
        private void SetStatus(string message) { if (m_Status != null) m_Status.text = message; }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private static Button WoodButton(Transform parent, string label, Vector2 position, Vector2 size)
        {
            var go = Rect(label + " Button", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            var image = go.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.33f, 0.19f, 0.07f), new Color(0.11f, 0.055f, 0.018f));
            image.type = Image.Type.Sliced;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.12f, 1.03f, 0.78f);
            colors.pressedColor = new Color(0.85f, 0.72f, 0.45f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);
            button.colors = colors;

            var text = AddText(go.transform, "Label", label, 31, FontStyle.Bold, new Color(0.96f, 0.9f, 0.72f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            AddShadow(go, 5, -5, 0.8f);
            return button;
        }

        private Button SmallButton(string label, Vector2 position)
        {
            var go = Rect("Utility " + label, m_MenuRoot.transform, new Vector2(1, 0), new Vector2(1, 0), position, new Vector2(58, 58));
            var image = go.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.22f, 0.13f, 0.05f), new Color(0.06f, 0.03f, 0.01f));
            image.type = Image.Type.Sliced;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var text = AddText(go.transform, "Icon", label, 26, FontStyle.Bold, new Color(0.95f, 0.9f, 0.72f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            return button;
        }

        private static void WorldIcon(Transform parent, Vector2 position, bool campfire)
        {
            var icon = AddImage(parent, "World Icon", campfire ? new Color(0.82f, 0.43f, 0.08f, 0.85f) : new Color(0.34f, 0.7f, 0.18f, 0.75f),
                Vector2.zero, Vector2.zero, position, new Vector2(115, 115));
            icon.raycastTarget = false;
            var outline = icon.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.82f, 0.3f, 0.45f);
            outline.effectDistance = new Vector2(3, -3);
        }

        private static GameObject FullRect(string name, Transform parent)
        {
            return Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static GameObject Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return go;
        }

        private static Image AddImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var image = Rect(name, parent, anchorMin, anchorMax, position, size).AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text AddText(Transform parent, string name, string content, int size, FontStyle style, Color color, TextAnchor alignment,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 dimensions)
        {
            var text = Rect(name, parent, anchorMin, anchorMax, position, dimensions).AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, size - 10);
            text.resizeTextMaxSize = size;
            return text;
        }

        private static void AddShadow(GameObject go, float x, float y, float alpha)
        {
            var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, alpha);
            shadow.effectDistance = new Vector2(x, y);
            shadow.useGraphicAlpha = true;
        }

        private static Sprite WoodSprite(Color light, Color dark)
        {
            const int width = 128;
            const int height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.07f, y * 0.045f);
                    float lines = Mathf.Sin((x + y * 0.35f) * 0.18f) * 0.08f;
                    pixels[y * width + x] = Color.Lerp(dark, light, Mathf.Clamp01(0.35f + grain * 0.55f + lines));
                }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f, 0,
                SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
        }

        // Kept for compatibility with existing scene button events.
        public void BtnExit() => QuitGame();

        public void BtnLevel(int num)
        {
            if (m_GameplayData != null) m_GameplayData.LevelNumber = num;
            BeginGameplay();
        }
    }
}
