using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalGame.ScriptableObjects;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;
        private Canvas canvas;
        private GameObject root, settings;
        private Text status;
        private Button continueButton;
        private Toggle invertToggle, fullscreenToggle;
        private const string GAME_SCENE = "SampleScene";
        private const string SAVE = "SurvivalQuest.SaveExists";
        private const string MASTER = "SurvivalQuest.MasterVolume";
        private const string MUSIC = "SurvivalQuest.MusicVolume";
        private const string SFX = "SurvivalQuest.SfxVolume";
        private const string SENS = "SurvivalQuest.Sensitivity";
        private const string INVERT = "SurvivalQuest.InvertY";

        private void Awake()
        {
            EnsureEventSystem();
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start() { Build(); }

        private void Build()
        {
            GameObject c = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = c.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler cs = c.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1600, 900);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = .5f;
            root = Rect("Root", canvas.transform, Vector2.zero, Vector2.one);

            RawImage bg = Raw("Wilderness Background", root.transform, Vector2.zero, Vector2.one);
            bg.texture = MakeBackground(960, 540);
            AddImage(root.transform, "Vignette", new Color(.01f,.015f,.008f,.32f), Vector2.zero, Vector2.one);
            AddImage(root.transform, "LeftShade", new Color(.005f,.01f,.004f,.66f), new Vector2(0,0), new Vector2(.47f,1));
            AddImage(root.transform, "RightShade", new Color(.005f,.008f,.003f,.38f), new Vector2(.72f,0), new Vector2(1,1));

            Text title = TextUI(root.transform,"Title","SURVIVAL QUEST",60,FontStyle.Bold,new Color(.98f,.94f,.77f),TextAnchor.MiddleLeft,new Vector2(.045f,.82f),new Vector2(.49f,.94f));
            Shadow(title.gameObject, new Vector2(5,-5), .95f);
            Text sub = TextUI(root.transform,"Subtitle","THE WILDERNESS DOESN'T FORGIVE",16,FontStyle.Bold,new Color(.75f,.88f,.48f),TextAnchor.MiddleLeft,new Vector2(.048f,.77f),new Vector2(.49f,.82f));
            Shadow(sub.gameObject,new Vector2(2,-2),.8f);
            AddImage(root.transform,"Accent",new Color(.72f,.49f,.18f,.95f),new Vector2(.048f,.755f),new Vector2(.32f,.762f));

            GameObject menu = Rect("Menu",root.transform,new Vector2(.045f,.16f),new Vector2(.45f,.73f));
            string[] labels={"PLAY","CONTINUE","NEW GAME","SETTINGS","EXIT"};
            for(int i=0;i<labels.Length;i++)
            {
                float top=.96f-i*.18f, bottom=top-.12f;
                Button b=Wood(menu.transform,labels[i],new Vector2(.03f,bottom),new Vector2(.88f,top));
                if(i==0||i==2)b.onClick.AddListener(StartNew);
                else if(i==1){continueButton=b;b.onClick.AddListener(Continue);}
                else if(i==3)b.onClick.AddListener(OpenSettings);
                else b.onClick.AddListener(Quit);
            }
            status=TextUI(menu.transform,"Status","",12,FontStyle.Bold,new Color(1,.76f,.25f),TextAnchor.MiddleLeft,new Vector2(.03f,0),new Vector2(.90f,.055f));
            continueButton.interactable=HasSave();

            Card("SURVIVE THE UNKNOWN","Explore • Gather • Craft • Survive",.57f,.69f,.965f,.91f,"FIELD GUIDE");
            Card("YOUR JOURNEY",HasSave()?"SAVE DATA FOUND":"NO SAVE DATA",.57f,.40f,.965f,.62f,"EXPEDITION STATUS");
            TextUI(root.transform,"Version","VERSION 0.1",11,FontStyle.Normal,new Color(.8f,.88f,.64f,.9f),TextAnchor.MiddleLeft,new Vector2(.045f,.035f),new Vector2(.2f,.065f));
            TextUI(root.transform,"Copyright","© SURVIVAL QUEST",11,FontStyle.Normal,new Color(.8f,.88f,.64f,.7f),TextAnchor.MiddleLeft,new Vector2(.045f,.005f),new Vector2(.24f,.035f));
            Button mute=Utility("♪",.885f);mute.onClick.AddListener(Mute);
            Button full=Utility("□",.94f);full.onClick.AddListener(ToggleFullscreen);
            BuildSettings();
        }

        private void Card(string title,string body,float xmin,float ymin,float xmax,float ymax,string tag)
        {
            GameObject g=Rect(title,root.transform,new Vector2(xmin,ymin),new Vector2(xmax,ymax));
            Image i=g.AddComponent<Image>();i.sprite=WoodSprite(new Color(.11f,.15f,.075f),new Color(.018f,.028f,.012f));i.type=Image.Type.Sliced;i.color=new Color(1,1,1,.90f);
            Outline o=g.AddComponent<Outline>();o.effectColor=new Color(.55f,.38f,.16f,.95f);o.effectDistance=new Vector2(2,-2);Shadow(g,new Vector2(6,-6),.75f);
            TextUI(g.transform,"Title",title,20,FontStyle.Bold,Color.white,TextAnchor.MiddleLeft,new Vector2(.055f,.52f),new Vector2(.94f,.82f));
            TextUI(g.transform,"Body",body,13,FontStyle.Normal,new Color(.85f,.92f,.72f),TextAnchor.MiddleLeft,new Vector2(.055f,.30f),new Vector2(.94f,.52f));
            TextUI(g.transform,"Tag",tag,9,FontStyle.Bold,new Color(.69f,.8f,.49f),TextAnchor.MiddleRight,new Vector2(.42f,.07f),new Vector2(.94f,.18f));
            AddImage(g.transform,"Art",new Color(.22f,.35f,.14f,.55f),new Vector2(.72f,.12f),new Vector2(.94f,.45f));
        }

        private void BuildSettings()
        {
            settings=Rect("Settings",root.transform,new Vector2(.16f,.07f),new Vector2(.84f,.93f));
            Image p=settings.AddComponent<Image>();p.sprite=WoodSprite(new Color(.075f,.105f,.045f),new Color(.012f,.02f,.008f));p.type=Image.Type.Sliced;
            Outline o=settings.AddComponent<Outline>();o.effectColor=new Color(.62f,.43f,.18f,.95f);o.effectDistance=new Vector2(3,-3);Shadow(settings,new Vector2(8,-8),.85f);
            TextUI(settings.transform,"Title","SETTINGS",34,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleLeft,new Vector2(.06f,.88f),new Vector2(.94f,.97f));
            TextUI(settings.transform,"Sub","GAMEPLAY   •   AUDIO   •   DISPLAY",11,FontStyle.Bold,new Color(.68f,.8f,.48f),TextAnchor.MiddleLeft,new Vector2(.065f,.82f),new Vector2(.94f,.88f));
            Section("AUDIO",.75f); SliderUI(settings.transform,"Master Volume",.67f,PlayerPrefs.GetFloat(MASTER,1),v=>Save(MASTER,v)); SliderUI(settings.transform,"Music Volume",.58f,PlayerPrefs.GetFloat(MUSIC,.8f),v=>Save(MUSIC,v)); SliderUI(settings.transform,"SFX Volume",.49f,PlayerPrefs.GetFloat(SFX,.9f),v=>Save(SFX,v));
            Section("GAMEPLAY",.40f); SliderUI(settings.transform,"Camera Sensitivity",.32f,PlayerPrefs.GetFloat(SENS,.5f),v=>Save(SENS,v)); invertToggle=ToggleUI(settings.transform,"Invert Y Axis",.23f,PlayerPrefs.GetInt(INVERT,0)==1,v=>PlayerPrefs.SetInt(INVERT,v?1:0));
            Section("DISPLAY",.14f); fullscreenToggle=ToggleUI(settings.transform,"Fullscreen",.075f,Screen.fullScreen,v=>Screen.fullScreen=v); DropdownUI(settings.transform,.075f);
            Button back=Wood(settings.transform,"BACK",new Vector2(.72f,.015f),new Vector2(.93f,.09f));back.onClick.AddListener(CloseSettings);
            settings.SetActive(false);
        }

        private void Section(string s,float y){TextUI(settings.transform,s+"L",s,12,FontStyle.Bold,new Color(.72f,.84f,.5f),TextAnchor.MiddleLeft,new Vector2(.065f,y),new Vector2(.5f,y+.045f));AddImage(settings.transform,s+"Line",new Color(.42f,.31f,.14f,.8f),new Vector2(.065f,y-.008f),new Vector2(.93f,y-.004f));}
        private Slider SliderUI(Transform parent,string label,float y,float value,UnityEngine.Events.UnityAction<float> cb)
        {
            TextUI(parent,label, label,14,FontStyle.Bold,new Color(.92f,.9f,.8f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.40f,y+.055f));
            GameObject g=Rect(label+"Slider",parent,new Vector2(.43f,y+.006f),new Vector2(.90f,y+.058f));Slider s=g.AddComponent<Slider>();s.minValue=0;s.maxValue=1;s.value=value;s.onValueChanged.AddListener(cb);
            Image track=Rect("Track",g.transform,Vector2.zero,Vector2.one).AddComponent<Image>();track.color=new Color(.08f,.065f,.035f);track.raycastTarget=false;
            GameObject fill=Rect("Fill",g.transform,Vector2.zero,new Vector2(.5f,1));Image fi=fill.AddComponent<Image>();fi.color=new Color(.70f,.52f,.19f);fi.raycastTarget=false;
            GameObject handle=Rect("Handle",g.transform,new Vector2(0,.5f),new Vector2(0,.5f));Image hi=handle.AddComponent<Image>();hi.color=new Color(.96f,.82f,.46f);hi.raycastTarget=false;s.fillRect=fill.GetComponent<RectTransform>();s.handleRect=handle.GetComponent<RectTransform>();return s;
        }
        private Toggle ToggleUI(Transform parent,string label,float y,bool value,UnityEngine.Events.UnityAction<bool> cb){TextUI(parent,label,label,14,FontStyle.Bold,new Color(.92f,.9f,.8f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.55f,y+.055f));GameObject g=Rect(label+"Toggle",parent,new Vector2(.78f,y),new Vector2(.91f,y+.055f));Toggle t=g.AddComponent<Toggle>();t.isOn=value;t.onValueChanged.AddListener(cb);Image bg=g.AddComponent<Image>();bg.color=new Color(.1f,.08f,.04f);t.targetGraphic=bg;GameObject check=Rect("Check",g.transform,new Vector2(.08f,.15f),new Vector2(.92f,.85f));Image ci=check.AddComponent<Image>();ci.color=new Color(.72f,.55f,.2f);t.graphic=ci;return t;}
        private void DropdownUI(Transform parent,float y){TextUI(parent,"QualityLabel","Graphics Quality",14,FontStyle.Bold,new Color(.92f,.9f,.8f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.52f,y+.055f));GameObject g=Rect("Quality",parent,new Vector2(.58f,y),new Vector2(.91f,y+.055f));Image bg=g.AddComponent<Image>();bg.color=new Color(.1f,.08f,.04f);Dropdown d=g.AddComponent<Dropdown>();d.targetGraphic=bg;d.options.Add(new Dropdown.OptionData("Low"));d.options.Add(new Dropdown.OptionData("Medium"));d.options.Add(new Dropdown.OptionData("High"));d.options.Add(new Dropdown.OptionData("Ultra"));d.value=Mathf.Clamp(QualitySettings.GetQualityLevel(),0,3);Text caption=TextUI(g.transform,"Caption","High",13,FontStyle.Bold,new Color(.95f,.9f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);d.captionText=caption;d.onValueChanged.AddListener(v=>QualitySettings.SetQualityLevel(Mathf.Clamp(v,0,QualitySettings.names.Length-1)));}

        private void OpenSettings(){settings.SetActive(true);} private void CloseSettings(){settings.SetActive(false);}
        private void StartNew(){PlayerPrefs.SetInt(SAVE,1);PlayerPrefs.Save();LoadGame();}
        private void Continue(){if(!HasSave()){status.text="NO SAVE DATA — START A NEW JOURNEY";return;}LoadGame();}
        private void LoadGame(){if(m_LoadingGame)return;m_LoadingGame=true;Time.timeScale=1;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GAME_SCENE);}
        private void Quit(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void Mute(){AudioListener.volume=AudioListener.volume>.01f?0:PlayerPrefs.GetFloat(MASTER,1);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;if(fullscreenToggle!=null)fullscreenToggle.isOn=Screen.fullScreen;}
        private void Save(string key,float value){PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();if(key==MASTER)AudioListener.volume=value;}
        private bool HasSave(){return PlayerPrefs.GetInt(SAVE,0)==1;}

        private static void EnsureEventSystem(){if(FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()!=null)return;GameObject go=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }
        private static GameObject Rect(string name,Transform parent,Vector2 min,Vector2 max){GameObject g=new GameObject(name,typeof(RectTransform));RectTransform r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);return g;}
        private static Image AddImage(Transform p,string n,Color c,Vector2 min,Vector2 max){Image i=Rect(n,p,min,max).AddComponent<Image>();i.color=c;return i;}
        private static RawImage Raw(string n,Transform p,Vector2 min,Vector2 max){return Rect(n,p,min,max).AddComponent<RawImage>();}
        private static Text TextUI(Transform p,string n,string text,int size,FontStyle style,Color color,TextAnchor align,Vector2 min,Vector2 max){Text t=Rect(n,p,min,max).AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=size;t.fontStyle=style;t.color=color;t.alignment=align;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
        private static void Shadow(GameObject g,Vector2 d,float a){Shadow s=g.GetComponent<Shadow>()??g.AddComponent<Shadow>();s.effectColor=new Color(0,0,0,a);s.effectDistance=d;s.useGraphicAlpha=true;}
        private static Button Wood(Transform p,string label,Vector2 min,Vector2 max){GameObject g=Rect(label+"Button",p,min,max);Image i=g.AddComponent<Image>();i.sprite=WoodSprite(new Color(.34f,.20f,.07f),new Color(.09f,.045f,.012f));i.type=Image.Type.Sliced;Button b=g.AddComponent<Button>();b.targetGraphic=i;ColorBlock c=b.colors;c.highlightedColor=new Color(1,.94f,.72f);c.pressedColor=new Color(.82f,.67f,.39f);c.disabledColor=new Color(.45f,.45f,.45f,.45f);b.colors=c;Text t=TextUI(g.transform,"Label",label,28,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);t.raycastTarget=false;Shadow(g,new Vector2(4,-4),.8f);return b;}
        private Button Utility(string label,float x){GameObject g=Rect("Utility",root.transform,new Vector2(x,.035f),new Vector2(x+.045f,.095f));Image i=g.AddComponent<Image>();i.sprite=WoodSprite(new Color(.2f,.12f,.04f),new Color(.055f,.025f,.008f));i.type=Image.Type.Sliced;Button b=g.AddComponent<Button>();b.targetGraphic=i;TextUI(g.transform,"Icon",label,18,FontStyle.Bold,new Color(.95f,.88f,.68f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);return b;}
        private static Sprite WoodSprite(Color light,Color dark){const int w=128,h=64;Texture2D t=new Texture2D(w,h,TextureFormat.RGBA32,false);Color[] p=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)p[y*w+x]=Color.Lerp(dark,light,Mathf.Clamp01(.35f+Mathf.PerlinNoise(x*.07f,y*.045f)*.55f));t.SetPixels(p);t.Apply();return Sprite.Create(t,new Rect(0,0,w,h),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect,new Vector4(14,14,14,14));}
        private static Texture2D MakeBackground(int w,int h){Texture2D t=new Texture2D(w,h,TextureFormat.RGB24,false);Color[] p=new Color[w*h];for(int y=0;y<h;y++){Color c=Color.Lerp(new Color(.42f,.53f,.36f),new Color(.08f,.14f,.13f),y/(float)h);for(int x=0;x<w;x++)p[y*w+x]=c;}t.SetPixels(p);for(int i=0;i<7;i++){int x=(int)(w*(.08f+i*.14f));int baseY=(int)(h*.78f);for(int yy=baseY-120;yy<baseY;yy++)for(int xx=x-35;xx<x+35;xx++){if(xx>=0&&xx<w&&yy>=0&&yy<h){float half=35f*(yy-(baseY-120))/120f;if(Mathf.Abs(xx-x)<half)p[yy*w+xx]=new Color(.06f,.18f,.05f);}}}t.SetPixels(p);t.Apply();return t;}
        public void BtnExit()=>Quit();
        public void BtnLevel(int num){if(m_GameplayData!=null)m_GameplayData.LevelNumber=num;LoadGame();}
    }
}
