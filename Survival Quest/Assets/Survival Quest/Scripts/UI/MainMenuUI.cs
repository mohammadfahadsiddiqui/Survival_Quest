using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalGame.ScriptableObjects;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    /// <summary>Premium main menu generated at runtime and layered over the real Survival Quest world.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;

        private static MainMenuUI s_Instance;
        private Canvas m_Canvas;
        private GameObject m_MenuRoot;
        private GameObject m_SettingsRoot;
        private GameObject m_MenuButtons;
        private GameObject m_SettingsContent;
        private Text m_Status;
        private Button m_ContinueButton;
        private Slider m_MasterSlider;
        private Slider m_MusicSlider;
        private Slider m_SfxSlider;
        private Slider m_SensitivitySlider;
        private Slider m_UIScaleSlider;
        private Toggle m_InvertYToggle;
        private Toggle m_FullscreenToggle;
        private Dropdown m_QualityDropdown;
        private RawImage m_Background;
        private Texture2D m_BackgroundTexture;
        private GameObject m_HudRoot;
        private GameObject m_InputRoot;
        private bool m_GameStarted;

        private const string SaveKey = "SurvivalQuest.SaveExists";
        private const string MasterKey = "SurvivalQuest.MasterVolume";
        private const string MusicKey = "SurvivalQuest.MusicVolume";
        private const string SfxKey = "SurvivalQuest.SfxVolume";
        private const string SensitivityKey = "SurvivalQuest.Sensitivity";
        private const string InvertKey = "SurvivalQuest.InvertY";
        private const string UIScaleKey = "SurvivalQuest.UIScale";

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
            HideGameplayUI();
            BuildMenu();
            StartCoroutine(CaptureWorldBackground());
        }

        private void BuildMenu()
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = canvasObject.GetComponent<Canvas>();
            m_Canvas.transform.SetParent(transform, false);
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            m_MenuRoot = CreateRect("Menu", m_Canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Actual gameplay camera frame. This gives the menu a real image of the current Survival Quest world.
            m_Background = AddRawImage(m_MenuRoot.transform, "Game World Background", new Color(0.70f, 0.80f, 0.58f, 1f), Vector2.zero, Vector2.one);
            m_Background.raycastTarget = false;

            // Cinematic treatment while preserving the game's original low-poly identity.
            AddImage(m_MenuRoot.transform, "Cinematic Shade", new Color(0.015f, 0.025f, 0.012f, 0.30f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(m_MenuRoot.transform, "Left Shade", new Color(0.008f, 0.018f, 0.008f, 0.68f), new Vector2(0f, 0f), new Vector2(0.49f, 1f), Vector2.zero, Vector2.zero);
            AddImage(m_MenuRoot.transform, "Right Shade", new Color(0.008f, 0.012f, 0.006f, 0.48f), new Vector2(0.62f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            BuildHeader();
            BuildLeftMenu();
            BuildInfoCards();
            BuildFooter();
            BuildSettings();

            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private IEnumerator CaptureWorldBackground()
        {
            yield return new WaitForEndOfFrame();
            Camera cam = Camera.main;
            if (cam == null) yield break;

            int width = Mathf.Max(960, Screen.width);
            int height = Mathf.Max(540, Screen.height);
            RenderTexture oldTarget = cam.targetTexture;
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            rt.Create();

            cam.targetTexture = rt;
            cam.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            m_BackgroundTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            m_BackgroundTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            m_BackgroundTexture.Apply();

            RenderTexture.active = previous;
            cam.targetTexture = oldTarget;
            rt.Release();
            Destroy(rt);

            m_Background.texture = m_BackgroundTexture;
            m_Background.uvRect = new Rect(0f, 0f, 1f, 1f);
        }

        private void BuildHeader()
        {
            Text title = AddText(m_MenuRoot.transform, "Title", "SURVIVAL QUEST", 58, FontStyle.Bold,
                new Color(0.98f, 0.94f, 0.78f), TextAnchor.MiddleLeft,
                new Vector2(0.045f, 0.79f), new Vector2(0.48f, 0.91f));
            AddShadow(title.gameObject, new Vector2(4f, -4f), 0.9f);

            Text subtitle = AddText(m_MenuRoot.transform, "Subtitle", "THE WILDERNESS DOESN'T FORGIVE", 18, FontStyle.Bold,
                new Color(0.78f, 0.89f, 0.55f), TextAnchor.MiddleLeft,
                new Vector2(0.048f, 0.75f), new Vector2(0.48f, 0.80f));
            AddShadow(subtitle.gameObject, new Vector2(2f, -2f), 0.75f);

            AddImage(m_MenuRoot.transform, "Title Accent", new Color(0.72f, 0.49f, 0.18f, 0.95f),
                new Vector2(0.048f, 0.735f), new Vector2(0.34f, 0.742f), Vector2.zero, Vector2.zero);
        }

        private void BuildLeftMenu()
        {
            m_MenuButtons = CreateRect("Menu Buttons", m_MenuRoot.transform,
                new Vector2(0.045f, 0.20f), new Vector2(0.46f, 0.72f), Vector2.zero, Vector2.zero);

            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };
            for (int i = 0; i < labels.Length; i++)
            {
                float top = 0.92f - i * 0.185f;
                float bottom = top - 0.125f;
                Button button = CreateWoodButton(m_MenuButtons.transform, labels[i], new Vector2(0.02f, bottom), new Vector2(0.94f, top));

                if (i == 0 || i == 2) button.onClick.AddListener(StartNewJourney);
                else if (i == 1)
                {
                    m_ContinueButton = button;
                    button.onClick.AddListener(ContinueJourney);
                }
                else if (i == 3) button.onClick.AddListener(OpenSettings);
                else button.onClick.AddListener(QuitGame);
            }

            m_Status = AddText(m_MenuButtons.transform, "Status", "", 13, FontStyle.Bold,
                new Color(1f, 0.76f, 0.25f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0.00f), new Vector2(0.94f, 0.06f));
            RefreshContinueState();
        }

        private void BuildInfoCards()
        {
            CreateInfoCard("SurviveCard", "SURVIVE THE UNKNOWN", "Explore • Gather • Craft • Survive", 0.59f, 0.69f, 0.965f, 0.91f);
            CreateInfoCard("JourneyCard", "YOUR JOURNEY", HasSave() ? "SAVE DATA FOUND" : "NO SAVE DATA", 0.59f, 0.39f, 0.965f, 0.61f);
        }

        private void CreateInfoCard(string name, string title, string body, float xMin, float yMin, float xMax, float yMax)
        {
            GameObject card = CreateRect(name, m_MenuRoot.transform, new Vector2(xMin, yMin), new Vector2(xMax, yMax), Vector2.zero, Vector2.zero);
            Image bg = card.AddComponent<Image>();
            bg.sprite = CreateWoodSprite(new Color(0.10f, 0.13f, 0.065f), new Color(0.018f, 0.028f, 0.012f));
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 1f, 1f, 0.86f);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.38f, 0.16f, 0.90f);
            outline.effectDistance = new Vector2(2f, -2f);
            AddShadow(card, new Vector2(5f, -5f), 0.75f);

            Text heading = AddText(card.transform, "Heading", title, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.56f), new Vector2(0.92f, 0.82f));
            AddShadow(heading.gameObject, new Vector2(2f, -2f), 0.8f);

            AddText(card.transform, "Body", body, 13, FontStyle.Normal, new Color(0.84f, 0.91f, 0.70f), TextAnchor.MiddleLeft,
                new Vector2(0.055f, 0.35f), new Vector2(0.92f, 0.55f));
            AddText(card.transform, "Tag", "SURVIVAL QUEST • FIELD REPORT", 9, FontStyle.Bold,
                new Color(0.69f, 0.78f, 0.50f), TextAnchor.MiddleRight,
                new Vector2(0.34f, 0.07f), new Vector2(0.94f, 0.20f));

            RawImage image = AddRawImage(card.transform, "World Image", new Color(1f, 1f, 1f, 0.28f), new Vector2(0.59f, 0.12f), new Vector2(0.94f, 0.50f));
            image.raycastTarget = false;
            StartCoroutine(AssignCardTexture(image));
        }

        private IEnumerator AssignCardTexture(RawImage image)
        {
            yield return new WaitUntil(() => m_BackgroundTexture != null);
            image.texture = m_BackgroundTexture;
            image.uvRect = new Rect(Random.Range(0f, 0.35f), Random.Range(0.05f, 0.35f), 0.55f, 0.55f);
        }

        private void BuildFooter()
        {
            AddText(m_MenuRoot.transform, "Version", "VERSION 0.1", 11, FontStyle.Normal,
                new Color(0.80f, 0.88f, 0.64f, 0.85f), TextAnchor.MiddleLeft,
                new Vector2(0.045f, 0.025f), new Vector2(0.20f, 0.055f));
            AddText(m_MenuRoot.transform, "Copyright", "© SURVIVAL QUEST", 11, FontStyle.Normal,
                new Color(0.80f, 0.88f, 0.64f, 0.68f), TextAnchor.MiddleLeft,
                new Vector2(0.045f, 0.0f), new Vector2(0.24f, 0.03f));

            Button mute = CreateUtilityButton("♪", 0.89f, 0.035f);
            mute.onClick.AddListener(ToggleMute);
            Button fullscreen = CreateUtilityButton("□", 0.945f, 0.035f);
            fullscreen.onClick.AddListener(ToggleFullscreen);
        }

        private Button CreateUtilityButton(string label, float x, float y)
        {
            GameObject go = CreateRect("Utility", m_MenuRoot.transform, new Vector2(x, y), new Vector2(x + 0.045f, y + 0.06f), Vector2.zero, Vector2.zero);
            Image image = go.AddComponent<Image>();
            image.sprite = CreateWoodSprite(new Color(0.20f, 0.12f, 0.04f), new Color(0.055f, 0.025f, 0.008f));
            image.type = Image.Type.Sliced;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            Text icon = AddText(go.transform, "Icon", label, 18, FontStyle.Bold, new Color(0.95f, 0.88f, 0.68f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            icon.raycastTarget = false;
            return button;
        }

        private void BuildSettings()
        {
            m_SettingsRoot = CreateRect("Settings Panel", m_MenuRoot.transform,
                new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.92f), Vector2.zero, Vector2.zero);
            Image panel = m_SettingsRoot.AddComponent<Image>();
            panel.sprite = CreateWoodSprite(new Color(0.075f, 0.105f, 0.045f), new Color(0.012f, 0.020f, 0.008f));
            panel.type = Image.Type.Sliced;
            panel.color = new Color(1f, 1f, 1f, 0.98f);
            Outline outline = m_SettingsRoot.AddComponent<Outline>();
            outline.effectColor = new Color(0.62f, 0.43f, 0.18f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);
            AddShadow(m_SettingsRoot, new Vector2(8f, -8f), 0.8f);

            AddText(m_SettingsRoot.transform, "SettingsTitle", "SETTINGS", 34, FontStyle.Bold,
                new Color(0.97f, 0.91f, 0.72f), TextAnchor.MiddleLeft,
                new Vector2(0.07f, 0.88f), new Vector2(0.93f, 0.98f));
            AddText(m_SettingsRoot.transform, "SettingsSubtitle", "GAMEPLAY  •  AUDIO  •  DISPLAY  •  ACCESSIBILITY", 11, FontStyle.Bold,
                new Color(0.67f, 0.80f, 0.48f), TextAnchor.MiddleLeft,
                new Vector2(0.075f, 0.82f), new Vector2(0.93f, 0.88f));

            m_SettingsContent = CreateRect("Settings Content", m_SettingsRoot.transform,
                new Vector2(0.07f, 0.14f), new Vector2(0.93f, 0.80f), Vector2.zero, Vector2.zero);

            BuildSettingsSection("AUDIO", 0.67f, CreateAudioSettings);
            BuildSettingsSection("GAMEPLAY", 0.39f, CreateGameplaySettings);
            BuildSettingsSection("DISPLAY", 0.17f, CreateDisplaySettings);

            Button back = CreateWoodButton(m_SettingsRoot.transform, "BACK", new Vector2(0.70f, 0.025f), new Vector2(0.94f, 0.105f));
            back.onClick.AddListener(CloseSettings);
            m_SettingsRoot.SetActive(false);
        }

        private void BuildSettingsSection(string title, float y, System.Action<Transform> builder)
        {
            AddText(m_SettingsContent.transform, title, title, 13, FontStyle.Bold,
                new Color(0.74f, 0.88f, 0.54f), TextAnchor.MiddleLeft,
                new Vector2(0f, y), new Vector2(0.20f, y + 0.075f));
            AddImage(m_SettingsContent.transform, title + " Line", new Color(0.50f, 0.34f, 0.14f, 0.75f),
                new Vector2(0.21f, y + 0.034f), new Vector2(1f, y + 0.038f), Vector2.zero, Vector2.zero);
            builder(m_SettingsContent.transform);
        }

        private void CreateAudioSettings(Transform parent)
        {
            m_MasterSlider = CreateSliderSetting(parent, "Master Volume", 0.69f, PlayerPrefs.GetFloat(MasterKey, 1f), SetMaster);
            m_MusicSlider = CreateSliderSetting(parent, "Music Volume", 0.58f, PlayerPrefs.GetFloat(MusicKey, 0.8f), SetMusic);
            m_SfxSlider = CreateSliderSetting(parent, "SFX Volume", 0.47f, PlayerPrefs.GetFloat(SfxKey, 0.9f), SetSfx);
        }

        private void CreateGameplaySettings(Transform parent)
        {
            m_SensitivitySlider = CreateSliderSetting(parent, "Camera Sensitivity", 0.41f, PlayerPrefs.GetFloat(SensitivityKey, 0.5f), SetSensitivity);
            m_InvertYToggle = CreateToggleSetting(parent, "Invert Y Axis", 0.30f, PlayerPrefs.GetInt(InvertKey, 0) == 1, SetInvertY);
        }

        private void CreateDisplaySettings(Transform parent)
        {
            m_FullscreenToggle = CreateToggleSetting(parent, "Fullscreen", 0.20f, Screen.fullScreen, SetFullscreen);
            m_QualityDropdown = CreateDropdownSetting(parent, "Graphics Quality", 0.10f);
            m_UIScaleSlider = CreateSliderSetting(parent, "Interface Scale", 0.00f, PlayerPrefs.GetFloat(UIScaleKey, 1f), SetUIScale);
        }

        private Slider CreateSliderSetting(Transform parent, string label, float y, float value, UnityEngine.Events.UnityAction<float> callback)
        {
            AddText(parent, label, label, 11, FontStyle.Bold, new Color(0.90f, 0.91f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.27f, y), new Vector2(0.52f, y + 0.075f));

            GameObject root = CreateRect(label + " Slider", parent, new Vector2(0.54f, y + 0.018f), new Vector2(0.96f, y + 0.073f), Vector2.zero, Vector2.zero);
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(callback);

            Image track = CreateRect("Track", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).AddComponent<Image>();
            track.color = new Color(0.06f, 0.08f, 0.035f);
            track.raycastTarget = false;

            GameObject fill = CreateRect("Fill", root.transform, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.68f, 0.49f, 0.17f);
            fillImage.raycastTarget = false;

            GameObject handle = CreateRect("Handle", root.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(18f, 24f));
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.94f, 0.80f, 0.42f);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            return slider;
        }

        private Toggle CreateToggleSetting(Transform parent, string label, float y, bool value, UnityEngine.Events.UnityAction<bool> callback)
        {
            AddText(parent, label, label, 11, FontStyle.Bold, new Color(0.90f, 0.91f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.27f, y), new Vector2(0.63f, y + 0.075f));
            GameObject root = CreateRect(label + " Toggle", parent, new Vector2(0.84f, y + 0.01f), new Vector2(0.96f, y + 0.08f), Vector2.zero, Vector2.zero);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.11f, 0.05f);
            Toggle toggle = root.AddComponent<Toggle>();
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(callback);

            GameObject check = CreateRect("Check", root.transform, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), Vector2.zero, Vector2.zero);
            Image checkImage = check.AddComponent<Image>();
            checkImage.color = new Color(0.76f, 0.60f, 0.22f);
            toggle.graphic = checkImage;
            return toggle;
        }

        private Dropdown CreateDropdownSetting(Transform parent, string label, float y)
        {
            AddText(parent, label, label, 11, FontStyle.Bold, new Color(0.90f, 0.91f, 0.80f), TextAnchor.MiddleLeft,
                new Vector2(0.27f, y), new Vector2(0.63f, y + 0.075f));
            GameObject root = CreateRect(label + " Dropdown", parent, new Vector2(0.72f, y + 0.005f), new Vector2(0.96f, y + 0.08f), Vector2.zero, Vector2.zero);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.035f);
            Dropdown dropdown = root.AddComponent<Dropdown>();
            dropdown.options.Clear();
            dropdown.options.Add(new Dropdown.OptionData("Low"));
            dropdown.options.Add(new Dropdown.OptionData("Medium"));
            dropdown.options.Add(new Dropdown.OptionData("High"));
            dropdown.options.Add(new Dropdown.OptionData("Ultra"));
            dropdown.value = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, 3);
            dropdown.onValueChanged.AddListener(QualitySettings.SetQualityLevel);
            return dropdown;
        }

        private void OpenSettings()
        {
            m_SettingsRoot.SetActive(true);
            m_MenuButtons.SetActive(false);
        }

        private void CloseSettings()
        {
            m_SettingsRoot.SetActive(false);
            m_MenuButtons.SetActive(true);
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
                m_Status.text = "NO SAVE DATA — START A NEW JOURNEY";
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
            RestoreGameplayUI();
            if (m_Canvas != null) Destroy(m_Canvas.gameObject);
            if (m_BackgroundTexture != null) Destroy(m_BackgroundTexture);
            Destroy(gameObject);
        }

        private void SetMaster(float value) { AudioListener.volume = value; PlayerPrefs.SetFloat(MasterKey, value); PlayerPrefs.Save(); }
        private void SetMusic(float value) { PlayerPrefs.SetFloat(MusicKey, value); PlayerPrefs.Save(); }
        private void SetSfx(float value) { PlayerPrefs.SetFloat(SfxKey, value); PlayerPrefs.Save(); }
        private void SetSensitivity(float value) { PlayerPrefs.SetFloat(SensitivityKey, value); PlayerPrefs.Save(); }
        private void SetInvertY(bool value) { PlayerPrefs.SetInt(InvertKey, value ? 1 : 0); PlayerPrefs.Save(); }
        private void SetUIScale(float value) { PlayerPrefs.SetFloat(UIScaleKey, value); PlayerPrefs.Save(); }
        private void SetFullscreen(bool value) { Screen.fullScreen = value; }

        private void ToggleMute()
        {
            AudioListener.volume = AudioListener.volume > 0.01f ? 0f : PlayerPrefs.GetFloat(MasterKey, 1f);
        }

        private void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            if (m_FullscreenToggle != null) m_FullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        }

        private void RefreshContinueState()
        {
            if (m_ContinueButton != null) m_ContinueButton.interactable = HasSave();
        }

        private bool HasSave() => PlayerPrefs.GetInt(SaveKey, 0) == 1;

        private void HideGameplayUI()
        {
            m_HudRoot = GameObject.Find("ui-base");
            if (m_HudRoot != null) m_HudRoot.SetActive(false);
            m_InputRoot = GameObject.Find("InputControl");
            if (m_InputRoot != null) m_InputRoot.SetActive(false);
        }

        private void RestoreGameplayUI()
        {
            if (m_HudRoot != null) m_HudRoot.SetActive(true);
            if (m_InputRoot != null) m_InputRoot.SetActive(true);
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

        private static Button CreateWoodButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateRect(label + " Button", parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = root.AddComponent<Image>();
            image.sprite = CreateWoodSprite(new Color(0.34f, 0.19f, 0.065f), new Color(0.09f, 0.04f, 0.01f));
            image.type = Image.Type.Sliced;

            Button button = root.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.90f, 0.90f, 0.90f, 0.92f);
            colors.highlightedColor = new Color(1f, 0.94f, 0.68f, 1f);
            colors.pressedColor = new Color(0.80f, 0.66f, 0.36f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.40f);
            button.colors = colors;

            Text text = AddText(root.transform, "Label", label, 25, FontStyle.Bold,
                new Color(0.97f, 0.91f, 0.73f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
            text.raycastTarget = false;
            AddShadow(root, new Vector2(4f, -4f), 0.8f);
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

        private static RawImage AddRawImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            RawImage image = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero).AddComponent<RawImage>();
            image.color = color;
            image.uvRect = new Rect(0f, 0f, 1f, 1f);
            return image;
        }

        private static Text AddText(Transform parent, string name, string content, int fontSize, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            Text text = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero).AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(8, fontSize - 6);
            text.resizeTextMaxSize = fontSize;
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
                    pixels[y * width + x] = Color.Lerp(dark, light, Mathf.Clamp01(0.35f + grain * 0.55f + lines));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f, 0,
                SpriteMeshType.FullRect, new Vector4(14f, 14f, 14f, 14f));
        }

        public void BtnExit() => QuitGame();

        public void BtnLevel(int num)
        {
            if (m_GameplayData != null) m_GameplayData.LevelNumber = num;
            BeginGameplay();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
