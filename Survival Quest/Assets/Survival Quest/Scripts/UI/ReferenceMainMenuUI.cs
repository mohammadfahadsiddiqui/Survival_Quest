using System.Collections.Generic;
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
            line.raycastTarget = false;
        }

        private void SliderRow(string text,float y,float value,UnityEngine.Events.UnityAction<float> callback)
        {
            Label(settingsPanel.transform,text,14,.08f,y,.40f,y+.06f,TextAnchor.MiddleLeft);
            GameObject holder = Rect(text+" Slider",settingsPanel.transform,new Vector2(.43f,y+.008f),new Vector2(.90f,y+.058f));
            Slider slider = holder.AddComponent<Slider>();
            slider.minValue=0; slider.maxValue=1; slider.value=Mathf.Clamp01(value); slider.interactable=true;
            slider.onValueChanged.AddListener(callback);

            Image track=Rect("Track",holder.transform,Vector2.zero,Vector2.one).AddComponent<Image>();
            track.color=new Color(.07f,.045f,.015f); track.raycastTarget=true;
            GameObject fill=Rect("Fill",holder.transform,Vector2.zero,new Vector2(.5f,1));
            Image fi=fill.AddComponent<Image>(); fi.color=new Color(.73f,.52f,.18f); fi.raycastTarget=false;
            GameObject handle=Rect("Handle",holder.transform,new Vector2(0,.5f),new Vector2(0,.5f));
            handle.GetComponent<RectTransform>().sizeDelta=new Vector2(28,34);
            Image hi=handle.AddComponent<Image>(); hi.color=new Color(.98f,.82f,.43f); hi.raycastTarget=true;
            slider.fillRect=fill.GetComponent<RectTransform>();
            slider.handleRect=handle.GetComponent<RectTransform>();
        }

        private void ToggleRow(string text,float y,bool value,UnityEngine.Events.UnityAction<bool> callback)
        {
            Label(settingsPanel.transform,text,14,.08f,y,.55f,y+.06f,TextAnchor.MiddleLeft);
            GameObject holder=Rect(text+" Toggle",settingsPanel.transform,new Vector2(.78f,y),new Vector2(.91f,y+.065f));
            Image bg=holder.AddComponent<Image>(); bg.color=new Color(.08f,.055f,.018f); bg.raycastTarget=true;
            Toggle toggle=holder.AddComponent<Toggle>(); toggle.targetGraphic=bg; toggle.isOn=value; toggle.interactable=true; toggle.onValueChanged.AddListener(callback);
            GameObject check=Rect("Check",holder.transform,new Vector2(.08f,.15f),new Vector2(.46f,.85f));
            Image ci=check.AddComponent<Image>(); ci.color=new Color(.75f,.54f,.18f); ci.raycastTarget=false; toggle.graphic=ci;
        }

        private Text Label(Transform parent,string text,int size,float x1,float y1,float x2,float y2,TextAnchor align,Color? color=null)
        {
            Text t=Rect(text+" Label",parent,new Vector2(x1,y1),new Vector2(x2,y2)).AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text=text; t.fontSize=size; t.fontStyle=FontStyle.Bold; t.alignment=align;
            t.color=color??new Color(.95f,.91f,.79f); t.raycastTarget=false;
            return t;
        }

        private void Hit(Transform parent,string name,float x1,float y1,float x2,float y2,UnityEngine.Events.UnityAction action)
        {
            GameObject go=Rect(name+" Click",parent,new Vector2(x1,y1),new Vector2(x2,y2));

            // This Graphic has no mesh, but GraphicRaycaster can still use its RectTransform as the hit area.
            InvisibleRaycastGraphic hitGraphic=go.AddComponent<InvisibleRaycastGraphic>();
            hitGraphic.raycastTarget=true;

            Button button=go.AddComponent<Button>();
            button.targetGraphic=hitGraphic;
            button.transition=Selectable.Transition.None;
            button.interactable=true;
            button.navigation=Navigation.defaultNavigation;
            button.onClick.AddListener(action);

            // Pointer events are received by this same invisible hit object.
            HoverGlowController controller=go.AddComponent<HoverGlowController>();
            controller.Glow=go.AddComponent<HoverGlowGraphic>();
        }

        private void StartNew(){LoadGame();}
        private void ContinueGame(){LoadGame();}

        private void LoadGame()
        {
            if(loading)return;
            loading=true;
            Time.timeScale=1f;
            Cursor.visible=false;
            Cursor.lockState=CursorLockMode.Locked;
            SceneManager.LoadScene(GameScene);
        }

        private void CloseSettings(){if(settingsPanel!=null)settingsPanel.SetActive(false);}
        private void Save(string key,float value){PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();}
        private void SetVolume(string key,float value,bool master){Save(key,value);if(master)AudioListener.volume=value;}
        private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0f:PlayerPrefs.GetFloat("SurvivalQuest.MasterVolume",1f);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;}

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }

        private static GameObject Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {
            GameObject go=new GameObject(name,typeof(RectTransform));
            RectTransform rt=go.GetComponent<RectTransform>();
            rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max;
            rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            return go;
        }

        private static void EnsureSingleEventSystem()
        {
            EventSystem[] systems=FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            EventSystem system=null;

            if(systems.Length>0)
            {
                system=systems[0];
                for(int i=1;i<systems.Length;i++)
                    if(systems[i]!=null) Destroy(systems[i].gameObject);
            }
            else
            {
                system=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
            }

            system.enabled=true;

            // The old implementation returned immediately when an EventSystem already existed.
            // That left scenes with an EventSystem but no input module, making every menu control dead.
            if(system.GetComponent<BaseInputModule>()==null)
            {
#if ENABLE_INPUT_SYSTEM
                system.gameObject.AddComponent<InputSystemUIInputModule>();
#else
                system.gameObject.AddComponent<StandaloneInputModule>();
#endif
            }
        }
    }

    internal sealed class InvisibleRaycastGraphic : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }

    internal sealed class HoverGlowController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public HoverGlowGraphic Glow { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(Glow!=null) Glow.SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(Glow!=null) Glow.SetHover(false);
        }
    }

    internal sealed class HoverGlowGraphic : Graphic
    {
        private bool hovering;
        private float pulse=1f;

        private readonly Color[] layerColors =
        {
            new Color(1f, .76f, .28f, .95f),
            new Color(1f, .55f, .10f, .42f),
            new Color(1f, .30f, .02f, .16f)
        };

        private readonly float[] layerWidths = { 2f, 5f, 9f };

        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
            color=Color.white;
        }

        public void SetHover(bool value)
        {
            if(hovering==value)return;
            hovering=value;
            SetVerticesDirty();
        }

        private void Update()
        {
            if(!hovering)return;
            pulse=.82f+Mathf.Sin(Time.unscaledTime*5f)*.18f;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(!hovering)return;

            Rect r=GetPixelAdjustedRect();
            float radius=Mathf.Min(12f,Mathf.Min(r.width,r.height)*.20f);

            for(int layer=layerWidths.Length-1;layer>=0;layer--)
            {
                float width=layerWidths[layer];
                Color c=layerColors[layer];
                c.a*=pulse;
                AddRoundedRing(vh,r,radius,width,c);
            }
        }

        private static void AddRoundedRing(VertexHelper vh,Rect rect,float radius,float width,Color color)
        {
            const int segments=8;
            List<Vector2> outer=BuildRoundedLoop(rect,radius+width*.5f,segments);
            Rect innerRect=new Rect(rect.x+width,rect.y+width,rect.width-width*2f,rect.height-width*2f);
            if(innerRect.width<=1f||innerRect.height<=1f)return;
            List<Vector2> inner=BuildRoundedLoop(innerRect,Mathf.Max(0f,radius-width*.35f),segments);

            int count=Mathf.Min(outer.Count,inner.Count);
            for(int i=0;i<count;i++)
            {
                int next=(i+1)%count;
                UIVertex a=UIVertex.simpleVert; a.position=outer[i]; a.color=color;
                UIVertex b=UIVertex.simpleVert; b.position=outer[next]; b.color=color;
                UIVertex c=UIVertex.simpleVert; c.position=inner[next]; c.color=color;
                UIVertex d=UIVertex.simpleVert; d.position=inner[i]; d.color=color;
                vh.AddUIVertexQuad(new[]{a,b,c,d});
            }
        }

        private static List<Vector2> BuildRoundedLoop(Rect r,float radius,int segments)
        {
            radius=Mathf.Clamp(radius,0f,Mathf.Min(r.width,r.height)*.5f);
            List<Vector2> points=new List<Vector2>(segments*4);
            AddArc(points,new Vector2(r.xMax-radius,r.yMax-radius),radius,0f,90f,segments);
            AddArc(points,new Vector2(r.xMin+radius,r.yMax-radius),radius,90f,180f,segments);
            AddArc(points,new Vector2(r.xMin+radius,r.yMin+radius),radius,180f,270f,segments);
            AddArc(points,new Vector2(r.xMax-radius,r.yMin+radius),radius,270f,360f,segments);
            return points;
        }

        private static void AddArc(List<Vector2> points,Vector2 center,float radius,float start,float end,int segments)
        {
            for(int i=0;i<segments;i++)
            {
                float t=i/(float)segments;
                float angle=Mathf.Lerp(start,end,t)*Mathf.Deg2Rad;
                points.Add(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius);
            }
        }
    }
}
