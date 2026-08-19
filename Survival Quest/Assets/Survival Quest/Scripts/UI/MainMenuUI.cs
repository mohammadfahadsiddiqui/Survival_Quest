using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalGame.ScriptableObjects;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    /// <summary>
    /// Runtime-generated Survival Quest main menu.
    /// The gameplay scene remains visible behind the UI, so the menu always matches the game's world.
    /// </summary>
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

            GameObject host = new GameObject("Survival Quest - Main Menu");
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

        private void Start()
        {
            BuildMenu();
        }

        private void BuildMenu()
        {
            if (m_Canvas != null) return;

            EnsureEventSystem();

            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = canvasObject.GetComponent<Canvas>();
            m_Canvas.transform.SetParent(transform, false);
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            m_MenuRoot = CreateRect("Menu", m_Canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Semi-transparent shades preserve the actual low-poly game scene behind the menu.
            AddImage(m_MenuRoot.transform, "Left Shade", new Color(0.015f, 0.045f, 0.018f, 0.62f),
                new Vector2(0f, 0f), new Vector2(0.50f, 1f), Vector2.zero, Vector2.zero);
            AddImage(m_MenuRoot.transform, "Right Shade", new Color(0.01f, 0.02f, 0.008f, 0.35f),
                new Vector2(0.56f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            BuildLeftMenu();
            BuildCards();
            BuildFooter();
            BuildSettings();

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void BuildLeftMenu()
        {
            Text title = AddText(m_MenuRoot.transform, "Title", "SURVIVAL QUEST", 62, FontStyle.Bold,
                new Color(0.98f, 0.95f, 0.78f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(55f, -48f), new Vector2(620f, 75f));
            AddShadow(title.gameObject, new Vector2(5f, -5f), 0.9f);

            Text subtitle = AddText(m_MenuRoot.transform, "Subtitle", "SURVIVE • EXPLORE • CRAFT • CONQUER", 18, FontStyle.Bold,
                new Color(0.72f, 0.90f, 0.35f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -102f), new Vector2(550f, 30f));
            AddShadow(subtitle.gameObject, new Vector2(2f, -2f), 0.7f);

            AddImage(m_MenuRoot.transform, "Title Line", new Color(0.56f, 0.35f, 0.12f, 0.95f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -132f), new Vector2(430f, 4f));

            GameObject menu = CreateRect("Menu Buttons", m_MenuRoot.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(55f, -10f), new Vector2(520f, 500f));

            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };
            for (int i = 0; i < labels.Length; i++)
            {
                Button button = CreateWoodButton(menu.transform, labels[i], new Vector2(260f, 190f - i * 88f), new Vector2(490f, 68f));

                if (i == 0 || i == 2) button.onClick.AddListener(StartNewJourney);
                else if (i == 1)
                {
                    m_ContinueButton = button;
                    button.onClick.AddListener(ContinueJourney);
                }
                else if (i == 3) button.onClick.AddListener(OpenSettings);
                else button.onClick.AddListener(QuitGame);
            }

            m_Status = AddText(menu.transform, "Status", "", 16, FontStyle.Bold, new Color(1f, 0.75f, 0.20f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(5f, 5f), new Vector2(490f, 25f));

            RefreshContinueState();
        }

        private void BuildCards()
        {
            GameObject top = CreateCard(new Vector2(-55f, 130f));
            AddText(top.transform, "Title", "SURVIVE THE UNKNOWN", 26, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(25f, 42f), new Vector2(455f, 40f));
            AddText(top.transform, "Body", "Explore • Gather • Craft • Survive", 17, FontStyle.Normal,
                new Color(0.88f, 0.94f, 0.75f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(25f, 10f), new Vector2(420f, 30f));
            AddText(top.transform, "World", "WILDERNESS", 13, FontStyle.Bold,
                new Color(0.72f, 0.90f, 0.35f), TextAnchor.MiddleRight,
                Vector2.zero, Vector2.zero, new Vector2(-25f, -95f), new Vector2(160f, 24f));

            GameObject bottom = CreateCard(new Vector2(-55f, -185f));
            AddText(bottom.transform, "Title", "YOUR JOURNEY", 26, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(25f, 42f), new Vector2(455f, 40f));
            AddText(bottom.transform, "Save", HasSave() ? "SAVE DATA FOUND" : "NO SAVE DATA", 18, FontStyle.Bold,
                HasSave() ? new Color(0.72f, 0.93f, 0.38f) : new Color(1f, 0.70f, 0.20f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(25f, -8f), new Vector2(350f, 30f));
            AddText(bottom.transform, "Body", "Begin a new survival expedition.", 17, FontStyle.Normal,
                new Color(0.86f, 0.90f, 0.78f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(25f, -45f), new Vector2(430f, 30f));
        }

        private GameObject CreateCard(Vector2 position)
        {
            GameObject card = CreateRect("Info Card", m_MenuRoot.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), position, new Vector2(545f, 250f));

            Image image = card.AddComponent<Image>();
            image.sprite = CreateWoodSprite(new Color(0.12f, 0.16f, 0.08f), new Color(0.035f, 0.05f, 0.025f));
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 0.92f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.52f, 0.34f, 0.14f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            AddShadow(card, new Vector2(6f, -6f), 0.7f);
            return card;
        }

        private void BuildFooter()
        {
            AddText(m_MenuRoot.transform, "Version", "VERSION 0.1", 13, FontStyle.Normal,
                new Color(0.78f, 0.88f, 0.58f, 0.85f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(32f, 35f), new Vector2(180f, 25f));
            AddText(m_MenuRoot.transform, "Copyright", "© SURVIVAL QUEST", 13, FontStyle.Normal,
                new Color(0.78f, 0.88f, 0.58f, 0.70f), TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.zero, new Vector2(32f, 10f), new Vector2(220f, 25f));

            Button mute = CreateSmallButton("♪", new Vector2(-135f, 35f));
            mute.onClick.AddListener(ToggleMute);
            Button fullscreen = CreateSmallButton("[]", new Vector2(-55f, 35f));
            fullscreen.onClick.AddListener(ToggleFullscreen);
        }

        private void BuildSettings()
        {
            m_SettingsRoot = CreateRect("Settings Panel", m_MenuRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 500f));

            Image bg = m_SettingsRoot.AddComponent<Image>();
            bg.sprite = CreateWoodSprite(new Color(0.09f, 0.13f, 0.055f), new Color(0.025f, 0.035f, 0.015f));
            bg.type = Image.Type.Sliced;

            Outline outline = m_SettingsRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0.60f, 0.42f, 0.18f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            AddText(m_SettingsRoot.transform, "Title", "SETTINGS", 38, FontStyle.Bold,
                new Color(0.96f, 0.90f, 0.70f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(600f, 55f));
            AddText(m_SettingsRoot.transform, "Volume", "MASTER VOLUME", 18, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250f, 55f), new Vector2(500f, 32f));

            GameObject sliderObject = CreateRect("Volume Slider", m_SettingsRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 5f), new Vector2(500f, 36f));
            m_VolumeSlider = sliderObject.AddComponent<Slider>();
            m_VolumeSlider.minValue = 0f;
            m_VolumeSlider.maxValue = 1f;
            m_VolumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, 1f);
            m_VolumeSlider.onValueChanged.AddListener(SetVolume);

            GameObject trackObject = CreateRect("Track", sliderObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image track = trackObject.AddComponent<Image>();
            track.color = new Color(0.12f, 0.08f, 0.035f);

            GameObject fillObject = CreateRect("Fill", sliderObject.transform, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color(0.72f, 0.55f, 0.20f);

            GameObject handleObject = CreateRect("Handle", sliderObject.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(30f, 30f));
            Image handle = handleObject.AddComponent<Image>();
            handle.color = new Color(0.94f, 0.82f, 0.48f);

            m_VolumeSlider.fillRect = fillObject.GetComponent<RectTransform>();
            m_VolumeSlider.handleRect = handleObject.GetComponent<RectTransform>();

            Button back = CreateWoodButton(m_SettingsRoot.transform, "BACK", new Vector2(0f, -150f), new Vector2(300f, 65f));
            back.onClick.AddListener(CloseSettings);

            m_SettingsRoot.SetActive(false);
        }

        private void OpenSettings()
        {
            m_SettingsRoot.SetActive(true);
            m_MenuRoot.transform.Find("Menu Buttons").gameObject.SetActive(false);
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
                if (m_Status != null) m_Status.text = "NO SAVE DATA — START A NEW JOURNEY";
                return;
            }
            BeginGameplay();
        }

        private void BeginGameplay()
        {
            if (m_GameStarted) return;
            m_GameStarted = true;
            Time.timeScale = 1f;
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
            AudioListener.volume = AudioListener.volume > 0.01f ? 0f : PlayerPrefs.GetFloat(VolumeKey, 1f);
        }

        private void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        private void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumeKey, value);
            PlayerPrefs.Save();
        }

        private void RefreshContinueState()
        {
            if (m_ContinueButton != null) m_ContinueButton.interactable = HasSave();
        }

        private bool HasSave()
        {
            return PlayerPrefs.GetInt(SaveKey, 0) == 1;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private static Button CreateWoodButton(Transform parent, string label, Vector2 position, Vector2 size)
        {
            GameObject buttonObject = CreateRect(label + " Button", parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            Image image = buttonObject.AddComponent<Image>();
            image.sprite = CreateWoodSprite(new Color(0.33f, 0.19f, 0.07f), new Color(0.10f, 0.05f, 0.015f));
            image.type = Image.Type.Sliced;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.95f, 0.75f);
            colors.pressedColor = new Color(0.85f, 0.72f, 0.45f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.45f);
            button.colors = colors;

            Text text = AddText(buttonObject.transform, "Label", label, 30, FontStyle.Bold,
                new Color(0.96f, 0.90f, 0.72f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            AddShadow(buttonObject, new Vector2(5f, -5f), 0.8f);
            return button;
        }

        private Button CreateSmallButton(string label, Vector2 position)
        {
            GameObject objectRoot = CreateRect("Utility " + label, m_MenuRoot.transform,
                new Vector2(1f, 0f), new Vector2(1f, 0f), position, new Vector2(58f, 58f));
            Image image = objectRoot.AddComponent<Image>();
            image.sprite = CreateWoodSprite(new Color(0.22f, 0.13f, 0.05f), new Color(0.06f, 0.03f, 0.01f));
            image.type = Image.Type.Sliced;
            Button button = objectRoot.AddComponent<Button>();
            button.targetGraphic = image;

            Text text = AddText(objectRoot.transform, "Icon", label, 24, FontStyle.Bold,
                new Color(0.95f, 0.90f, 0.72f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            return button;
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return go;
        }

        private static Image AddImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            Image image = CreateRect(name, parent, anchorMin, anchorMax, position, size).AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text AddText(Transform parent, string name, string content, int fontSize, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            Text text = CreateRect(name, parent, anchorMin, anchorMax, position, size).AddComponent<Text>();

            // Unity 6 removed Arial.ttf as a built-in runtime font. LegacyRuntime.ttf is the supported replacement.
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void AddShadow(GameObject target, Vector2 distance, float alpha)
        {
            Shadow shadow = target.GetComponent<Shadow>();
            if (shadow == null) shadow = target.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, alpha);
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static Sprite CreateWoodSprite(Color light, Color dark)
        {
            const int width = 128;
            const int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.07f, y * 0.045f);
                    float lines = Mathf.Sin((x + y * 0.35f) * 0.18f) * 0.08f;
                    float value = Mathf.Clamp01(0.35f + grain * 0.55f + lines);
                    pixels[y * width + x] = Color.Lerp(dark, light, value);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f, 0,
                SpriteMeshType.FullRect, new Vector4(14f, 14f, 14f, 14f));
        }

        // Compatibility with older scene button events.
        public void BtnExit() => QuitGame();

        public void BtnLevel(int num)
        {
            if (m_GameplayData != null) m_GameplayData.LevelNumber = num;
            BeginGameplay();
        }
    }
}
