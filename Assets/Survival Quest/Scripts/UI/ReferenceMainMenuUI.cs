using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ReferenceMainMenuUI : MonoBehaviour
    {
        private const string GAME_SCENE = "SampleScene";
        private const string ART_NAME = "MainMenuArtwork";
        private Canvas canvas;
        private GameObject settingsPanel;

        private void Awake()
        {
            foreach (var old in FindObjectsByType<MainMenuUI>(FindObjectsSortMode.None))
                if (old != null && old != this) old.enabled = false;
            EnsureEventSystem();
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start() => Build();

        private void Build()
        {
            if (canvas != null) return;
            canvas = new GameObject("REFERENCE MAIN MENU", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1659f, 948f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var root = MakeRect("REFERENCE ARTWORK", canvas.transform, Vector2.zero, Vector2.one);
            var artwork = root.AddComponent<RawImage>();
            artwork.texture = Resources.Load<Texture2D>(ART_NAME);
            artwork.color = Color.white;
            artwork.uvRect = new Rect(0f, 0f, 1f, 1f);
            artwork.raycastTarget = false;

            if (artwork.texture == null)
            {
                Debug.LogError("REFERENCE MENU: MainMenuArtwork.png is missing. Put it at Assets/Resources/MainMenuArtwork.png");
                return;
            }

            AddHit(root.transform, "PLAY", new Vector2(.050f,.385f), new Vector2(.325f,.475f), LoadGame);
            AddHit(root.transform, "CONTINUE", new Vector2(.050f,.295f), new Vector2(.325f,.375f), ContinueGame);
            AddHit(root.transform, "NEW GAME", new Vector2(.050f,.205f), new Vector2(.325f,.285f), NewGame);
            AddHit(root.transform, "SETTINGS", new Vector2(.050f,.115f), new Vector2(.325f,.195f), OpenSettings);
            AddHit(root.transform, "EXIT", new Vector2(.050f,.025f), new Vector2(.325f,.105f), QuitGame);
            AddHit(root.transform, "AUDIO", new Vector2(.825f,.035f), new Vector2(.905f,.115f), ToggleMute);
            AddHit(root.transform, "FULLSCREEN", new Vector2(.910f,.035f), new Vector2(.985f,.115f), ToggleFullscreen);
        }

        private void AddHit(Transform parent, string name, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var go = MakeRect(name + " Hit Area", parent, min, max);
            var image = go.AddComponent<Image>();
            image.color = new Color(0f,0f,0f,0f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            var colors = button.colors;
            colors.highlightedColor = new Color(1f,1f,1f,0.025f);
            colors.pressedColor = new Color(1f,.75f,.2f,.05f);
            colors.fadeDuration = .06f;
            button.colors = colors;
        }

        private void OpenSettings()
        {
            if (settingsPanel != null) { settingsPanel.SetActive(true); return; }
            settingsPanel = MakeRect("SETTINGS OVERLAY", canvas.transform, new Vector2(.18f,.10f), new Vector2(.82f,.90f));
            var bg = settingsPanel.AddComponent<Image>(); bg.color = new Color(.025f,.045f,.012f,.97f);
            var outline = settingsPanel.AddComponent<Outline>(); outline.effectColor = new Color(.72f,.48f,.16f,.95f); outline.effectDistance = new Vector2(3,-3);
            Label(settingsPanel.transform,"SETTINGS",34,new Vector2(.07f,.86f),new Vector2(.93f,.95f));
            AddSlider(settingsPanel.transform,"Master Volume",.67f,PlayerPrefs.GetFloat("SurvivalQuest.MasterVolume",1f),v=>{PlayerPrefs.SetFloat("SurvivalQuest.MasterVolume",v);AudioListener.volume=v;PlayerPrefs.Save();});
            AddSlider(settingsPanel.transform,"Music Volume",.55f,PlayerPrefs.GetFloat("SurvivalQuest.MusicVolume",.8f),v=>{PlayerPrefs.SetFloat("SurvivalQuest.MusicVolume",v);PlayerPrefs.Save();});
            AddSlider(settingsPanel.transform,"SFX Volume",.43f,PlayerPrefs.GetFloat("SurvivalQuest.SfxVolume",.9f),v=>{PlayerPrefs.SetFloat("SurvivalQuest.SfxVolume",v);PlayerPrefs.Save();});
            AddSlider(settingsPanel.transform,"Camera Sensitivity",.31f,PlayerPrefs.GetFloat("SurvivalQuest.Sensitivity",.5f),v=>{PlayerPrefs.SetFloat("SurvivalQuest.Sensitivity",v);PlayerPrefs.Save();});
            AddToggle(settingsPanel.transform,"Invert Y Axis",.20f,PlayerPrefs.GetInt("SurvivalQuest.InvertY",0)==1,v=>{PlayerPrefs.SetInt("SurvivalQuest.InvertY",v?1:0);PlayerPrefs.Save();});
            AddToggle(settingsPanel.transform,"Fullscreen",.12f,Screen.fullScreen,v=>Screen.fullScreen=v);
            AddHit(settingsPanel.transform,"BACK",new Vector2(.68f,.025f),new Vector2(.94f,.105f),()=>settingsPanel.SetActive(false));
            Label(settingsPanel.transform,"BACK",24,new Vector2(.68f,.025f),new Vector2(.94f,.105f));
        }

        private void AddSlider(Transform parent,string name,float y,float value,UnityEngine.Events.UnityAction<float> callback)
        {
            Label(parent,name,14,new Vector2(.08f,y),new Vector2(.40f,y+.06f));
            var holder=MakeRect(name+" Slider",parent,new Vector2(.43f,y+.012f),new Vector2(.90f,y+.058f));
            var slider=holder.AddComponent<Slider>(); slider.minValue=0; slider.maxValue=1; slider.value=Mathf.Clamp01(value); slider.onValueChanged.AddListener(callback);
            var track=MakeRect("Track",holder.transform,Vector2.zero,Vector2.one).AddComponent<Image>(); track.color=new Color(.07f,.045f,.018f);
            var fill=MakeRect("Fill",holder.transform,Vector2.zero,new Vector2(Mathf.Clamp01(value),1f)).AddComponent<Image>(); fill.color=new Color(.74f,.52f,.18f); fill.raycastTarget=false;
            var handle=MakeRect("Handle",holder.transform,new Vector2(Mathf.Clamp01(value),.5f),new Vector2(Mathf.Clamp01(value),.5f)); handle.GetComponent<RectTransform>().sizeDelta=new Vector2(26,32); handle.AddComponent<Image>().color=new Color(.98f,.82f,.43f);
            slider.fillRect=fill.GetComponent<RectTransform>(); slider.handleRect=handle.GetComponent<RectTransform>();
        }

        private void AddToggle(Transform parent,string name,float y,bool value,UnityEngine.Events.UnityAction<bool> callback)
        {
            Label(parent,name,14,new Vector2(.08f,y),new Vector2(.55f,y+.06f));
            var holder=MakeRect(name+" Toggle",parent,new Vector2(.78f,y),new Vector2(.90f,y+.065f));
            var bg=holder.AddComponent<Image>(); bg.color=new Color(.08f,.055f,.02f);
            var toggle=holder.AddComponent<Toggle>(); toggle.targetGraphic=bg; toggle.isOn=value; toggle.onValueChanged.AddListener(callback);
            var check=MakeRect("Check",holder.transform,new Vector2(.08f,.15f),new Vector2(.44f,.85f)).AddComponent<Image>(); check.color=new Color(.75f,.54f,.17f); check.raycastTarget=false; toggle.graphic=check;
        }

        private Text Label(Transform parent,string text,int size,Vector2 min,Vector2 max)
        {
            var t=MakeRect(text,parent,min,max).AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=text; t.fontSize=size; t.fontStyle=FontStyle.Bold; t.color=new Color(.94f,.91f,.80f); t.alignment=TextAnchor.MiddleLeft; t.raycastTarget=false; return t;
        }

        private static GameObject MakeRect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero; return go;
        }

        private void NewGame(){PlayerPrefs.SetInt("SurvivalQuest.SaveExists",1);PlayerPrefs.Save();LoadGame();}
        private void ContinueGame(){LoadGame();}
        private void LoadGame(){Time.timeScale=1f;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GAME_SCENE);}
        private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0f:PlayerPrefs.GetFloat("SurvivalQuest.MasterVolume",1f);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;}
        private void QuitGame(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private static void EnsureEventSystem()
        {
            var systems=FindObjectsByType<EventSystem>(FindObjectsSortMode.None); if(systems.Length>0){for(int i=1;i<systems.Length;i++)Destroy(systems[i].gameObject);return;}
            var go=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}