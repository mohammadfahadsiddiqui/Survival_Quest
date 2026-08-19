using System;
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
    public sealed class ExactReferenceMainMenu : MonoBehaviour
    {
        private const string GAME_SCENE = "SampleScene";
        private const string SAVE = "SurvivalQuest.SaveExists";
        private const string MASTER = "SurvivalQuest.MasterVolume";
        private const string MUSIC = "SurvivalQuest.MusicVolume";
        private const string SFX = "SurvivalQuest.SfxVolume";
        private const string SENS = "SurvivalQuest.Sensitivity";
        private const string INVERT = "SurvivalQuest.InvertY";
        private const string ART_B64 = "";
        private Canvas canvas;
        private GameObject settings;
        private bool loading;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] private static void Boot(){if(SceneManager.GetActiveScene().name!="MainMenu")return;if(FindFirstObjectByType<ExactReferenceMainMenu>()!=null)return;var go=new GameObject("SURVIVAL QUEST - EXACT REFERENCE MENU");go.AddComponent<ExactReferenceMainMenu>();}
        private void Awake(){if(SceneManager.GetActiveScene().name!="MainMenu"){Destroy(gameObject);return;}foreach(var old in FindObjectsByType<MainMenuUI>(FindObjectsSortMode.None))old.enabled=false;foreach(var es in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))Destroy(es.gameObject);var esgo=new GameObject("EventSystem",typeof(EventSystem));#if ENABLE_INPUT_SYSTEM
            esgo.AddComponent<InputSystemUIInputModule>();
#else
            esgo.AddComponent<StandaloneInputModule>();
#endif
            Time.timeScale=1f;Cursor.visible=true;Cursor.lockState=CursorLockMode.None;}
        private void Start()=>Build();
        private void Build(){canvas=new GameObject("EXACT REFERENCE MAIN MENU",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10000;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;var root=Rect("REFERENCE ARTWORK",canvas.transform,Vector2.zero,Vector2.one);var image=root.AddComponent<RawImage>();image.texture=DecodeArtwork();image.raycastTarget=false;Hit(root.transform,"PLAY",.045f,.524f,.325f,.614f,NewGame);Hit(root.transform,"CONTINUE",.045f,.434f,.325f,.514f,ContinueGame);Hit(root.transform,"NEW GAME",.045f,.344f,.325f,.424f,NewGame);Hit(root.transform,"SETTINGS",.045f,.254f,.325f,.334f,OpenSettings);Hit(root.transform,"EXIT",.045f,.164f,.325f,.244f,QuitGame);Hit(root.transform,"AUDIO",.824f,.035f,.889f,.115f,ToggleMute);Hit(root.transform,"FULLSCREEN",.895f,.035f,.965f,.115f,ToggleFullscreen);}
        private Texture2D DecodeArtwork(){var tex=new Texture2D(2,2,TextureFormat.RGB24,false,true);tex.LoadImage(Convert.FromBase64String(ART_B64),false);tex.wrapMode=TextureWrapMode.Clamp;tex.filterMode=FilterMode.Bilinear;tex.name="Survival Quest Exact Main Menu";return tex;}
        private void Hit(Transform parent,string name,float x1,float y1,float x2,float y2,UnityEngine.Events.UnityAction action){var go=Rect(name,parent,new Vector2(x1,y1),new Vector2(x2,y2));var img=go.AddComponent<Image>();img.color=new Color(0,0,0,0);var b=go.AddComponent<Button>();b.targetGraphic=img;b.onClick.AddListener(action);}
        private void OpenSettings(){if(settings!=null){settings.SetActive(true);return;}settings=Rect("SETTINGS PANEL",canvas.transform,new Vector2(.18f,.12f),new Vector2(.82f,.88f));var bg=settings.AddComponent<Image>();bg.color=new Color(.025f,.045f,.012f,.985f);var outline=settings.AddComponent<Outline>();outline.effectColor=new Color(.72f,.50f,.20f,.95f);outline.effectDistance=new Vector2(3,-3);Label(settings.transform,"SETTINGS",34,.06f,.86f,.94f,.95f);Label(settings.transform,"AUDIO",12,.06f,.77f,.35f,.82f);SliderRow(settings.transform,"Master Volume",.68f,PlayerPrefs.GetFloat(MASTER,1f),v=>SaveFloat(MASTER,v,true));SliderRow(settings.transform,"Music Volume",.59f,PlayerPrefs.GetFloat(MUSIC,.8f),v=>SaveFloat(MUSIC,v,false));SliderRow(settings.transform,"SFX Volume",.50f,PlayerPrefs.GetFloat(SFX,.9f),v=>SaveFloat(SFX,v,false));Label(settings.transform,"GAMEPLAY",12,.06f,.42f,.35f,.47f);SliderRow(settings.transform,"Camera Sensitivity",.34f,PlayerPrefs.GetFloat(SENS,.5f),v=>SaveFloat(SENS,v,false));ToggleRow(settings.transform,"Invert Y Axis",.25f,PlayerPrefs.GetInt(INVERT,0)==1,v=>{PlayerPrefs.SetInt(INVERT,v?1:0);PlayerPrefs.Save();});Label(settings.transform,"DISPLAY",12,.06f,.16f,.35f,.21f);ToggleRow(settings.transform,"Fullscreen",.08f,Screen.fullScreen,v=>Screen.fullScreen=v);Hit(settings.transform,"BACK",.70f,.015f,.94f,.095f,()=>settings.SetActive(false));Label(settings.transform,"BACK",22,.70f,.015f,.94f,.095f);}
        private void SliderRow(Transform p,string name,float y,float value,UnityEngine.Events.UnityAction<float> action){Label(p,name,14,.08f,y,.40f,y+.055f);var h=Rect(name+" Slider",p,new Vector2(.43f,y+.006f),new Vector2(.90f,y+.06f));var s=h.AddComponent<Slider>();s.minValue=0;s.maxValue=1;s.value=value;s.onValueChanged.AddListener(action);var track=Rect("Track",h.transform,Vector2.zero,Vector2.one).AddComponent<Image>();track.color=new Color(.06f,.04f,.015f);var fill=Rect("Fill",h.transform,Vector2.zero,new Vector2(value,1)).AddComponent<Image>();fill.color=new Color(.73f,.52f,.18f);var handle=Rect("Handle",h.transform,new Vector2(value,.5f),new Vector2(value,.5f));handle.GetComponent<RectTransform>().sizeDelta=new Vector2(25,32);handle.AddComponent<Image>().color=new Color(.98f,.82f,.43f);s.fillRect=fill.GetComponent<RectTransform>();s.handleRect=handle.GetComponent<RectTransform>();}
        private void ToggleRow(Transform p,string name,float y,bool value,UnityEngine.Events.UnityAction<bool> action){Label(p,name,14,.08f,y,.55f,y+.055f);var go=Rect(name+" Toggle",p,new Vector2(.80f,y),new Vector2(.90f,y+.06f));var bg=go.AddComponent<Image>();bg.color=new Color(.08f,.055f,.018f);var t=go.AddComponent<Toggle>();t.targetGraphic=bg;t.isOn=value;t.onValueChanged.AddListener(action);var check=Rect("Check",go.transform,new Vector2(.08f,.15f),new Vector2(.45f,.85f));check.AddComponent<Image>().color=new Color(.74f,.54f,.18f);}
        private Text Label(Transform p,string text,int size,float x1,float y1,float x2,float y2){var t=Rect(text+" Label",p,new Vector2(x1,y1),new Vector2(x2,y2)).AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=size;t.fontStyle=FontStyle.Bold;t.alignment=TextAnchor.MiddleLeft;t.color=new Color(.95f,.91f,.79f);t.raycastTarget=false;return t;}
        private void SaveFloat(string key,float value,bool master){PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();if(master)AudioListener.volume=value;}private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0f:PlayerPrefs.GetFloat(MASTER,1f);}private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;}private void NewGame(){PlayerPrefs.SetInt(SAVE,1);PlayerPrefs.Save();LoadGame();}private void ContinueGame(){LoadGame();}private void LoadGame(){if(loading)return;loading=true;Time.timeScale=1f;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GAME_SCENE);}private void QuitGame(){#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }private static GameObject Rect(string name,Transform parent,Vector2 min,Vector2 max){var g=new GameObject(name,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;return g;}
    }
}