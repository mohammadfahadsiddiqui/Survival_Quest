using System;
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
    /// It intentionally uses the current gameplay camera as the background so the menu
    /// always matches the low-poly world shipped with the project.
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
            if (SceneManager.GetActiveScene().name != "SampleScene")
                return;

            if (FindObjectOfType<MainMenuUI>() != null)
                return;

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

        private void Start()
        {
            BuildMenu();
        }

        private void BuildMenu()
        {
            if (m_Canvas != null)
                return;

            EnsureEventSystem();

            m_Canvas = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
                .GetComponent<Canvas>();
            m_Canvas.transform.SetParent(transform, false);
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 500;

            var scaler = m_Canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            m_MenuRoot = CreateRect("Menu", m_Canvas.transform, Vector2.zero, Vector2.one);
            CreateBackgroundShade(m_MenuRoot.transform);
            BuildLeftMenu(m_MenuRoot.transform);
            BuildRightCards(m_MenuRoot.transform);
            BuildFooter(m_MenuRoot.transform);
            BuildSettingsPanel(m_MenuRoot.transform);

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void CreateBackgroundShade(Transform parent)
        {
            // A subtle vignette/side shade keeps the menu readable without replacing the
            // actual low-poly world rendered by the game's camera.
            var leftGo = CreateRect("Left Shade", parent, new Vector2(0f, 0f), new Vector2(0.48f, 1f));
            var left = leftGo.AddComponent<Image>();
            left.color = new Color(0.02f, 0.07f, 0.03f, 0.62f);
            left.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(4f, 0f);

            var rightGo = CreateRect("Right Shade", parent, new Vector2(0.58f, 0f), new Vector2(1f, 1f));
            var right = rightGo.AddComponent<Image>();
            right.color = new Color(0.01f, 0.025f, 0.01f, 0.45f);
        }

        private void BuildLeftMenu(Transform parent)
        {
            var title = CreateText("Title", parent, "SURVIVAL QUEST", 64, FontStyle.Bold,
                new Color(0.98f, 0.96f, 0.82f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(58f, -38f), new Vector2(650f, 78f));
            AddShadow(title, 5f, 5f, 0.9f);

            var subtitle = CreateText("Subtitle", parent, "SURVIVE • EXPLORE • CRAFT • CONQUER", 18, FontStyle.Bold,
                new Color(0.73f, 0.92f, 0.38f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(62f, -104f), new Vector2(560f, 32f));
            AddShadow(subtitle, 2f, 2f, 0.8f);

            CreateImage("Title Line", parent, new Color(0.56f, 0.35f, 0.12f, 0.9f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -132f), new Vector2(430f, 4f));

            var menu = CreateRect("Menu Buttons", parent, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(55f, -20f), new Vector2(530f, 485f));

            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };
            for (int i = 0; i < labels.Length; i++)
            {
                float y = 190f - i * 90f;
                var button = CreateWoodButton(menu.transform, labels[i], new Vector2(265f, y), new Vector2(500f, 72f));

                if (i == 0)
                    button.onClick.AddListener(StartNewJourney);
                else if (i == 1)
                {
                    m_ContinueButton = button;
                    button.onClick.AddListener(ContinueJourney);
                    RefreshContinueState();
                }
                else if (i == 2)
                    button.onClick.AddListener(StartNewJourney);
                else if (i == 3)
                    button.onClick.AddListener(OpenSettings);
                else
                    button.onClick.AddListener(QuitGame);
            }

            m_Status = CreateText("Status", menu.transform, "", 17, FontStyle.Bold,
                new Color(1f, 0.78f, 0.25f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(10f, 8f), new Vector2(500f, 28f));
        }

        private void BuildRightCards(Transform parent)
        {
            var top = CreateCard(parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-55f, 125f), new Vector2(545f, 270f));
            CreateText("CardTitle", top.transform, "SURVIVE THE UNKNOWN", 27, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(26f, 30f), new Vector2(480f, 42f));
            CreateText("CardBody", top.transform, "Explore • Gather • Craft • Survive", 17, FontStyle.Normal,
                new Color(0.9f, 0.95f, 0.78f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(26f, 8f), new Vector2(480f, 30f));
            CreateWorldIcon(top.transform, new Vector2(445f, 175f));

            var bottom = CreateCard(parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-55f, -190f), new Vector2(545f, 270f));
            CreateText("JourneyTitle", bottom.transform, "YOUR JOURNEY", 27, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(26f, 32f), new Vector2(480f, 42f));
            CreateText("Save", bottom.transform, HasSave() ? "SAVE DATA FOUND" : "NO SAVE DATA", 18, FontStyle.Bold,
                HasSave() ? new Color(0.72f, 0.93f, 0.38f, 1f) : new Color(1f, 0.72f, 0.24f, 1f),
                TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(26f, -8f), new Vector2(300f, 30f));
            CreateText("JourneyBody", bottom.transform, "Begin a new survival expedition.", 17, FontStyle.Normal,
                new Color(0.88f, 0.91f, 0.82f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(26f, -42f), new Vector2(430f, 30f));
            CreateWorldIcon(bottom.transform, new Vector2(445f, 165f), true);
        }

        private GameObject CreateCard(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = CreateRect("Info Card", parent, anchorMin, anchorMax, pos, size);
            var image = go.AddComponent<Image>();
            image.sprite = MakeWoodSprite(new Color(0.13f, 0.17f, 0.09f), new Color(0.05f, 0.07f, 0.035f));
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 1f, 1f, 0.94f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.37f, 0.16f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            AddShadow(go, 7f, -7f, 0.75f);
            return go;
        }

        private void BuildFooter(Transform parent)
        {
            CreateText("Version", parent, "VERSION 0.1", 14, FontStyle.Normal,
                new Color(0.8f, 0.9f, 0.62f, 0.85f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, 32f), new Vector2(180f, 25f));
            CreateText("Copyright", parent, "© SURVIVAL QUEST", 14, FontStyle.Normal,
                new Color(0.8f, 0.9f, 0.62f, 0.7f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, 8f), new Vector2(220f, 25f));

            var audio = CreateSmallButton(parent, "♪", new Vector2(-135f, 35f), new Vector2(58f, 58f));
            audio.onClick.AddListener(ToggleMute);
            var fullscreen = CreateSmallButton(parent, "⛶", new Vector2(-55f, 35f), new Vector2(58f, 58f));
            fullscreen.onClick.AddListener(ToggleFullscreen);
        }

        private void BuildSettingsPanel(Transform parent)
        {
            m_SettingsRoot = CreateRect("Settings Panel", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(720f, 520f));
            var bg = m_SettingsRoot.AddComponent<Image>();
            bg.sprite = MakeWoodSprite(new Color(0.09f, 0.13f, 0.06f), new Color(0.025f, 0.04f, 0.02f));
            bg.type = Image.Type.Sliced;
            var outline = m_SettingsRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.42f, 0.18f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            CreateText("SettingsTitle", m_SettingsRoot.transform, "SETTINGS", 40, FontStyle.Bold,
                new Color(0.96f, 0.9f, 0.7f, 1f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(600f, 60f));
            CreateText("VolumeLabel", m_SettingsRoot.transform, "MASTER VOLUME", 18, FontStyle.Bold,
                Color.white, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-250f, 55f), new Vector2(500f, 35f));

            var sliderGo = CreateRect("Volume Slider", m_SettingsRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 5f), new Vector2(500f, 36f));
            m_VolumeSlider = sliderGo.AddComponent<Slider>();
            m_VolumeSlider.minValue = 0f;
            m_VolumeSlider.maxValue = 1f;
            m_VolumeSlider.value = PlayerPrefs.GetFloat(VolumeKey, 1f);
            m_VolumeSlider.onValueChanged.AddListener(SetVolume);

            var trackGo = CreateRect("Track", sliderGo.transform, Vector2.zero, Vector2.one);
            var track = trackGo.AddComponent<Image>();
            track.color = new Color(0.12f, 0.08f, 0.04f, 1f);
            track.raycastTarget = false;

            var fillArea = CreateRect("Fill Area", sliderGo.transform, Vector2.zero, Vector2.one);
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 8f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -8f);
            var fillGo = CreateRect("Fill", fillArea.transform, Vector2.zero, Vector2.one);
            var fill = fillGo.AddComponent<Image>();
            fill.color = new Color(0.72f, 0.55f, 0.2f, 1f);

            var handleGo = CreateRect("Handle", sliderGo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(30f, 30f));
            var handle = handleGo.AddComponent<Image>();
            handle.color = new Color(0.94f, 0.82f, 0.48f, 1f);

            m_VolumeSlider.fillRect = fillGo.GetComponent<RectTransform>();
            m_VolumeSlider.handleRect = handleGo.GetComponent<RectTransform>();
            m_VolumeSlider.direction = Slider.Direction.LeftToRight;
            handleGo.transform.SetAsLastSibling();

            var back = CreateWoodButton(m_SettingsRoot.transform, "BACK", new Vector2(0f, -155f), new Vector2(320f, 68f));
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
            if (m_GameStarted)
                return;

            m_GameStarted = true;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (m_Canvas != null)
                Destroy(m_Canvas.gameObject);

            Destroy(gameObject);
        }

        private void QuitGame()
        {
            SetStatus("LEAVING THE WILDERNESS...");
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
            if (m_ContinueButton == null)
                return;
            bool hasSave = HasSave();
            m_ContinueButton.interactable = hasSave;
            var colors = m_ContinueButton.colors;
            colors.normalColor = hasSave ? Color.white : new Color(1f, 1f, 1f, 0.42f);
            m_ContinueButton.colors = colors;
        }

        private bool HasSave() => PlayerPrefs.GetInt(SaveKey, 0) == 1;

        private void SetStatus(string message)
        {
            if (m_Status == null) return;
            m_Status.text = message;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null) return;

            var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private Button CreateWoodButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            var go = CreateRect(label + " Button", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var image = go.AddComponent<Image>();
            image.sprite = MakeWoodSprite(new Color(0.33f, 0.19f, 0.07f), new Color(0.11f, 0.055f, 0.018f));
            image.type = Image.Type.Sliced;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.12f, 1.03f, 0.78f, 1f),
                pressedColor = new Color(0.85f, 0.72f, 0.45f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.45f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.08f, 0.04f, 0.01f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);
            AddShadow(go, 5f, -5f, 0.8f);

            var text = CreateText("Label", go.transform, label, 31, FontStyle.Bold,
                new Color(0.96f, 0.9f, 0.72f, 1f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;

            CreateRopeDetail(go.transform, new Vector2(-size.x * 0.5f + 25f, 0f), size.y);
            CreateRopeDetail(go.transform, new Vector2(size.x * 0.5f - 25f, 0f), size.y);
            return button;
        }

        private Button CreateSmallButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            var go = CreateRect("Utility " + label, parent, new Vector2(1f, 0f), new Vector2(1f, 0f), pos, size);
            var image = go.AddComponent<Image>();
            image.sprite = MakeWoodSprite(new Color(0.22f, 0.13f, 0.05f), new Color(0.06f, 0.03f, 0.01f));
            image.type = Image.Type.Sliced;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText("Icon", go.transform, label, 26, FontStyle.Bold,
                new Color(0.95f, 0.9f, 0.72f, 1f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.raycastTarget = false;
            AddShadow(go, 4f, -4f, 0.8f);
            return button;
        }

        private void CreateRopeDetail(Transform parent, Vector2 pos, float height)
        {
            var rope = CreateImage("Rope", parent, new Color(0.68f, 0.48f, 0.22f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(10f, height - 12f));
            rope.transform.SetAsLastSibling();
        }

        private void CreateWorldIcon(Transform parent, Vector2 pos, bool campfire = false)
        {
            var icon = CreateImage("World Icon", parent, campfire ? new Color(0.82f, 0.43f, 0.08f, 0.85f) : new Color(0.34f, 0.7f, 0.18f, 0.75f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), pos, new Vector2(115f, 115f));
            icon.raycastTarget = false;
            var outline = icon.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.95f, 0.82f, 0.3f, 0.45f);
            outline.effectDistance = new Vector2(3f, -3f);
            CreateText("IconText", parent, campfire ? "CAMP" : "WILD", 13, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(0f, 0f),
                pos + new Vector2(0f, -67f), new Vector2(120f, 24f));
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            return go;
        }

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 anchor, Vector2 sizeAnchor)
        {
            return CreateImage(name, parent, color, anchor, anchor, Vector2.zero, sizeAnchor);
        }

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = CreateRect(name, parent, anchorMin, anchorMax, pos, size);
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 dimensions)
        {
            var go = CreateRect(name, parent, anchorMin, anchorMax, pos, dimensions);
            var text = go.AddComponent<Text>();
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
            shadow.effectColor = new Color(0f, 0f, 0f, alpha);
            shadow.effectDistance = new Vector2(x, y);
            shadow.useGraphicAlpha = true;
        }

        // Backwards-compatible public hooks used by older scene button events.
        public void BtnExit() => QuitGame();

        public void BtnLevel(int num)
        {
            if (m_GameplayData != null)
                m_GameplayData.LevelNumber = num;
            BeginGameplay();
        }

        private static Sprite MakeWoodSprite(Color light, Color dark)
        {
            const int width = 128;
            const int height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float grain = Mathf.PerlinNoise(x * 0.07f, y * 0.045f);
                    float lines = Mathf.Sin((x + y * 0.35f) * 0.18f) * 0.08f;
                    float t = Mathf.Clamp01(0.35f + grain * 0.55f + lines);
                    pixels[y * width + x] = Color.Lerp(dark, light, t);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f, 0,
                SpriteMeshType.FullRect, new Vector4(14f, 14f, 14f, 14f));
        }
    }
}
