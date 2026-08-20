using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    [DefaultExecutionOrder(-1000)]
    public class ReferenceMainMenuUI : MonoBehaviour
    {
        private const string GameScene = "SampleScene";
        private Canvas canvas;
        private GameObject settingsPanel;
        private bool loading;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            EnsureSingleEventSystem();
            foreach (MainMenuUI oldMenu in FindObjectsByType<MainMenuUI>(FindObjectsSortMode.None))
                if (oldMenu != null) oldMenu.enabled = false;
        }

        private void Start() => Build();

        private void Build()
        {
            if (canvas != null) return;

            canvas = new GameObject("SURVIVAL QUEST - EXACT ARTWORK MENU", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1659f, 948f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = Rect("Exact Main Menu Artwork", canvas.transform, Vector2.zero, Vector2.one);
            RawImage artwork = root.AddComponent<RawImage>();
            artwork.texture = Resources.Load<Texture2D>("MainMenuArtwork");
            artwork.color = Color.white;
            artwork.raycastTarget = false;

            if (artwork.texture == null)
            {
                Debug.LogError("SURVIVAL QUEST: MainMenuArtwork.png is missing. Put the supplied asset at Assets/Resources/MainMenuArtwork.png");
                return;
            }

            Hit(root.transform, "PLAY",       .045f, .525f, .325f, .615f, StartNew);
            Hit(root.transform, "CONTINUE",   .045f, .435f, .325f, .515f, ContinueGame);
            Hit(root.transform, "NEW GAME",   .045f, .345f, .325f, .425f, StartNew);
            Hit(root.transform, "SETTINGS",   .045f, .255f, .325f, .335f, OpenSettings);
            Hit(root.transform, "EXIT",       .045f, .165f, .325f, .245f, QuitGame);
            Hit(root.transform, "AUDIO",      .804f, .045f, .859f, .140f, ToggleMute);
            Hit(root.transform, "FULLSCREEN", .868f, .045f, .925f, .140f, ToggleFullscreen);
        }

        private void OpenSettings()
        {
            if (settingsPanel != null) { settingsPanel.SetActive(true); return; }
            settingsPanel = Rect("Settings Panel", canvas.transform, new Vector2(.17f,.08f), new Vector2(.83f,.92f));
            Image bg = settingsPanel.AddComponent<Image>();
            bg.color = new Color(.018f,.035f,.008f,.985f);
            Outline outline = settingsPanel.AddComponent<Outline>();
            outline.effectColor = new Color(.72f,.50f,.20f,.95f);
            outline.effectDistance = new Vector2(3,-3);

            Label(settingsPanel.transform,"SETTINGS",36,.06f,.88f,.94f,.96f,TextAnchor.MiddleLeft);
            Label(settingsPanel.transform,"GAMEPLAY  •  AUDIO  •  DISPLAY",12,.065f,.82f,.94f,.87f,TextAnchor.MiddleLeft,new Color(.68f,.80f,.48f));
            Section("AUDIO",.75f);
            SliderRow("Master Volume",.67f,PlayerPrefs.GetFloat("SurvivalQuest.MasterVolume",1f),v=>SetVolume("SurvivalQuest.MasterVolume",v,true));
            SliderRow("Music Volume",.57f,PlayerPrefs.GetFloat("SurvivalQuest.MusicVolume",.8f),v=>Save("SurvivalQuest.MusicVolume",v));
            SliderRow("SFX Volume",.47f,PlayerPrefs.GetFloat("SurvivalQuest.SfxVolume",.9f),v=>Save("SurvivalQuest.SfxVolume",v));
            Section("GAMEPLAY",.39f);
            SliderRow("Camera Sensitivity",.31f,PlayerPrefs.GetFloat("SurvivalQuest.Sensitivity",.5f),v=>Save("SurvivalQuest.Sensitivity",v));
            ToggleRow("Invert Y Axis",.22f,PlayerPrefs.GetInt("SurvivalQuest.InvertY",0)==1,v=>{PlayerPrefs.SetInt("SurvivalQuest.InvertY",v?1:0);PlayerPrefs.Save();});
            Section("DISPLAY",.14f);
            ToggleRow("Fullscreen",.07f,Screen.fullScreen,v=>Screen.fullScreen=v);
            Hit(settingsPanel.transform,"BACK",.70f,.01f,.94f,.095f,CloseSettings);
            Label(settingsPanel.transform,"BACK",25,.70f,.01f,.94f,.095f,TextAnchor.MiddleCenter);
        }

        private void Section(string text,float y)
        {
            Label(settingsPanel.transform,text,12,.07f,y,.35f,y+.045f,TextAnchor.MiddleLeft,new Color(.70f,.83f,.46f));
            Image line = Rect(text+" Line",settingsPanel.transform,new Vector2(.07f,y-.012f),new Vector2(.93f,y-.008f)).AddComponent<Image>();
            line.color = new Color(.55f,.37f,.12f,.8f);
        }

        private void SliderRow(string text,float y,float value,UnityEngine.Events.UnityAction<float> callback)
        {
            Label(settingsPanel.transform,text,14,.08f,y,.40f,y+.06f,TextAnchor.MiddleLeft);
            GameObject holder = Rect(text+" Slider",settingsPanel.transform,new Vector2(.43f,y+.008f),new Vector2(.90f,y+.058f));
            Slider slider = holder.AddComponent<Slider>();
            slider.minValue=0; slider.maxValue=1; slider.value=Mathf.Clamp01(value); slider.interactable=true;
            slider.onValueChanged.AddListener(callback);
            Image track=Rect("Track",holder.transform,Vector2.zero,Vector2.one).AddComponent<Image>(); track.color=new Color(.07f,.045f,.015f); track.raycastTarget=true;
            GameObject fill=Rect("Fill",holder.transform,Vector2.zero,new Vector2(.5f,1)); Image fi=fill.AddComponent<Image>(); fi.color=new Color(.73f,.52f,.18f); fi.raycastTarget=false;
            GameObject handle=Rect("Handle",holder.transform,new Vector2(0,.5f),new Vector2(0,.5f)); handle.GetComponent<RectTransform>().sizeDelta=new Vector2(28,34); Image hi=handle.AddComponent<Image>(); hi.color=new Color(.98f,.82f,.43f); hi.raycastTarget=true;
            slider.fillRect=fill.GetComponent<RectTransform>(); slider.handleRect=handle.GetComponent<RectTransform>();
        }

        private void ToggleRow(string text,float y,bool value,UnityEngine.Events.UnityAction<bool> callback)
        {
            Label(settingsPanel.transform,text,14,.08f,y,.55f,y+.06f,TextAnchor.MiddleLeft);
            GameObject holder=Rect(text+" Toggle",settingsPanel.transform,new Vector2(.78f,y),new Vector2(.91f,y+.065f));
            Image bg=holder.AddComponent<Image>(); bg.color=new Color(.08f,.055f,.018f); bg.raycastTarget=true;
            Toggle toggle=holder.AddComponent<Toggle>(); toggle.targetGraphic=bg; toggle.isOn=value; toggle.interactable=true; toggle.onValueChanged.AddListener(callback);
            GameObject check=Rect("Check",holder.transform,new Vector2(.08f,.15f),new Vector2(.46f,.85f)); Image ci=check.AddComponent<Image>(); ci.color=new Color(.75f,.54f,.18f); ci.raycastTarget=false; toggle.graphic=ci;
        }

        private Text Label(Transform parent,string text,int size,float x1,float y1,float x2,float y2,TextAnchor align,Color? color=null)
        {
            Text t=Rect(text+" Label",parent,new Vector2(x1,y1),new Vector2(x2,y2)).AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=text; t.fontSize=size; t.fontStyle=FontStyle.Bold; t.alignment=align; t.color=color??new Color(.95f,.91f,.79f); t.raycastTarget=false; return t;
        }

        private void Hit(Transform parent,string name,float x1,float y1,float x2,float y2,UnityEngine.Events.UnityAction action)
        {
            GameObject go=Rect(name+" Click",parent,new Vector2(x1,y1),new Vector2(x2,y2));
            Image image=go.AddComponent<Image>();
            // Completely invisible hit area: it must never create a colored rectangle over the artwork.
            image.color=Color.clear;
            image.raycastTarget=true;

            Button button=go.AddComponent<Button>();
            button.targetGraphic=image;
            button.transition=Selectable.Transition.None;
            button.onClick.AddListener(action);

            MainMenuHoverGlow glow=go.AddComponent<MainMenuHoverGlow>();
            glow.Configure(image);
        }

        private void StartNew(){LoadGame();}
        private void ContinueGame(){LoadGame();}
        private void LoadGame(){if(loading)return; loading=true; Time.timeScale=1f; Cursor.visible=false; Cursor.lockState=CursorLockMode.Locked; SceneManager.LoadScene(GameScene);}
        private void CloseSettings(){settingsPanel.SetActive(false);}
        private void Save(string key,float value){PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();}
        private void SetVolume(string key,float value,bool master){Save(key,value);if(master)AudioListener.volume=value;}
        private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0f:PlayerPrefs.GetFloat("SurvivalQuest.MasterVolume",1f);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;}
        private void QuitGame(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }

        private static GameObject Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            GameObject go=new GameObject(name,typeof(RectTransform)); RectTransform rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; return go;
        }

        private static void EnsureSingleEventSystem()
        {
            EventSystem[] systems=FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if(systems.Length>0){for(int i=1;i<systems.Length;i++)Destroy(systems[i].gameObject);return;}
            GameObject go=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    internal sealed class MainMenuHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool hovering;
        private Outline tightGlow;
        private Outline middleGlow;
        private Outline outerGlow;

        public void Configure(Image target)
        {
            // Three outline layers create a glow around the option only.
            // The source Image stays fully transparent, so there is NO orange rectangle.
            tightGlow=target.gameObject.AddComponent<Outline>();
            tightGlow.useGraphicAlpha=false;
            tightGlow.effectColor=new Color(1f,.70f,.20f,.95f);
            tightGlow.effectDistance=new Vector2(2f,2f);
            tightGlow.enabled=false;

            middleGlow=target.gameObject.AddComponent<Outline>();
            middleGlow.useGraphicAlpha=false;
            middleGlow.effectColor=new Color(1f,.52f,.08f,.42f);
            middleGlow.effectDistance=new Vector2(5f,5f);
            middleGlow.enabled=false;

            outerGlow=target.gameObject.AddComponent<Outline>();
            outerGlow.useGraphicAlpha=false;
            outerGlow.effectColor=new Color(1f,.38f,.03f,.18f);
            outerGlow.effectDistance=new Vector2(9f,9f);
            outerGlow.enabled=false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering=true;
            SetGlow(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering=false;
            SetGlow(false);
        }

        private void SetGlow(bool enabled)
        {
            if(tightGlow!=null) tightGlow.enabled=enabled;
            if(middleGlow!=null) middleGlow.enabled=enabled;
            if(outerGlow!=null) outerGlow.enabled=enabled;
        }

        private void Update()
        {
            if(!hovering) return;

            float pulse=0.82f+Mathf.Sin(Time.unscaledTime*4.5f)*0.18f;
            if(tightGlow!=null) tightGlow.effectColor=new Color(1f,.72f,.25f,.95f*pulse);
            if(middleGlow!=null) middleGlow.effectColor=new Color(1f,.52f,.08f,.42f*pulse);
            if(outerGlow!=null) outerGlow.effectColor=new Color(1f,.35f,.02f,.18f*pulse);
        }
    }
}
