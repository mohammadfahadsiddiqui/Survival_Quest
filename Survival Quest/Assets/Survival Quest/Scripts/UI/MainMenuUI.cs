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
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;

        private Canvas m_Canvas;
        private GameObject m_Root, m_Settings;
        private Text m_Status, m_QualityText;
        private Button m_Continue;
        private Toggle m_Fullscreen;
        private bool m_LoadingGame;
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

        private void Start() => Build();

        private void Build()
        {
            if (m_Canvas != null) return;
            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = canvasObject.GetComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            m_Root = Rect("Root", m_Canvas.transform, Vector2.zero, Vector2.one);

            RawImage background = Raw("Low Poly Wilderness", m_Root.transform, Vector2.zero, Vector2.one);
            background.texture = MakeBackground(1280, 720);
            AddImage(m_Root.transform, "Vignette", new Color(0f, .015f, 0f, .28f), Vector2.zero, Vector2.one);
            AddImage(m_Root.transform, "Left Shade", new Color(.005f, .015f, .002f, .54f), new Vector2(0, 0), new Vector2(.49f, 1));
            AddImage(m_Root.transform, "Right Shade", new Color(.005f, .012f, .002f, .30f), new Vector2(.70f, 0), new Vector2(1, 1));

            BuildMain();
            BuildCards();
            BuildFooter();
            BuildSettings();
        }

        private void BuildMain()
        {
            Text title = TextUI(m_Root.transform, "Title", "SURVIVAL QUEST", 64, FontStyle.Bold, new Color(.98f,.94f,.77f), TextAnchor.MiddleLeft, new Vector2(.055f,.82f), new Vector2(.48f,.94f));
            AddShadow(title, new Vector2(5,-5), .9f);
            Text sub = TextUI(m_Root.transform, "Subtitle", "THE WILDERNESS DOESN'T FORGIVE", 17, FontStyle.Bold, new Color(.74f,.88f,.48f), TextAnchor.MiddleLeft, new Vector2(.058f,.775f), new Vector2(.48f,.82f));
            AddShadow(sub, new Vector2(2,-2), .7f);
            AddImage(m_Root.transform, "Accent", new Color(.72f,.49f,.18f,.95f), new Vector2(.058f,.758f), new Vector2(.34f,.764f));

            GameObject menu = Rect("Menu", m_Root.transform, new Vector2(.055f,.17f), new Vector2(.44f,.73f));
            string[] labels = { "PLAY", "CONTINUE", "NEW GAME", "SETTINGS", "EXIT" };
            for (int i=0;i<labels.Length;i++)
            {
                float top=.96f-i*.18f, bottom=top-.125f;
                Button b=WoodButton(menu.transform,labels[i],new Vector2(.02f,bottom),new Vector2(.92f,top));
                if(i==0||i==2) b.onClick.AddListener(StartNew);
                else if(i==1){m_Continue=b;b.onClick.AddListener(ContinueGame);}
                else if(i==3)b.onClick.AddListener(OpenSettings);
                else b.onClick.AddListener(QuitGame);
            }
            m_Status=TextUI(menu.transform,"Status","",13,FontStyle.Bold,new Color(1,.76f,.25f),TextAnchor.MiddleLeft,new Vector2(.02f,0),new Vector2(.92f,.055f));
            m_Continue.interactable=HasSave();
        }

        private void BuildCards()
        {
            Card("SURVIVE THE UNKNOWN","Explore • Gather • Craft • Survive","FIELD GUIDE",.58f,.69f,.965f,.91f);
            Card("YOUR JOURNEY",HasSave()?"SAVE DATA FOUND":"NO SAVE DATA","EXPEDITION STATUS",.58f,.40f,.965f,.62f);
        }

        private void Card(string title,string body,string tag,float xmin,float ymin,float xmax,float ymax)
        {
            GameObject card=Rect(title+" Card",m_Root.transform,new Vector2(xmin,ymin),new Vector2(xmax,ymax));
            Image image=card.AddComponent<Image>(); image.sprite=WoodSprite(new Color(.13f,.18f,.08f),new Color(.015f,.025f,.01f)); image.type=Image.Type.Sliced;
            Outline outline=card.AddComponent<Outline>(); outline.effectColor=new Color(.52f,.36f,.15f,.95f); outline.effectDistance=new Vector2(2,-2); AddShadow(card,new Vector2(6,-6),.7f);
            TextUI(card.transform,"Title",title,21,FontStyle.Bold,Color.white,TextAnchor.MiddleLeft,new Vector2(.06f,.52f),new Vector2(.68f,.84f));
            TextUI(card.transform,"Body",body,13,FontStyle.Normal,new Color(.85f,.92f,.72f),TextAnchor.MiddleLeft,new Vector2(.06f,.30f),new Vector2(.68f,.52f));
            TextUI(card.transform,"Tag",tag,9,FontStyle.Bold,new Color(.69f,.80f,.49f),TextAnchor.MiddleRight,new Vector2(.45f,.08f),new Vector2(.94f,.20f));
            RawImage art=Raw("Art",card.transform,new Vector2(.73f,.13f),new Vector2(.94f,.48f)); art.texture=MakeCardArt(240,110); art.color=new Color(1,1,1,.9f);
        }

        private void BuildFooter()
        {
            TextUI(m_Root.transform,"Version","VERSION 0.1",11,FontStyle.Normal,new Color(.80f,.88f,.64f,.9f),TextAnchor.MiddleLeft,new Vector2(.055f,.038f),new Vector2(.20f,.066f));
            TextUI(m_Root.transform,"Copyright","© SURVIVAL QUEST",11,FontStyle.Normal,new Color(.80f,.88f,.64f,.7f),TextAnchor.MiddleLeft,new Vector2(.055f,.008f),new Vector2(.25f,.036f));
            Button mute=Utility("♪",.885f); mute.onClick.AddListener(ToggleMute);
            Button full=Utility("□",.94f); full.onClick.AddListener(ToggleFullscreen);
        }

        private void BuildSettings()
        {
            m_Settings=Rect("Settings",m_Root.transform,new Vector2(.17f,.08f),new Vector2(.83f,.92f));
            Image panel=m_Settings.AddComponent<Image>(); panel.sprite=WoodSprite(new Color(.075f,.105f,.045f),new Color(.012f,.020f,.008f)); panel.type=Image.Type.Sliced; panel.color=new Color(1,1,1,.98f);
            Outline outline=m_Settings.AddComponent<Outline>(); outline.effectColor=new Color(.62f,.43f,.18f,.95f); outline.effectDistance=new Vector2(3,-3); AddShadow(m_Settings,new Vector2(8,-8),.8f);
            TextUI(m_Settings.transform,"Title","SETTINGS",36,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleLeft,new Vector2(.06f,.88f),new Vector2(.94f,.96f));
            TextUI(m_Settings.transform,"Subtitle","GAMEPLAY  •  AUDIO  •  DISPLAY",11,FontStyle.Bold,new Color(.68f,.80f,.48f),TextAnchor.MiddleLeft,new Vector2(.065f,.82f),new Vector2(.94f,.87f));

            Header("AUDIO",.755f);
            SliderRow("Master Volume",.675f,PlayerPrefs.GetFloat(MASTER,1f),v=>Save(MASTER,v));
            SliderRow("Music Volume",.585f,PlayerPrefs.GetFloat(MUSIC,.8f),v=>Save(MUSIC,v));
            SliderRow("SFX Volume",.495f,PlayerPrefs.GetFloat(SFX,.9f),v=>Save(SFX,v));

            Header("GAMEPLAY",.405f);
            SliderRow("Camera Sensitivity",.335f,PlayerPrefs.GetFloat(SENS,.5f),v=>Save(SENS,v));
            ToggleRow("Invert Y Axis",.245f,PlayerPrefs.GetInt(INVERT,0)==1,v=>{PlayerPrefs.SetInt(INVERT,v?1:0);PlayerPrefs.Save();});

            Header("DISPLAY",.155f);
            ToggleRow("Fullscreen",.095f,Screen.fullScreen,v=>Screen.fullScreen=v);
            QualityRow(.025f);

            Button back=WoodButton(m_Settings.transform,"BACK",new Vector2(.72f,.015f),new Vector2(.93f,.085f)); back.onClick.AddListener(CloseSettings);
            m_Settings.SetActive(false);
        }

        private void Header(string label,float y)
        {
            TextUI(m_Settings.transform,label+" Header",label,12,FontStyle.Bold,new Color(.72f,.84f,.50f),TextAnchor.MiddleLeft,new Vector2(.065f,y),new Vector2(.35f,y+.04f));
            AddImage(m_Settings.transform,label+" Line",new Color(.42f,.31f,.14f,.8f),new Vector2(.065f,y-.012f),new Vector2(.935f,y-.008f));
        }

        private void SliderRow(string label,float y,float value,UnityAction<float> callback)
        {
            TextUI(m_Settings.transform,label+" Label",label,14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.39f,y+.055f));
            GameObject g=Rect(label+" Slider",m_Settings.transform,new Vector2(.43f,y+.008f),new Vector2(.90f,y+.058f));
            Slider s=g.AddComponent<Slider>(); s.minValue=0; s.maxValue=1; s.value=Mathf.Clamp01(value); s.onValueChanged.AddListener(callback);
            Image track=Rect("Track",g.transform,Vector2.zero,Vector2.one).AddComponent<Image>(); track.color=new Color(.07f,.055f,.025f); track.raycastTarget=false;
            GameObject fill=Rect("Fill",g.transform,Vector2.zero,new Vector2(.5f,1)); Image fi=fill.AddComponent<Image>(); fi.color=new Color(.70f,.52f,.19f); fi.raycastTarget=false;
            GameObject handle=Rect("Handle",g.transform,new Vector2(0,.5f),new Vector2(0,.5f)); handle.GetComponent<RectTransform>().sizeDelta=new Vector2(28,28); Image hi=handle.AddComponent<Image>(); hi.color=new Color(.96f,.82f,.46f); hi.raycastTarget=false;
            s.fillRect=fill.GetComponent<RectTransform>(); s.handleRect=handle.GetComponent<RectTransform>();
        }

        private void ToggleRow(string label,float y,bool value,UnityAction<bool> callback)
        {
            TextUI(m_Settings.transform,label+" Label",label,14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.50f,y+.055f));
            GameObject g=Rect(label+" Toggle",m_Settings.transform,new Vector2(.80f,y+.004f),new Vector2(.90f,y+.065f)); Image bg=g.AddComponent<Image>(); bg.color=new Color(.08f,.065f,.03f);
            Toggle t=g.AddComponent<Toggle>(); t.isOn=value; t.targetGraphic=bg; t.onValueChanged.AddListener(callback);
            GameObject check=Rect("Check",g.transform,new Vector2(.08f,.16f),new Vector2(.45f,.84f)); Image ci=check.AddComponent<Image>(); ci.color=new Color(.72f,.55f,.20f); ci.raycastTarget=false; t.graphic=ci;
        }

        private void QualityRow(float y)
        {
            TextUI(m_Settings.transform,"Quality Label","Graphics Quality",14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.42f,y+.055f));
            GameObject g=Rect("Graphics Quality",m_Settings.transform,new Vector2(.46f,y+.002f),new Vector2(.68f,y+.064f)); Image bg=g.AddComponent<Image>(); bg.sprite=WoodSprite(new Color(.25f,.15f,.055f),new Color(.07f,.035f,.01f)); bg.type=Image.Type.Sliced;
            Button b=g.AddComponent<Button>(); b.targetGraphic=bg; m_QualityText=TextUI(g.transform,"Value",QualityName(),13,FontStyle.Bold,new Color(.96f,.90f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one); m_QualityText.raycastTarget=false; b.onClick.AddListener(CycleQuality);
        }

        private string QualityName()
        {
            string[] names={"Low","Medium","High","Ultra"}; return names[Mathf.Clamp(QualitySettings.GetQualityLevel(),0,3)];
        }

        private void CycleQuality()
        {
            string[] names={"Low","Medium","High","Ultra"}; int next=(Mathf.Clamp(QualitySettings.GetQualityLevel(),0,3)+1)%4; int max=Mathf.Max(0,QualitySettings.names.Length-1); QualitySettings.SetQualityLevel(Mathf.Min(next,max)); if(m_QualityText!=null)m_QualityText.text=names[next];
        }

        private void OpenSettings(){m_Settings.SetActive(true);}
        private void CloseSettings(){m_Settings.SetActive(false);}
        private void StartNew(){PlayerPrefs.SetInt(SAVE,1);PlayerPrefs.Save();LoadGame();}
        private void ContinueGame(){if(!HasSave()){m_Status.text="NO SAVE DATA — START A NEW JOURNEY";return;}LoadGame();}
        private void LoadGame(){if(m_LoadingGame)return;m_LoadingGame=true;Time.timeScale=1f;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GAME_SCENE);}
        private void QuitGame(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0f:PlayerPrefs.GetFloat(MASTER,1f);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;if(m_Fullscreen!=null)m_Fullscreen.isOn=Screen.fullScreen;}
        private void Save(string key,float value){PlayerPrefs.SetFloat(key,value);PlayerPrefs.Save();if(key==MASTER)AudioListener.volume=value;}
        private bool HasSave(){return PlayerPrefs.GetInt(SAVE,0)==1;}

        private static void EnsureSingleEventSystem()
        {
            var systems=FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
            if(systems.Length>0){for(int i=1;i<systems.Length;i++)Destroy(systems[i].gameObject);return;}
            GameObject go=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        private static GameObject Rect(string name,Transform parent,Vector2 min,Vector2 max)
        {GameObject g=new GameObject(name,typeof(RectTransform));RectTransform r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);return g;}
        private static Image AddImage(Transform p,string n,Color c,Vector2 min,Vector2 max){Image i=Rect(n,p,min,max).AddComponent<Image>();i.color=c;return i;}
        private static RawImage Raw(string n,Transform p,Vector2 min,Vector2 max){return Rect(n,p,min,max).AddComponent<RawImage>();}
        private static Text TextUI(Transform p,string n,string value,int size,FontStyle style,Color color,TextAnchor align,Vector2 min,Vector2 max){Text t=Rect(n,p,min,max).AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.fontStyle=style;t.color=color;t.alignment=align;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
        private static void AddShadow(GameObject g,Vector2 d,float a){Shadow s=g.GetComponent<Shadow>()??g.AddComponent<Shadow>();s.effectColor=new Color(0,0,0,a);s.effectDistance=d;s.useGraphicAlpha=true;}
        private static Button WoodButton(Transform p,string label,Vector2 min,Vector2 max){GameObject g=Rect(label+" Button",p,min,max);Image i=g.AddComponent<Image>();i.sprite=WoodSprite(new Color(.34f,.20f,.07f),new Color(.09f,.045f,.012f));i.type=Image.Type.Sliced;Button b=g.AddComponent<Button>();b.targetGraphic=i;ColorBlock c=b.colors;c.highlightedColor=new Color(1,.94f,.72f);c.pressedColor=new Color(.82f,.67f,.39f);c.disabledColor=new Color(.45f,.45f,.45f,.45f);c.fadeDuration=.08f;b.colors=c;Text t=TextUI(g.transform,"Label",label,28,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);t.raycastTarget=false;AddShadow(g,new Vector2(4,-4),.8f);return b;}
        private Button Utility(string label,float x){GameObject g=Rect("Utility "+label,m_Root.transform,new Vector2(x,.035f),new Vector2(x+.045f,.095f));Image i=g.AddComponent<Image>();i.sprite=WoodSprite(new Color(.20f,.12f,.04f),new Color(.055f,.025f,.008f));i.type=Image.Type.Sliced;Button b=g.AddComponent<Button>();b.targetGraphic=i;Text t=TextUI(g.transform,"Icon",label,18,FontStyle.Bold,new Color(.95f,.88f,.68f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);t.raycastTarget=false;return b;}

        private static Sprite WoodSprite(Color center,Color edge)
        {const int s=32;Texture2D t=new Texture2D(s,s,TextureFormat.RGBA32,false);t.wrapMode=TextureWrapMode.Clamp;for(int y=0;y<s;y++)for(int x=0;x<s;x++){float border=Mathf.Min(Mathf.Min(x,s-1-x),Mathf.Min(y,s-1-y));float grain=Mathf.Sin(y*.75f+x*.08f)*.035f;Color c=Color.Lerp(edge,center,Mathf.Clamp01(border/7f));c.r+=grain;c.g+=grain;c.b+=grain;t.SetPixel(x,y,c);}t.Apply();return Sprite.Create(t,new Rect(0,0,s,s),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,8));}

        private static Texture2D MakeBackground(int w,int h)
        {
            Texture2D t=new Texture2D(w,h,TextureFormat.RGB24,false);t.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){float nx=x/(float)w,ny=y/(float)h;Color sky=Color.Lerp(new Color(.045f,.075f,.03f),new Color(.20f,.32f,.12f),ny);Color ground=Color.Lerp(new Color(.08f,.15f,.03f),new Color(.30f,.52f,.08f),Mathf.Clamp01((ny-.42f)*1.7f));Color c=ny<.43f?sky:ground;float ridge=.22f+.05f*Mathf.Sin(nx*9)+.025f*Mathf.Sin(nx*23);if(ny>.34f&&ny<ridge+.15f)c=Color.Lerp(c,new Color(.28f,.18f,.09f),.75f);if(ny>.45f)c*=.92f+Mathf.Abs(Mathf.Sin(nx*70)*Mathf.Sin(ny*52))*.08f;t.SetPixel(x,y,c);} 
            Tree(t,145,390,150,250);Tree(t,310,350,120,210);Tree(t,1080,350,135,240);Tree(t,1180,425,105,210);Rock(t,620,445,190,115);Rock(t,970,415,130,85);Cabin(t,790,330,190,125);t.Apply();return t;
        }
        private static void Tree(Texture2D t,int cx,int baseY,int height,int width){int top=Mathf.Clamp(baseY-height,0,t.height-1);Color leaves=new Color(.07f,.22f,.04f),dark=new Color(.025f,.075f,.015f),trunk=new Color(.18f,.09f,.035f);for(int y=top;y<baseY;y++){float p=(y-top)/(float)Mathf.Max(1,baseY-top);int half=Mathf.RoundToInt(Mathf.Lerp(width*.12f,width*.5f,p));for(int x=Mathf.Max(0,cx-half);x<=Mathf.Min(t.width-1,cx+half);x++){float edge=Mathf.Abs(x-cx)/(float)Mathf.Max(1,half);if(edge<1)t.SetPixel(x,y,Color.Lerp(leaves,dark,edge));}}for(int y=baseY-8;y<baseY+20;y++)for(int x=cx-8;x<=cx+8;x++)if(x>=0&&x<t.width&&y>=0&&y<t.height)t.SetPixel(x,y,trunk);}
        private static void Rock(Texture2D t,int cx,int cy,int width,int height){Color r=new Color(.22f,.27f,.24f);for(int y=Mathf.Max(0,cy-height/2);y<Mathf.Min(t.height,cy+height/2);y++){float py=(y-(cy-height/2f))/height;float half=Mathf.Sin(py*Mathf.PI)*width*.5f;for(int x=Mathf.Max(0,cx-(int)half);x<Mathf.Min(t.width,cx+(int)half);x++)t.SetPixel(x,y,Color.Lerp(r,new Color(.12f,.15f,.14f),py*.45f));}}
        private static void Cabin(Texture2D t,int cx,int cy,int width,int height){int l=cx-width/2,r=cx+width/2,b=cy,top=cy+height/2;Color wall=new Color(.30f,.19f,.09f),roof=new Color(.07f,.12f,.14f),door=new Color(.12f,.065f,.025f);for(int y=b;y<top;y++)for(int x=l;x<r;x++)if(x>=0&&x<t.width&&y>=0&&y<t.height)t.SetPixel(x,y,wall);for(int y=top;y<top+height/2;y++){float row=(y-top)/(height*.5f);int half=Mathf.RoundToInt(width*.5f*(1-row));for(int x=cx-half;x<=cx+half;x++)if(x>=0&&x<t.width&&y>=0&&y<t.height)t.SetPixel(x,y,roof);}for(int y=b;y<b+height/2;y++)for(int x=cx-16;x<cx+16;x++)if(x>=0&&x<t.width&&y>=0&&y<t.height)t.SetPixel(x,y,door);}
        private static Texture2D MakeCardArt(int w,int h){Texture2D t=new Texture2D(w,h,TextureFormat.RGB24,false);for(int y=0;y<h;y++)for(int x=0;x<w;x++){float ny=y/(float)h;Color c=Color.Lerp(new Color(.06f,.12f,.03f),new Color(.28f,.45f,.08f),ny);t.SetPixel(x,y,c);}t.Apply();Tree(t,w/5,h-10,h/2,w/4);Tree(t,w*4/5,h-5,h*3/5,w/3);Rock(t,w/2,h*3/4,w/3,h/2);t.Apply();return t;}
    }
}
