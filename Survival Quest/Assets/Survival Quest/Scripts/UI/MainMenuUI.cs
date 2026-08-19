using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using SurvivalGame.ScriptableObjects;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    /// <summary>
    /// Self-contained Survival Quest main menu for MainMenu.unity.
    /// Uses only runtime-generated Unity UI, so it does not require scene references or a Dropdown template.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;

        private Canvas m_Canvas;
        private GameObject m_Root;
        private GameObject m_Settings;
        private Text m_Status;
        private Button m_ContinueButton;
        private Toggle m_InvertToggle;
        private Toggle m_FullscreenToggle;
        private Text m_QualityValue;
        private bool m_LoadingGame;
        private bool m_Built;

        private const string GAME_SCENE = "SampleScene";
        private const string SAVE = "SurvivalQuest.SaveExists";
        private const string MASTER = "SurvivalQuest.MasterVolume";
        private const string MUSIC = "SurvivalQuest.MusicVolume";
        private const string SFX = "SurvivalQuest.SfxVolume";
        private const string SENS = "SurvivalQuest.Sensitivity";
        private const string INVERT = "SurvivalQuest.InvertY";

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            EnsureSingleEventSystem();
        }

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (m_Built) return;
            m_Built = true;

            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = canvasObject.GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            m_Root = Rect("Root", m_Canvas.transform, Vector2.zero, Vector2.one);

            // Low-poly Survival Quest backdrop. It intentionally uses the game's visual language.
            RawImage background = Raw("Wilderness Background", m_Root.transform, Vector2.zero, Vector2.one);
            background.texture = MakeLowPolyBackground(1280, 720);

            AddImage(m_Root.transform, "Cinematic Vignette", new Color(0f, 0.02f, 0f, 0.34f), Vector2.zero, Vector2.one);
            AddImage(m_Root.transform, "Left Readability Shade", new Color(0.005f, 0.015f, 0.002f, 0.58f), new Vector2(0f, 0f), new Vector2(0.48f, 1f));
            AddImage(m_Root.transform, "Right Readability Shade", new Color(0.005f, 0.012f, 0.002f, 0.36f), new Vector2(0.70f, 0f), new Vector2(1f, 1f));

            BuildMainMenu();
            BuildInfoCards();
            BuildFooter();
            BuildSettings();
        }

        private void BuildMainMenu()
        {
            Text title = TextUI(m_Root.transform, "Title", "SURVIVAL QUEST", 64, FontStyle.Bold,
                new Color(0.98f, 0.94f, 0.77f), TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.82f), new Vector2(0.48f, 0.94f));
            Shadow(title.gameObject, new Vector2(5f, -5f), 0.9f);

            Text subtitle = TextUI(m_Root.transform, "Subtitle", "THE WILDERNESS DOESN'T FORGIVE", 17, FontStyle.Bold,
                new Color(0.74f, 0.88f, 0.48f), TextAnchor.MiddleLeft,
                new Vector2(0.058f, 0.775f), new Vector2(0.48f, 0.82f));
            Shadow(subtitle.gameObject, new Vector2(2f, -2f), 0.75f);

            AddImage(m_Root.transform, "Title Accent", new Color(0.72f, 0.49f, 0.18f, 0.95f),
                new Vector2(0.058f, 0.758f), new Vector2(0.34f, 0.764f));

            GameObject menu = Rect("Main Menu Buttons", m_Root.transform, new Vector2(0.055f, 0.18f), new Vector2(0.44f, 0.73f));
            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };

            for (int i = 0; i < labels.Length; i++)
            {
                float top = 0.96f - i * 0.18f;
                float bottom = top - 0.125f;
                Button button = WoodButton(menu.transform, labels[i], new Vector2(0.02f, bottom), new Vector2(0.92f, top));

                if (i == 0 || i == 2)
                    button.onClick.AddListener(StartNew);
                else if (i == 1)
                {
                    m_ContinueButton = button;
                    button.onClick.AddListener(ContinueGame);
                }
                else if (i == 3)
                    button.onClick.AddListener(OpenSettings);
                else
                    button.onClick.AddListener(QuitGame);
            }

            m_Status = TextUI(menu.transform, "Status", "", 13, FontStyle.Bold,
                new Color(1f, 0.76f, 0.25f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0f), new Vector2(0.92f, 0.055f));

            m_ContinueButton.interactable = HasSave();
        }

        private void BuildInfoCards()
        {
            CreateInfoCard("SURVIVE THE UNKNOWN", "Explore • Gather • Craft • Survive", "FIELD GUIDE", 0.58f, 0.69f, 0.965f, 0.91f);
            CreateInfoCard("YOUR JOURNEY", HasSave() ? "SAVE DATA FOUND" : "NO SAVE DATA", "EXPEDITION STATUS", 0.58f, 0.40f, 0.965f, 0.62f);
        }

        private void CreateInfoCard(string title, string body, string tag, float xmin, float ymin, float xmax, float ymax)
        {
            GameObject card = Rect(title + " Card", m_Root.transform, new Vector2(xmin, ymin), new Vector2(xmax, ymax));
            Image image = card.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.13f, 0.18f, 0.08f), new Color(0.015f, 0.025f, 0.01f));
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 0.93f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.52f, 0.36f, 0.15f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);
            Shadow(card, new Vector2(6f, -6f), 0.72f);

            TextUI(card.transform, "Title", title, 21, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.52f), new Vector2(0.68f, 0.84f));
            TextUI(card.transform, "Body", body, 13, FontStyle.Normal, new Color(0.85f, 0.92f, 0.72f), TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.30f), new Vector2(0.68f, 0.52f));
            TextUI(card.transform, "Tag", tag, 9, FontStyle.Bold, new Color(0.69f, 0.80f, 0.49f), TextAnchor.MiddleRight,
                new Vector2(0.45f, 0.08f), new Vector2(0.94f, 0.20f));

            // Decorative low-poly art window, matching the actual game rather than using unrelated imagery.
            RawImage art = Raw("Art", card.transform, new Vector2(0.73f, 0.13f), new Vector2(0.94f, 0.48f));
            art.texture = MakeCardArt(240, 110);
            art.color = new Color(1f, 1f, 1f, 0.88f);
        }

        private void BuildFooter()
        {
            TextUI(m_Root.transform, "Version", "VERSION 0.1", 11, FontStyle.Normal,
                new Color(0.80f, 0.88f, 0.64f, 0.90f), TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.038f), new Vector2(0.20f, 0.066f));
            TextUI(m_Root.transform, "Copyright", "© SURVIVAL QUEST", 11, FontStyle.Normal,
                new Color(0.80f, 0.88f, 0.64f, 0.70f), TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.008f), new Vector2(0.25f, 0.036f));

            Button mute = UtilityButton("♪", 0.885f);
            mute.onClick.AddListener(ToggleMute);
            Button fullscreen = UtilityButton("□", 0.94f);
            fullscreen.onClick.AddListener(ToggleFullscreen);
        }

        private void BuildSettings()
        {
            m_Settings = Rect("Settings Panel", m_Root.transform, new Vector2(0.17f, 0.09f), new Vector2(0.83f, 0.91f));

            Image panel = m_Settings.AddComponent<Image>();
            panel.sprite = WoodSprite(new Color(0.075f, 0.105f, 0.045f), new Color(0.012f, 0.020f, 0.008f));
            panel.type = Image.Type.Sliced;
            panel.color = new Color(1f, 1f, 1f, 0.97f);

            Outline outline = m_Settings.AddComponent<Outline>();
            outline.effectColor = new Color(0.62f, 0.43f, 0.18f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);
            Shadow(m_Settings, new Vector2(8f, -8f), 0.85f);

            TextUI(m_Settings.transform, "Title", "SETTINGS", 36, FontStyle.Bold,
                new Color(0.97f, 0.91f, 0.72f), TextAnchor.MiddleLeft,
                new Vector2(0.065f, 0.87f), new Vector2(0.94f, 0.96f));
            TextUI(m_Settings.transform, "Subtitle", "GAMEPLAY   •   AUDIO   •   DISPLAY", 11, FontStyle.Bold,
                new Color(0.68f, 0.80f, 0.48f), TextAnchor.MiddleLeft,
                new Vector2(0.068f, 0.81f), new Vector2(0.94f, 0.86f));

            SectionHeader("AUDIO", 0.735f);
            CreateSliderRow("Master Volume", 0.665f, PlayerPrefs.GetFloat(MASTER, 1f), v => Save(MASTER, v));
            CreateSliderRow("Music Volume", 0.575f, PlayerPrefs.GetFloat(MUSIC, 0.8f), v => Save(MUSIC, v));
            CreateSliderRow("SFX Volume", 0.485f, PlayerPrefs.GetFloat(SFX, 0.9f), v => Save(MUSIC, PlayerPrefs.GetFloat(MUSIC, 0.8f)));

            SectionHeader("GAMEPLAY", 0.395f);
            CreateSliderRow("Camera Sensitivity", 0.325f, PlayerPrefs.GetFloat(SENS, 0.5f), v => Save(SENS, v));
            m_InvertToggle = CreateToggleRow("Invert Y Axis", 0.235f, PlayerPrefs.GetInt(INVERT, 0) == 1,
                value => { PlayerPrefs.SetInt(INVERT, value ? 1 : 0); PlayerPrefs.Save(); });

            SectionHeader("DISPLAY", 0.145f);
            m_FullscreenToggle = CreateToggleRow("Fullscreen", 0.075f, Screen.fullScreen,
                value => Screen.fullScreen = value);
            CreateQualityRow(0.075f);

            Button back = WoodButton(m_Settings.transform, "BACK", new Vector2(0.70f, 0.015f), new Vector2(0.93f, 0.09f));
            back.onClick.AddListener(CloseSettings);

            m_Settings.SetActive(false);
        }

        private void SectionHeader(string label, float y)
        {
            TextUI(m_Settings.transform, label + " Label", label, 12, FontStyle.Bold,
                new Color(0.72f, 0.84f, 0.50f), TextAnchor.MiddleLeft,
                new Vector2(0.065f, y), new Vector2(0.35f, y + 0.04f));
            AddImage(m_Settings.transform, label + " Divider", new Color(0.42f, 0.31f, 0.14f, 0.80f),
                new Vector2(0.065f, y - 0.012f), new Vector2(0.935f, y - 0.008f));
        }

        private Slider CreateSliderRow(string label, float y, float value, UnityAction<float> callback)
        {
            TextUI(m_Settings.transform, label + " Label", label, 14, FontStyle.Bold,
                new Color(0.92f, 0.90f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.08f, y), new Vector2(0.39f, y + 0.055f));

            GameObject sliderObject = Rect(label + " Slider", m_Settings.transform,
                new Vector2(0.43f, y + 0.008f), new Vector2(0.90f, y + 0.058f));
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = Mathf.Clamp01(value);
            slider.onValueChanged.AddListener(callback);

            Image track = Rect("Track", sliderObject.transform, Vector2.zero, Vector2.one).AddComponent<Image>();
            track.color = new Color(0.07f, 0.055f, 0.025f);
            track.raycastTarget = false;

            GameObject fillObject = Rect("Fill", sliderObject.transform, Vector2.zero, new Vector2(0.5f, 1f));
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color(0.70f, 0.52f, 0.19f);
            fill.raycastTarget = false;

            GameObject handleObject = Rect("Handle", sliderObject.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(28f, 28f);
            Image handle = handleObject.AddComponent<Image>();
            handle.color = new Color(0.96f, 0.82f, 0.46f);
            handle.raycastTarget = false;

            slider.fillRect = fillObject.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            return slider;
        }

        private Toggle CreateToggleRow(string label, float y, bool value, UnityAction<bool> callback)
        {
            TextUI(m_Settings.transform, label + " Label", label, 14, FontStyle.Bold,
                new Color(0.92f, 0.90f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.08f, y), new Vector2(0.50f, y + 0.055f));

            GameObject toggleObject = Rect(label + " Toggle", m_Settings.transform,
                new Vector2(0.80f, y + 0.004f), new Vector2(0.90f, y + 0.065f));
            Image background = toggleObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.065f, 0.03f);

            Toggle toggle = toggleObject.AddComponent<Toggle>();
            toggle.isOn = value;
            toggle.targetGraphic = background;
            toggle.onValueChanged.AddListener(callback);

            GameObject checkObject = Rect("Check", toggleObject.transform,
                new Vector2(0.08f, 0.16f), new Vector2(0.45f, 0.84f));
            Image check = checkObject.AddComponent<Image>();
            check.color = new Color(0.72f, 0.55f, 0.20f);
            check.raycastTarget = false;
            toggle.graphic = check;
            return toggle;
        }

        private void CreateQualityRow(float y)
        {
            TextUI(m_Settings.transform, "Graphics Quality Label", "Graphics Quality", 14, FontStyle.Bold,
                new Color(0.92f, 0.90f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.08f, y), new Vector2(0.50f, y + 0.055f));

            GameObject buttonObject = Rect("Graphics Quality Button", m_Settings.transform,
                new Vector2(0.59f, y + 0.002f), new Vector2(0.90f, y + 0.064f));
            Image image = buttonObject.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.25f, 0.15f, 0.055f), new Color(0.07f, 0.035f, 0.01f));
            image.type = Image.Type.Sliced;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            m_QualityValue = TextUI(buttonObject.transform, "Value", CurrentQualityName(), 13, FontStyle.Bold,
                new Color(0.96f, 0.90f, 0.72f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            m_QualityValue.raycastTarget = false;
            button.onClick.AddListener(CycleQuality);
        }

        private void CycleQuality()
        {
            string[] names = { "Low", "Medium", "High", "Ultra" };
            int current = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, names.Length - 1);
            int next = (current + 1) % names.Length;
            int actual = Mathf.Clamp(next, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            QualitySettings.SetQualityLevel(actual);
            if (m_QualityValue != null) m_QualityValue.text = names[next];
        }

        private string CurrentQualityName()
        {
            string[] names = { "Low", "Medium", "High", "Ultra" };
            return names[Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, names.Length - 1)];
        }

        private void OpenSettings()
        {
            if (m_Settings != null) m_Settings.SetActive(true);
        }

        private void CloseSettings()
        {
            if (m_Settings != null) m_Settings.SetActive(false);
        }

        private void StartNew()
        {
            PlayerPrefs.SetInt(SAVE, 1);
            PlayerPrefs.Save();
            LoadGame();
        }

        private void ContinueGame()
        {
            if (!HasSave())
            {
                if (m_Status != null) m_Status.text = "NO SAVE DATA — START A NEW JOURNEY";
                return;
            }
            LoadGame();
        }

        private void LoadGame()
        {
            if (m_LoadingGame) return;
            m_LoadingGame = true;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            SceneManager.LoadScene(GAME_SCENE);
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
            AudioListener.volume = AudioListener.volume > 0.01f ? 0f : PlayerPrefs.GetFloat(MASTER, 1f);
        }

        private void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            if (m_FullscreenToggle != null) m_FullscreenToggle.isOn = Screen.fullScreen;
        }

        private void Save(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            if (key == MASTER) AudioListener.volume = value;
        }

        private bool HasSave()
        {
            return PlayerPrefs.GetInt(SAVE, 0) == 1;
        }

        private static void EnsureSingleEventSystem()
        {
            UnityEngine.EventSystems.EventSystem[] systems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if (systems.Length > 0)
            {
                for (int i = 1; i < systems.Length; i++)
                    Destroy(systems[i].gameObject);
                return;
            }

            GameObject go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            return go;
        }

        private static Image AddImage(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            Image image = Rect(name, parent, min, max).AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RawImage Raw(string name, Transform parent, Vector2 min, Vector2 max)
        {
            return Rect(name, parent, min, max).AddComponent<RawImage>();
        }

        private static Text TextUI(Transform parent, string name, string value, int size, FontStyle style,
            Color color, TextAnchor alignment, Vector2 min, Vector2 max)
        {
            Text text = Rect(name, parent, min, max).AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void Shadow(GameObject target, Vector2 distance, float alpha)
        {
            Shadow shadow = target.GetComponent<Shadow>() ?? target.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, alpha);
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static Button WoodButton(Transform parent, string label, Vector2 min, Vector2 max)
        {
            GameObject go = Rect(label + " Button", parent, min, max);
            Image image = go.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.34f, 0.20f, 0.07f), new Color(0.09f, 0.045f, 0.012f));
            image.type = Image.Type.Sliced;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.94f, 0.72f);
            colors.pressedColor = new Color(0.82f, 0.67f, 0.39f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.45f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Text text = TextUI(go.transform, "Label", label, 28, FontStyle.Bold,
                new Color(0.97f, 0.91f, 0.72f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            text.raycastTarget = false;
            Shadow(go, new Vector2(4f, -4f), 0.8f);
            return button;
        }

        private Button UtilityButton(string label, float x)
        {
            GameObject go = Rect("Utility " + label, m_Root.transform,
                new Vector2(x, 0.035f), new Vector2(x + 0.045f, 0.095f));
            Image image = go.AddComponent<Image>();
            image.sprite = WoodSprite(new Color(0.20f, 0.12f, 0.04f), new Color(0.055f, 0.025f, 0.008f));
            image.type = Image.Type.Sliced;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = TextUI(go.transform, "Icon", label, 18, FontStyle.Bold,
                new Color(0.95f, 0.88f, 0.68f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            text.raycastTarget = false;
            return button;
        }

        private static Sprite WoodSprite(Color center, Color edge)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float border = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    float grain = Mathf.Sin(y * 0.75f + x * 0.08f) * 0.035f;
                    float t = Mathf.Clamp01(border / 7f);
                    Color color = Color.Lerp(edge, center, t);
                    color.r += grain;
                    color.g += grain;
                    color.b += grain;
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(8f, 8f, 8f, 8f));
        }

        private static Texture2D MakeLowPolyBackground(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                float ny = y / (float)height;
                for (int x = 0; x < width; x++)
                {
                    float nx = x / (float)width;
                    Color sky = Color.Lerp(new Color(0.055f, 0.095f, 0.035f), new Color(0.16f, 0.27f, 0.10f), ny);
                    Color ground = Color.Lerp(new Color(0.10f, 0.18f, 0.035f), new Color(0.30f, 0.52f, 0.09f), Mathf.Clamp01((ny - 0.42f) * 1.6f));
                    Color color = ny < 0.43f ? sky : ground;

                    // Distant blocky cliffs.
                    float ridge = 0.22f + 0.04f * Mathf.Sin(nx * 9f) + 0.025f * Mathf.Sin(nx * 23f);
                    if (ny > 0.34f && ny < ridge + 0.15f)
                        color = Color.Lerp(color, new Color(0.28f, 0.18f, 0.09f), 0.75f);

                    // Low-poly ground bands.
                    float grid = Mathf.Abs(Mathf.Sin(nx * 70f) * Mathf.Sin(ny * 52f));
                    if (ny > 0.45f) color *= 0.92f + grid * 0.08f;

                    tex.SetPixel(x, y, color);
                }
            }

            // Simple low-poly tree silhouettes and rocks are stamped after the base gradient.
            DrawTree(tex, 145, 390, 150, 250);
            DrawTree(tex, 300, 335, 120, 220);
            DrawTree(tex, 1060, 345, 130, 240);
            DrawTree(tex, 1165, 420, 105, 210);
            DrawRock(tex, 620, 430, 180, 110);
            DrawRock(tex, 960, 405, 130, 85);
            DrawCabin(tex, 790, 320, 190, 125);
            return tex;
        }

        private static void DrawTree(Texture2D tex, int cx, int baseY, int height, int width)
        {
            int minX = Mathf.Max(0, cx - width / 2);
            int maxX = Mathf.Min(tex.width - 1, cx + width / 2);
            int top = Mathf.Clamp(baseY - height, 0, tex.height - 1);
            Color trunk = new Color(0.18f, 0.09f, 0.035f);
            Color leaves = new Color(0.08f, 0.23f, 0.045f);

            for (int y = top; y <= baseY; y++)
            {
                float p = (y - top) / (float)Mathf.Max(1, baseY - top);
                int half = Mathf.RoundToInt(Mathf.Lerp(width * 0.12f, width * 0.5f, p));
                for (int x = Mathf.Max(minX, cx - half); x <= Mathf.Min(maxX, cx + half); x++)
                {
                    float edge = Mathf.Abs(x - cx) / (float)Mathf.Max(1, half);
                    if (edge < 1f && y < baseY - 18)
                        tex.SetPixel(x, y, Color.Lerp(leaves, new Color(0.025f, 0.08f, 0.02f), edge));
                }
            }

            for (int y = baseY - 8; y <= baseY + 20; y++)
            {
                for (int x = cx - 8; x <= cx + 8; x++)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, trunk);
            }
        }

        private static void DrawRock(Texture2D tex, int cx, int cy, int width, int height)
        {
            Color rock = new Color(0.22f, 0.27f, 0.24f);
            for (int y = Mathf.Max(0, cy - height / 2); y < Mathf.Min(tex.height, cy + height / 2); y++)
            {
                float py = (y - (cy - height / 2f)) / height;
                float half = Mathf.Sin(py * Mathf.PI) * width * 0.5f;
                for (int x = Mathf.Max(0, cx - (int)half); x < Mathf.Min(tex.width, cx + (int)half); x++)
                    tex.SetPixel(x, y, Color.Lerp(rock, new Color(0.12f, 0.15f, 0.14f), py * 0.45f));
            }
        }

        private static void DrawCabin(Texture2D tex, int cx, int cy, int width, int height)
        {
            int left = cx - width / 2;
            int right = cx + width / 2;
            int bottom = cy;
            int top = cy + height / 2;
            Color wall = new Color(0.30f, 0.19f, 0.09f);
            Color roof = new Color(0.07f, 0.12f, 0.14f);
            Color door = new Color(0.12f, 0.065f, 0.025f);

            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, wall);

            for (int y = bottom + height / 2; y < top + height / 2; y++)
            {
                float row = (y - (bottom + height / 2f)) / (height * 0.5f);
                int half = Mathf.RoundToInt(width * 0.5f * (1f - row));
                for (int x = cx - half; x <= cx + half; x++)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, roof);
            }

            for (int y = bottom; y < bottom + height / 2; y++)
                for (int x = cx - 16; x < cx + 16; x++)
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height) tex.SetPixel(x, y, door);
        }

        private static Texture2D MakeCardArt(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float ny = y / (float)height;
                    float nx = x / (float)width;
                    Color c = Color.Lerp(new Color(0.08f, 0.15f, 0.04f), new Color(0.28f, 0.45f, 0.08f), ny);
                    c *= 0.82f + Mathf.Sin(nx * 17f) * 0.04f;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            DrawTree(tex, width / 5, height - 10, height / 2, width / 4);
            DrawTree(tex, width * 4 / 5, height - 5, height * 3 / 5, width / 3);
            DrawRock(tex, width / 2, height * 3 / 4, width / 3, height / 2);
            tex.Apply();
            return tex;
        }
    }
}
