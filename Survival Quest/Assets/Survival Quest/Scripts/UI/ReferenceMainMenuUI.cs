using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    [DefaultExecutionOrder(-999)]
    public class ReferenceMainMenuUI : MonoBehaviour
    {
        const string GameScene="SampleScene";
        const string SaveKey="SurvivalQuest.SaveExists";
        const string MasterKey="SurvivalQuest.MasterVolume";
        const string MusicKey="SurvivalQuest.MusicVolume";
        const string SfxKey="SurvivalQuest.SfxVolume";
        const string SensKey="SurvivalQuest.Sensitivity";
        const string InvertKey="SurvivalQuest.InvertY";
        Canvas canvas; GameObject settings; bool loading;
        void Awake(){Time.timeScale=1;Cursor.visible=true;Cursor.lockState=CursorLockMode.None;EnsureEventSystem();}
        void Start(){Build();}
        void Build(){
            if(canvas!=null)return;
            canvas=new GameObject("SURVIVAL QUEST - REFERENCE MENU",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=5000;
            var cs=canvas.GetComponent<CanvasScaler>();cs.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;cs.referenceResolution=new Vector2(1659,948);cs.matchWidthOrHeight=.5f;
            var root=Rect("Reference Artwork",canvas.transform,Vector2.zero,Vector2.one);
            var raw=root.AddComponent<RawImage>();raw.texture=LoadArtwork();raw.raycastTarget=false;
            Hit(root.transform,"PLAY",.045f,.525f,.325f,.615f,NewGame);Hit(root.transform,"CONTINUE",.045f,.435f,.325f,.515f,ContinueGame);Hit(root.transform,"NEW GAME",.045f,.345f,.325f,.425f,NewGame);Hit(root.transform,"SETTINGS",.045f,.255f,.325f,.335f,OpenSettings);Hit(root.transform,"EXIT",.045f,.165f,.325f,.245f,QuitGame);Hit(root.transform,"AUDIO",.842f,.035f,.902f,.115f,ToggleMute);Hit(root.transform,"FULLSCREEN",.913f,.035f,.975f,.115f,ToggleFullscreen);
        }
        Texture2D LoadArtwork(){var sb=new StringBuilder();for(int i=0;i<3;i++){var t=Resources.Load<TextAsset>("MainMenuArtwork/p0"+i);if(t==null){Debug.LogError("Missing MainMenuArtwork/p0"+i+".txt");return Texture2D.blackTexture;}sb.Append(t.text.Trim());}var tex=new Texture2D(2,2,TextureFormat.RGB24,false,true);tex.LoadImage(Convert.FromBase64String(sb.ToString()),false);tex.filterMode=FilterMode.Bilinear;tex.wrapMode=TextureWrapMode.Clamp;return tex;}
        void Hit(Transform p,string n,float x1,float y1,float x2,float y2,UnityEngine.Events.UnityAction a){var g=Rect(n+" Click",p,new Vector2(x1,y1),new Vector2(x2,y2));var im=g.AddComponent<Image>();im.color=new Color(0,0,0,0);var b=g.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(a);}
        void OpenSettings(){if(settings!=null){settings.SetActive(true);return;}settings=Rect("SETTINGS",canvas.transform,new Vector2(.17f,.08f),new Vector2(.83f,.92f));var bg=settings.AddComponent<Image>();bg.color=new Color(.018f,.035f,.008f,.985f);var o=settings.AddComponent<Outline>();o.effectColor=new Color(.72f,.5f,.2f,.95f);o.effectDistance=new Vector2(3,-3);Label(settings.transform,"SETTINGS",38,.07f,.88f,.94f,.96f,TextAnchor.MiddleLeft);Label(settings.transform,"GAMEPLAY   •   AUDIO   •   DISPLAY",12,.07f,.82f,.94f,.87f,TextAnchor.MiddleLeft,new Color(.67f,.8f,.42f));Section("AUDIO",.75f);SliderRow("Master Volume",.67f,PlayerPrefs.GetFloat(MasterKey,1),v=>SetFloat(MasterKey,v,true));SliderRow("Music Volume",.57f,PlayerPrefs.GetFloat(MusicKey,.8f),v=>SetFloat(MusicKey,v,false));SliderRow("SFX Volume",.47f,PlayerPrefs.GetFloat(SfxKey,.9f),v=>SetFloat(SfxKey,v,false));Section("GAMEPLAY",.39f);SliderRow("Camera Sensitivity",.31f,PlayerPrefs.GetFloat(SensKey,.5f),v=>SetFloat(SensKey,v,false));ToggleRow("Invert Y Axis",.22f,PlayerPrefs.GetInt(InvertKey,0)==1,v=>{PlayerPrefs.SetInt(InvertKey,v?1:0);PlayerPrefs.Save();});Section("DISPLAY",.14f);ToggleRow("Fullscreen",.07f,Screen.fullScreen,v=>Screen.fullScreen=v);Hit(settings.transform,"BACK",.70f,.01f,.94f,.095f,()=>settings.SetActive(false));Label(settings.transform,"BACK",25,.70f,.01f,.94f,.095f,TextAnchor.MiddleCenter);}
        void Section(string s,float y){Label(settings.transform,s,12,.07f,y,.35f,y+.045f,TextAnchor.MiddleLeft,new Color(.7f,.83f,.46f));var l=Rect(s+" Line",settings.transform,new Vector2(.07f,y-.012f),new Vector2(.93f,y-.008f)).AddComponent<Image>();l.color=new Color(.55f,.37f,.12f,.8f);}
        void SliderRow(string n,float y,float v,UnityEngine.Events.UnityAction<float> a){Label(settings.transform,n,15,.08f,y,.4f,y+.06f,TextAnchor.MiddleLeft);var h=Rect(n+" Slider",settings.transform,new Vector2(.43f,y+.008f),new Vector2(.9f,y+.06f));var s=h.AddComponent<Slider>();s.minValue=0;s.maxValue=1;s.value=Mathf.Clamp01(v);s.onValueChanged.AddListener(a);var tr=Rect("Track",h.transform,Vector2.zero,Vector2.one).AddComponent<Image>();tr.color=new Color(.07f,.045f,.015f);tr.raycastTarget=true;var fi=Rect("Fill",h.transform,Vector2.zero,new Vector2(.5f,1));fi.AddComponent<Image>().color=new Color(.73f,.52f,.18f);var ha=Rect("Handle",h.transform,new Vector2(0,.5f),new Vector2(0,.5f));ha.GetComponent<RectTransform>().sizeDelta=new Vector2(28,34);ha.AddComponent<Image>().color=new Color(.98f,.82f,.43f);s.fillRect=fi.GetComponent<RectTransform>();s.handleRect=ha.GetComponent<RectTransform>();}
        void ToggleRow(string n,float y,bool v,UnityEngine.Events.UnityAction<bool> a){Label(settings.transform,n,15,.08f,y,.55f,y+.06f,TextAnchor.MiddleLeft);var g=Rect(n+" Toggle",settings.transform,new Vector2(.8f,y),new Vector2(.91f,y+.065f));var bg=g.AddComponent<Image>();bg.color=new Color(.08f,.055f,.018f);var t=g.AddComponent<Toggle>();t.targetGraphic=bg;t.isOn=v;t.onValueChanged.AddListener(a);var c=Rect("Check",g.transform,new Vector2(.08f,.16f),new Vector2(.46f,.84f)).AddComponent<Image>();c.color=new Color(.74f,.54f,.18f);c.raycastTarget=false;t.graphic=c;}
        Text Label(Transform p,string s,int z,float x1,float y1,float x2,float y2,TextAnchor a,Color? c=null){var t=Rect(s+" Label",p,new Vector2(x1,y1),new Vector2(x2,y2)).AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=s;t.fontSize=z;t.fontStyle=FontStyle.Bold;t.alignment=a;t.color=c??new Color(.95f,.91f,.79f);t.raycastTarget=false;return t;}
        void NewGame(){PlayerPrefs.SetInt(SaveKey,1);PlayerPrefs.Save();LoadGame();}void ContinueGame(){LoadGame();}void LoadGame(){if(loading)return;loading=true;Time.timeScale=1;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GameScene);}void SetFloat(string k,float v,bool m){PlayerPrefs.SetFloat(k,v);PlayerPrefs.Save();if(m)AudioListener.volume=v;}void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0:PlayerPrefs.GetFloat(MasterKey,1);}void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;}
        void QuitGame(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        static GameObject Rect(string n,Transform p,Vector2 min,Vector2 max){var g=new GameObject(n,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;return g;}
        static void EnsureEventSystem(){var e=FindObjectsByType<EventSystem>(FindObjectsSortMode.None);if(e.Length>0){for(int i=1;i<e.Length;i++)Destroy(e[i].gameObject);return;}var g=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            g.AddComponent<InputSystemUIInputModule>();
#else
            g.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
