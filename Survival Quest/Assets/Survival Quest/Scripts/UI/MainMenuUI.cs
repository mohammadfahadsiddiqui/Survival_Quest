using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivalGame.ScriptableObjects;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace SurvivalGame.UI
{
    /// <summary>Standalone main menu. MainMenu.unity loads SampleScene when gameplay starts.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private DataStorage m_DataStorage;
        [SerializeField] private GameplayData m_GameplayData;
        private Canvas m_Canvas;
        private GameObject m_MenuRoot;
        private GameObject m_SettingsRoot;
        private Text m_Status;
        private Button m_ContinueButton;
        private Slider m_MasterSlider, m_MusicSlider, m_SfxSlider, m_SensitivitySlider;
        private Toggle m_InvertYToggle, m_FullscreenToggle;
        private Dropdown m_QualityDropdown;
        private bool m_LoadingGame;

        private const string GameplayScene = "SampleScene";
        private const string SaveKey = "SurvivalQuest.SaveExists";
        private const string MasterKey = "SurvivalQuest.MasterVolume";
        private const string MusicKey = "SurvivalQuest.MusicVolume";
        private const string SfxKey = "SurvivalQuest.SfxVolume";
        private const string SensitivityKey = "SurvivalQuest.Sensitivity";
        private const string InvertKey = "SurvivalQuest.InvertY";

        private void Awake()
        {
            EnsureEventSystem();
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start() => BuildMenu();

        private void BuildMenu()
        {
            GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            m_Canvas = canvasObject.GetComponent<Canvas>();
            m_Canvas.transform.SetParent(transform, false);
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            m_MenuRoot = CreateRect("Menu Root", m_Canvas.transform, Vector2.zero, Vector2.one);
            RawImage background = AddRawImage(m_MenuRoot.transform, "Generated Wilderness Background", Color.white, Vector2.zero, Vector2.one);
            background.texture = GenerateLowPolyBackground(960, 540);
            background.raycastTarget = false;
            AddImage(m_MenuRoot.transform, "Global Shade", new Color(0.01f,0.018f,0.008f,0.28f), Vector2.zero, Vector2.one);
            AddImage(m_MenuRoot.transform, "Left Shade", new Color(0.005f,0.012f,0.006f,0.68f), new Vector2(0f,0f), new Vector2(0.48f,1f));
            AddImage(m_MenuRoot.transform, "Right Shade", new Color(0.005f,0.01f,0.004f,0.42f), new Vector2(0.72f,0f), new Vector2(1f,1f));
            BuildHeader();
            BuildMainButtons();
            BuildInfoCards();
            BuildFooter();
            BuildSettings();
            RefreshContinueState();
        }

        private Texture2D GenerateLowPolyBackground(int width, int height)
        {
            Texture2D tex = new Texture2D(width,height,TextureFormat.RGB24,false) { filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
            Color[] pixels=new Color[width*height];
            Color skyTop=new Color(.10f,.16f,.15f), skyBottom=new Color(.46f,.57f,.38f), ground=new Color(.18f,.36f,.10f);
            for(int y=0;y<height;y++){float t=y/(float)(height-1);Color row=y<height*.58f?Color.Lerp(skyBottom,skyTop,t/.58f):ground;for(int x=0;x<width;x++)pixels[y*width+x]=row;}
            tex.SetPixels(pixels);
            DrawPoly(tex,new[]{N(0,.58f),N(.12f,.72f),N(.23f,.55f),N(.38f,.70f),N(.52f,.54f),N(.68f,.70f),N(.82f,.54f),N(1,.68f),N(1,.80f),N(0,.80f)},new Color(.18f,.25f,.17f));
            DrawPoly(tex,new[]{N(0,.70f),N(.15f,.78f),N(.28f,.64f),N(.42f,.79f),N(.57f,.65f),N(.72f,.79f),N(.86f,.63f),N(1,.76f),N(1,1),N(0,1)},new Color(.10f,.25f,.075f));
            DrawTree(tex,.07f,.70f,1.30f,new Color(.07f,.19f,.055f)); DrawTree(tex,.19f,.73f,.90f,new Color(.10f,.24f,.065f)); DrawTree(tex,.33f,.69f,1.15f,new Color(.08f,.20f,.055f)); DrawTree(tex,.56f,.70f,1.22f,new Color(.10f,.24f,.065f)); DrawTree(tex,.74f,.67f,1.40f,new Color(.07f,.18f,.05f)); DrawTree(tex,.89f,.73f,1.0f,new Color(.10f,.22f,.06f));
            DrawRock(tex,.43f,.81f,.14f,.07f,new Color(.25f,.29f,.24f)); DrawRock(tex,.80f,.80f,.18f,.09f,new Color(.27f,.30f,.25f)); DrawCabin(tex,.57f,.73f,.16f,.13f);
            tex.Apply(); return tex;
        }

        private Vector2 N(float x,float y)=>new Vector2(x,y);
        private void DrawTree(Texture2D tex,float x,float groundY,float scale,Color foliage){int cx=Mathf.RoundToInt(x*tex.width),gy=Mathf.RoundToInt(groundY*tex.height),th=Mathf.RoundToInt(70*scale),tw=Mathf.Max(5,Mathf.RoundToInt(8*scale));DrawRect(tex,cx-tw/2,gy-th,tw,th,new Color(.22f,.13f,.055f));DrawTriangle(tex,new Vector2(cx,gy-th-90*scale),85*scale,120*scale,foliage);DrawTriangle(tex,new Vector2(cx,gy-th-42*scale),105*scale,120*scale,Color.Lerp(foliage,Color.black,.15f));}
        private void DrawRock(Texture2D tex,float x,float y,float w,float h,Color c){int cx=Mathf.RoundToInt(x*tex.width),cy=Mathf.RoundToInt(y*tex.height),rw=Mathf.RoundToInt(w*tex.width),rh=Mathf.RoundToInt(h*tex.height);DrawTriangle(tex,new Vector2(cx,cy-rh),rw*.55f,rh*1.5f,c);}
        private void DrawCabin(Texture2D tex,float x,float y,float w,float h){int cx=Mathf.RoundToInt(x*tex.width),cy=Mathf.RoundToInt(y*tex.height),bw=Mathf.RoundToInt(w*tex.width),bh=Mathf.RoundToInt(h*tex.height);DrawRect(tex,cx-bw/2,cy-bh,bw,bh,new Color(.28f,.17f,.08f));DrawTriangle(tex,new Vector2(cx,cy-bh-bh*.62f),bw*.62f,bh*.75f,new Color(.10f,.17f,.18f));DrawRect(tex,cx-bw/12,cy-bh/2,Mathf.Max(3,bw/8),Mathf.Max(3,bh/2),new Color(.07f,.045f,.025f));}
        private void DrawTriangle(Texture2D tex,Vector2 center,float halfWidth,float height,Color c){int minX=Mathf.Max(0,Mathf.FloorToInt(center.x-halfWidth)),maxX=Mathf.Min(tex.width-1,Mathf.CeilToInt(center.x+halfWidth)),minY=Mathf.Max(0,Mathf.FloorToInt(center.y-height)),maxY=Mathf.Min(tex.height-1,Mathf.CeilToInt(center.y));for(int y=minY;y<=maxY;y++){float t=(y-minY)/Mathf.Max(1f,maxY-minY),half=Mathf.Lerp(halfWidth,0,t);for(int x=Mathf.Max(minX,Mathf.RoundToInt(center.x-half));x<=Mathf.Min(maxX,Mathf.RoundToInt(center.x+half));x++)tex.SetPixel(x,y,c);}}
        private void DrawPoly(Texture2D tex,Vector2[] norm,Color c){Vector2[] p=new Vector2[norm.Length];for(int i=0;i<p.Length;i++)p[i]=new Vector2(norm[i].x*tex.width,(1f-norm[i].y)*tex.height);for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++)if(Inside(new Vector2(x,y),p))tex.SetPixel(x,y,c);}
        private bool Inside(Vector2 p,Vector2[] poly){bool inside=false;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++){if(((poly[i].y>p.y)!=(poly[j].y>p.y))&&p.x<(poly[j].x-poly[i].x)*(p.y-poly[i].y)/((poly[j].y-poly[i].y)+.00001f)+poly[i].x)inside=!inside;}return inside;}
        private void DrawRect(Texture2D tex,int x,int y,int w,int h,Color c){int x2=Mathf.Clamp(x+w,0,tex.width),y2=Mathf.Clamp(y+h,0,tex.height);x=Mathf.Clamp(x,0,tex.width);y=Mathf.Clamp(y,0,tex.height);for(int yy=y;yy<y2;yy++)for(int xx=x;xx<x2;xx++)tex.SetPixel(xx,yy,c);}

        private void BuildHeader(){Text title=AddText(m_MenuRoot.transform,"Title","SURVIVAL QUEST",64,FontStyle.Bold,new Color(.98f,.95f,.78f),TextAnchor.MiddleLeft,new Vector2(.045f,.82f),new Vector2(.48f,.94f));AddShadow(title.gameObject,new Vector2(5,-5),.95f);Text sub=AddText(m_MenuRoot.transform,"Subtitle","THE WILDERNESS DOESN'T FORGIVE",17,FontStyle.Bold,new Color(.75f,.88f,.48f),TextAnchor.MiddleLeft,new Vector2(.048f,.77f),new Vector2(.48f,.82f));AddShadow(sub.gameObject,new Vector2(2,-2),.8f);AddImage(m_MenuRoot.transform,"Accent",new Color(.72f,.49f,.18f,.95f),new Vector2(.048f,.755f),new Vector2(.32f,.762f));}

        private void BuildMainButtons(){GameObject c=CreateRect("Main Menu Buttons",m_MenuRoot.transform,new Vector2(.045f,.17f),new Vector2(.45f,.74f));string[] labels={"PLAY","CONTINUE","NEW GAME","SETTINGS","EXIT"};for(int i=0;i<labels.Length;i++){float top=.96f-i*.18f,bottom=top-.12f;Button b=CreateWoodButton(c.transform,labels[i],new Vector2(.03f,bottom),new Vector2(.88f,top));if(i==0||i==2)b.onClick.AddListener(StartNewJourney);else if(i==1){m_ContinueButton=b;b.onClick.AddListener(ContinueJourney);}else if(i==3)b.onClick.AddListener(OpenSettings);else b.onClick.AddListener(QuitGame);}m_Status=AddText(c.transform,"Status","",13,FontStyle.Bold,new Color(1,.76f,.25f),TextAnchor.MiddleLeft,new Vector2(.03f,0),new Vector2(.90f,.055f));}

        private void BuildInfoCards(){CreateInfoCard("SURVIVE THE UNKNOWN","Explore • Gather • Craft • Survive",.57f,.69f,.965f,.91f,"FIELD GUIDE");CreateInfoCard("YOUR JOURNEY",HasSave()?"SAVE DATA FOUND":"NO SAVE DATA",.57f,.40f,.965f,.62f,"EXPEDITION STATUS");}
        private void CreateInfoCard(string title,string body,float xmin,float ymin,float xmax,float ymax,string tag){GameObject card=CreateRect(title,m_MenuRoot.transform,new Vector2(xmin,ymin),new Vector2(xmax,ymax));Image bg=card.AddComponent<Image>();bg.sprite=CreateWoodSprite(new Color(.11f,.15f,.075f),new Color(.018f,.028f,.012f));bg.type=Image.Type.Sliced;bg.color=new Color(1,1,1,.90f);Outline o=card.AddComponent<Outline>();o.effectColor=new Color(.55f,.38f,.16f,.95f);o.effectDistance=new Vector2(2,-2);AddShadow(card,new Vector2(6,-6),.8f);AddText(card.transform,"Title",title,21,FontStyle.Bold,Color.white,TextAnchor.MiddleLeft,new Vector2(.055f,.52f),new Vector2(.95f,.82f));AddText(card.transform,"Body",body,13,FontStyle.Normal,new Color(.85f,.92f,.72f),TextAnchor.MiddleLeft,new Vector2(.055f,.30f),new Vector2(.95f,.52f));AddText(card.transform,"Tag",tag,9,FontStyle.Bold,new Color(.69f,.80f,.49f),TextAnchor.MiddleRight,new Vector2(.40f,.07f),new Vector2(.95f,.19f));AddImage(card.transform,"ImageAccent",new Color(.22f,.35f,.14f,.50f),new Vector2(.72f,.12f),new Vector2(.94f,.45f));}

        private void BuildFooter(){AddText(m_MenuRoot.transform,"Version","VERSION 0.1",11,FontStyle.Normal,new Color(.80f,.88f,.64f,.90f),TextAnchor.MiddleLeft,new Vector2(.045f,.035f),new Vector2(.20f,.065f));AddText(m_MenuRoot.transform,"Copyright","© SURVIVAL QUEST",11,FontStyle.Normal,new Color(.80f,.88f,.64f,.70f),TextAnchor.MiddleLeft,new Vector2(.045f,.005f),new Vector2(.24f,.035f));Button mute=CreateUtilityButton("♪",.885f,.035f);mute.onClick.AddListener(ToggleMute);Button full=CreateUtilityButton("□",.94f,.035f);full.onClick.AddListener(ToggleFullscreen);}
        private Button CreateUtilityButton(string label,float x,float y){GameObject go=CreateRect("Utility",m_MenuRoot.transform,new Vector2(x,y),new Vector2(x+.045f,y+.06f));Image i=go.AddComponent<Image>();i.sprite=CreateWoodSprite(new Color(.20f,.12f,.04f),new Color(.055f,.025f,.008f));i.type=Image.Type.Sliced;Button b=go.AddComponent<Button>();b.targetGraphic=i;AddText(go.transform,"Icon",label,18,FontStyle.Bold,new Color(.95f,.88f,.68f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);return b;}

        private void BuildSettings(){m_SettingsRoot=CreateRect("Settings Panel",m_MenuRoot.transform,new Vector2(.16f,.07f),new Vector2(.84f,.93f));Image p=m_SettingsRoot.AddComponent<Image>();p.sprite=CreateWoodSprite(new Color(.075f,.105f,.045f),new Color(.012f,.020f,.008f));p.type=Image.Type.Sliced;p.color=new Color(1,1,1,.98f);Outline o=m_SettingsRoot.AddComponent<Outline>();o.effectColor=new Color(.62f,.43f,.18f,.95f);o.effectDistance=new Vector2(3,-3);AddShadow(m_SettingsRoot,new Vector2(8,-8),.85f);AddText(m_SettingsRoot.transform,"Title","SETTINGS",36,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleLeft,new Vector2(.06f,.87f),new Vector2(.94f,.97f));AddText(m_SettingsRoot.transform,"Sub","GAMEPLAY   •   AUDIO   •   DISPLAY",11,FontStyle.Bold,new Color(.68f,.80f,.48f),TextAnchor.MiddleLeft,new Vector2(.065f,.81f),new Vector2(.94f,.87f));CreateSection("AUDIO",.73f);m_MasterSlider=CreateSlider("Master Volume",.66f,PlayerPrefs.GetFloat(MasterKey,1),SetMaster);m_MusicSlider=CreateSlider("Music Volume",.57f,PlayerPrefs.GetFloat(MusicKey,.8f),SetMusic);m_SfxSlider=CreateSlider("SFX Volume",.48f,PlayerPrefs.GetFloat(SfxKey,.9f),SetSfx);CreateSection("GAMEPLAY",.38f);m_SensitivitySlider=CreateSlider("Camera Sensitivity",.31f,PlayerPrefs.GetFloat(SensitivityKey,.5f),SetSensitivity);m_InvertYToggle=CreateToggle("Invert Y Axis",.22f,PlayerPrefs.GetInt(InvertKey,0)==1,SetInvertY);CreateSection("DISPLAY",.13f);m_FullscreenToggle=CreateToggle("Fullscreen",.07f,Screen.fullScreen,SetFullscreen);m_QualityDropdown=CreateQualityDropdown(.07f);Button back=CreateWoodButton(m_SettingsRoot.transform,"BACK",new Vector2(.72f,.015f),new Vector2(.93f,.09f));back.onClick.AddListener(CloseSettings);m_SettingsRoot.SetActive(false);}
        private void CreateSection(string text,float y){AddText(m_SettingsRoot.transform,text+" Label",text,12,FontStyle.Bold,new Color(.72f,.84f,.50f),TextAnchor.MiddleLeft,new Vector2(.065f,y),new Vector2(.50f,y+.045f));AddImage(m_SettingsRoot.transform,text+" Line",new Color(.42f,.31f,.14f,.8f),new Vector2(.065f,y-.008f),new Vector2(.93f,y-.004f));}
        private Slider CreateSlider(string label,float y,float value,UnityEngine.Events.UnityAction<float> cb){AddText(m_SettingsRoot.transform,label+" Label",label,14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.39f,y+.055f));GameObject go=CreateRect(label+" Slider",m_SettingsRoot.transform,new Vector2(.43f,y+.006f),new Vector2(.90f,y+.058f));Slider s=go.AddComponent<Slider>();s.minValue=0;s.maxValue=1;s.value=value;s.onValueChanged.AddListener(cb);Image track=CreateRect("Track",go.transform,Vector2.zero,Vector2.one).AddComponent<Image>();track.color=new Color(.08f,.065f,.035f);track.raycastTarget=false;GameObject fill=CreateRect("Fill",go.transform,Vector2.zero,new Vector2(.5f,1));Image fi=fill.AddComponent<Image>();fi.color=new Color(.70f,.52f,.19f);GameObject handle=CreateRect("Handle",go.transform,new Vector2(0,.5f),new Vector2(0,.5f));Image hi=handle.AddComponent<Image>();hi.color=new Color(.96f,.82f,.46f);s.fillRect=fill.GetComponent<RectTransform>();s.handleRect=handle.GetComponent<RectTransform>();return s;}
        private Toggle CreateToggle(string label,float y,bool value,UnityEngine.Events.UnityAction<bool> cb){AddText(m_SettingsRoot.transform,label+" Label",label,14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.52f,y+.055f));GameObject go=CreateRect(label+" Toggle",m_SettingsRoot.transform,new Vector2(.78f,y),new Vector2(.91f,y+.055f));Toggle t=go.AddComponent<Toggle>();t.isOn=value;t.onValueChanged.AddListener(cb);Image bg=go.AddComponent<Image>();bg.color=new Color(.10f,.08f,.04f);t.targetGraphic=bg;GameObject check=CreateRect("Check",go.transform,new Vector2(.08f,.15f),new Vector2(.92f,.85f));Image ci=check.AddComponent<Image>();ci.color=new Color(.72f,.55f,.20f);t.graphic=ci;return t;}
        private Dropdown CreateQualityDropdown(float y){AddText(m_SettingsRoot.transform,"Quality Label","Graphics Quality",14,FontStyle.Bold,new Color(.92f,.90f,.80f),TextAnchor.MiddleLeft,new Vector2(.08f,y),new Vector2(.52f,y+.055f));GameObject go=CreateRect("Quality Dropdown",m_SettingsRoot.transform,new Vector2(.58f,y),new Vector2(.91f,y+.055f));Image i=go.AddComponent<Image>();i.color=new Color(.10f,.08f,.04f);Dropdown d=go.AddComponent<Dropdown>();d.targetGraphic=i;d.options.Add(new Dropdown.OptionData("Low"));d.options.Add(new Dropdown.OptionData("Medium"));d.options.Add(new Dropdown.OptionData("High"));d.options.Add(new Dropdown.OptionData("Ultra"));d.value=Mathf.Clamp(QualitySettings.GetQualityLevel(),0,3);d.onValueChanged.AddListener(SetQuality);Text caption=AddText(go.transform,"Caption","High",13,FontStyle.Bold,new Color(.95f,.90f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);d.captionText=caption;return d;}

        private void OpenSettings(){m_SettingsRoot.SetActive(true);} private void CloseSettings(){m_SettingsRoot.SetActive(false);}
        private void StartNewJourney(){PlayerPrefs.SetInt(SaveKey,1);PlayerPrefs.Save();LoadGameplay();}
        private void ContinueJourney(){if(!HasSave()){if(m_Status!=null)m_Status.text="NO SAVE DATA — START A NEW JOURNEY";return;}LoadGameplay();}
        private void LoadGameplay(){if(m_LoadingGame)return;m_LoadingGame=true;Time.timeScale=1;Cursor.visible=false;Cursor.lockState=CursorLockMode.Locked;SceneManager.LoadScene(GameplayScene,LoadSceneMode.Single);}
        private void QuitGame(){#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        private void ToggleMute(){AudioListener.volume=AudioListener.volume>.01f?0:PlayerPrefs.GetFloat(MasterKey,1);}
        private void ToggleFullscreen(){Screen.fullScreen=!Screen.fullScreen;if(m_FullscreenToggle!=null)m_FullscreenToggle.isOn=Screen.fullScreen;}
        private void SetMaster(float v){AudioListener.volume=v;PlayerPrefs.SetFloat(MasterKey,v);PlayerPrefs.Save();}
        private void SetMusic(float v){PlayerPrefs.SetFloat(MusicKey,v);PlayerPrefs.Save();}
        private void SetSfx(float v){PlayerPrefs.SetFloat(SfxKey,v);PlayerPrefs.Save();}
        private void SetSensitivity(float v){PlayerPrefs.SetFloat(SensitivityKey,v);PlayerPrefs.Save();}
        private void SetInvertY(bool v){PlayerPrefs.SetInt(InvertKey,v?1:0);PlayerPrefs.Save();}
        private void SetFullscreen(bool v){Screen.fullScreen=v;}
        private void SetQuality(int v){QualitySettings.SetQualityLevel(Mathf.Clamp(v,0,Mathf.Max(0,QualitySettings.names.Length-1)));}
        private bool HasSave(){return PlayerPrefs.GetInt(SaveKey,0)==1;}
        private void RefreshContinueState(){if(m_ContinueButton!=null)m_ContinueButton.interactable=HasSave();}
        private static void EnsureEventSystem(){if(FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()!=null)return;GameObject go=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }
        private static Button CreateWoodButton(Transform parent,string label,Vector2 min,Vector2 max){GameObject go=CreateRect(label+" Button",parent,min,max);Image i=go.AddComponent<Image>();i.sprite=CreateWoodSprite(new Color(.34f,.20f,.07f),new Color(.09f,.045f,.012f));i.type=Image.Type.Sliced;Button b=go.AddComponent<Button>();b.targetGraphic=i;ColorBlock cb=b.colors;cb.highlightedColor=new Color(1,.94f,.72f);cb.pressedColor=new Color(.82f,.67f,.39f);cb.disabledColor=new Color(.45f,.45f,.45f,.45f);b.colors=cb;Text t=AddText(go.transform,"Label",label,29,FontStyle.Bold,new Color(.97f,.91f,.72f),TextAnchor.MiddleCenter,Vector2.zero,Vector2.one);t.raycastTarget=false;AddShadow(go,new Vector2(4,-4),.8f);return b;}
        private static GameObject CreateRect(string name,Transform parent,Vector2 min,Vector2 max){GameObject go=new GameObject(name,typeof(RectTransform));RectTransform rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;rt.pivot=new Vector2(.5f,.5f);return go;}
        private static Image AddImage(Transform parent,string name,Color c,Vector2 min,Vector2 max){Image i=CreateRect(name,parent,min,max).AddComponent<Image>();i.color=c;return i;}
        private static RawImage AddRawImage(Transform parent,string name,Color c,Vector2 min,Vector2 max){RawImage i=CreateRect(name,parent,min,max).AddComponent<RawImage>();i.color=c;return i;}
        private static Text AddText(Transform parent,string name,string content,int size,FontStyle style,Color color,TextAnchor alignment,Vector2 min,Vector2 max){Text t=CreateRect(name,parent,min,max).AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=content;t.fontSize=size;t.fontStyle=style;t.color=color;t.alignment=alignment;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
        private static void AddShadow(GameObject go,Vector2 distance,float alpha){Shadow s=go.GetComponent<Shadow>()??go.AddComponent<Shadow>();s.effectColor=new Color(0,0,0,alpha);s.effectDistance=distance;s.useGraphicAlpha=true;}
        private static Sprite CreateWoodSprite(Color light,Color dark){const int w=128,h=64;Texture2D tex=new Texture2D(w,h,TextureFormat.RGBA32,false);Color[] p=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++){float grain=Mathf.PerlinNoise(x*.07f,y*.045f);p[y*w+x]=Color.Lerp(dark,light,Mathf.Clamp01(.35f+grain*.55f));}tex.SetPixels(p);tex.Apply();return Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect,new Vector4(14,14,14,14));}
        public void BtnExit()=>QuitGame(); public void BtnLevel(int num){if(m_GameplayData!=null)m_GameplayData.LevelNumber=num;LoadGameplay();}
    }
}
