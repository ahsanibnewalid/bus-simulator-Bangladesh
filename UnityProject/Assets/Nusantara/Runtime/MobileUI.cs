using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
namespace Nusantara {
public class MobileUI : MonoBehaviour {
    public BusController bus;
    public PassengerManager passengers;
    public Career career;
    public Commerce commerce;
    public CameraRig cameraRig;
    public LiveryManager livery;
    public BusUpgradeSO engineUpgrade, brakeUpgrade, suspensionUpgrade;
    public EnvironmentController environment;
    public FleetManager fleet;
    RectTransform safe, screen;
    CanvasScaler scaler;
    int layoutWidth,layoutHeight;
    GameObject hud, menu, garage, routes, shop, settings, payout;
    Text speedText, passengerText, wallet, rewardText, garageText, liveryStatus, signalText, pressureText;
    Image pressureFill;
    DialGraphic dial;
    Button doubleReward, repair;
    Font font;
    Sprite rounded;
    string page="";
    float refresh, frameTime=.033f;
    bool leftHeld,rightHeld;
    void Start() {
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rounded=RoundSprite();
        if(!FindFirstObjectByType<EventSystem>()) { var events=new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<StandaloneInputModule>(); }
        var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); scaler.matchWidthOrHeight=.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        safe=Rect("Safe Area",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); FitSafeArea();
        hud=Page("HUD",false); menu=Page("Menu",true); garage=Page("Garage",false); routes=Page("Routes",true); shop=Page("Shop",true);settings=Page("Settings",true);payout=Page("Payout",true);
        BuildHud(); BuildMenu(); BuildGarage(); BuildRoutes(); BuildShop(); BuildSettings(); BuildPayout();
        career.ApplySaved(bus,engineUpgrade,brakeUpgrade,suspensionUpgrade);fleet.Initialize();
        Show("menu");ApplyLayout();
    }
    RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 pos,Vector2 size) {
        var go=new GameObject(name,typeof(RectTransform)); var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    RectTransform Box(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,Color color) {
        var r=Rect(name,parent,anchor,anchor,pos,size);var image=r.gameObject.AddComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;image.color=color;return r;
    }
    Text Label(Transform parent,string value,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize=30) {
        var r=Rect(value,parent,anchor,anchor,pos,size); var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=fontSize;t.text=value;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=16;t.resizeTextMaxSize=fontSize;return t;
    }
    Button Button(Transform parent,string label,Vector2 anchor,Vector2 pos,Vector2 size,Action click) {
        var r=Box(parent,label,anchor,pos,size,new Color(.83f,.18f,.18f));var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();
        var colors=b.colors; colors.pressedColor=new Color(.72f,.11f,.11f);colors.disabledColor=new Color(.29f,.29f,.29f);b.colors=colors;
        Label(r,label,new Vector2(.5f,.5f),Vector2.zero,size-Vector2.one*12,27);
        b.onClick.AddListener(()=>click?.Invoke());return b;
    }
    void Hold(string label,Vector2 anchor,Vector2 pos,Vector2 size,Action<bool> changed) {
        var r=Box(hud.transform,label,anchor,pos,size,new Color(.83f,.18f,.18f));Label(r,label,new Vector2(.5f,.5f),Vector2.zero,size,26);r.gameObject.AddComponent<HoldControl>().Changed=changed;
    }
    GameObject Page(string name,bool backdrop) {
        var r=Rect(name,safe,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        if(backdrop) { var image=r.gameObject.AddComponent<Image>();image.color=new Color(.05f,.06f,.07f,.88f); }
        return r.gameObject;
    }
    void Header(GameObject panel,string title) { Label(panel.transform,title,new Vector2(.5f,1),new Vector2(0,-100),new Vector2(1100,80),55);Button(panel.transform,"KEMBALI",new Vector2(0,1),new Vector2(160,-90),new Vector2(240,100),()=>Show("menu")); }
    void BuildHud() {
        var h=hud.transform;
        Button(h,"MENU",new Vector2(0,1),new Vector2(150,-80),new Vector2(240,100),()=>Show("menu"));
        wallet=Label(h,"",new Vector2(.5f,1),new Vector2(0,-65),new Vector2(550,80),35);
        var card=Box(h,"Passenger card",new Vector2(1,1),new Vector2(-270,-135),new Vector2(500,225),new Color(.1f,.1f,.1f,.85f));
        passengerText=Label(card,"",new Vector2(.5f,.5f),Vector2.zero,new Vector2(465,210),27);
        var gauge=Box(h,"Instrument",new Vector2(0,0),new Vector2(205,460),new Vector2(330,300),new Color(.07f,.07f,.07f,.88f));
        var face=Rect("Speed dial",gauge,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);dial=face.gameObject.AddComponent<DialGraphic>();dial.raycastTarget=false;
        speedText=Label(gauge,"0 km/h",new Vector2(.5f,.5f),new Vector2(0,-62),new Vector2(300,95),26);
        Hold("LEFT",Vector2.zero,new Vector2(125,135),new Vector2(180,180),held=>{leftHeld=held;SetSteering();});
        Hold("RIGHT",Vector2.zero,new Vector2(335,135),new Vector2(180,180),held=>{rightHeld=held;SetSteering();});
        Hold("BRAKE",new Vector2(1,0),new Vector2(-350,145),new Vector2(185,210),held=>bus.brake=held?1:0);
        Hold("DRIVE",new Vector2(1,0),new Vector2(-125,145),new Vector2(185,210),held=>bus.throttle=held?1:0);
        Button(h,"DOOR",new Vector2(1,0),new Vector2(-350,355),new Vector2(180,130),bus.ToggleDoor);
        Button(h,"HORN",new Vector2(1,0),new Vector2(-125,355),new Vector2(180,130),bus.Horn);
        Button(h,"SIGNAL L",new Vector2(.5f,0),new Vector2(-210,125),new Vector2(180,110),()=>{bus.signalLeft=!bus.signalLeft;bus.signalRight=false;});
        Button(h,"SIGNAL R",new Vector2(.5f,0),new Vector2(0,125),new Vector2(180,110),()=>{bus.signalRight=!bus.signalRight;bus.signalLeft=false;});
        Button(h,"RETARDER",new Vector2(.5f,0),new Vector2(210,125),new Vector2(180,110),()=>bus.retarder=!bus.retarder);
        Button(h,"LIGHTS",new Vector2(.5f,0),new Vector2(-210,275),new Vector2(180,110),bus.ToggleLights);
        Button(h,"CAMERA",new Vector2(.5f,0),new Vector2(0,275),new Vector2(180,110),cameraRig.Cycle);
        Button(h,"PAUSE",new Vector2(.5f,0),new Vector2(210,275),new Vector2(180,110),()=>Show("menu"));
        var track=Box(h,"Air pressure",new Vector2(.5f,0),new Vector2(0,35),new Vector2(600,25),new Color(.18f,.18f,.18f));
        var fill=Rect("Fill",track,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero); pressureFill=fill.gameObject.AddComponent<Image>();pressureFill.sprite=rounded;pressureFill.color=new Color(1,.76f,.03f);pressureFill.type=Image.Type.Filled;pressureFill.fillMethod=Image.FillMethod.Horizontal;
        pressureText=Label(h,"AIR",new Vector2(.5f,0),new Vector2(0,57),new Vector2(600,40),19);
        signalText=Label(h,"",new Vector2(.5f,1),new Vector2(0,-150),new Vector2(650,70),26);
        Hold("CLUTCH",new Vector2(0,.5f),new Vector2(480,20),new Vector2(180,110),held=>bus.clutch=held?1:0);
        Button(h,"GEAR +",new Vector2(0,.5f),new Vector2(480,150),new Vector2(180,110),()=>bus.Shift(1));
        Button(h,"GEAR -",new Vector2(0,.5f),new Vector2(480,280),new Vector2(180,110),()=>bus.Shift(-1));
    }
    void BuildMenu() {
        var p=menu.transform;Label(p,"NUSANTARA\nHEAVY",new Vector2(.5f,1),new Vector2(0,-195),new Vector2(800,220),88);
        string[] names={"MULAI KERJA","GARASI","RUTE","TOKO","PENGATURAN"}; string[] targets={"drive","garage","routes","shop","settings"};
        for(int i=0;i<names.Length;i++) {string target=targets[i]; Button(p,names[i],new Vector2(.5f,.5f),new Vector2(0,110-i*130),new Vector2(650,105),()=>Show(target));}
    }
    void BuildGarage() {
        Header(garage,"GARASI");var p=garage.transform;
        var panel=Box(p,"Garage controls",new Vector2(1,.5f),new Vector2(-345,-30),new Vector2(650,840),new Color(.1f,.1f,.1f,.94f));
        Button(p,"ROTASI",new Vector2(0,.5f),new Vector2(180,40),new Vector2(280,110),cameraRig.RotateGarage);
        Button(p,"ZOOM",new Vector2(0,.5f),new Vector2(495,40),new Vector2(280,110),cameraRig.ZoomGarage);
        garageText=Label(panel,"",new Vector2(.5f,1),new Vector2(0,-85),new Vector2(610,155),25);
        Color[] colors={new Color(.72f,.11f,.11f),Color.white,new Color(.12f,.42f,.23f),new Color(.12f,.23f,.43f)};
        for(int i=0;i<colors.Length;i++){Color color=colors[i];Button(panel,new[]{"RED","WHITE","GREEN","BLUE"}[i],new Vector2(.5f,1),new Vector2(-225+i*150,-220),new Vector2(140,100),()=>livery.SetColor(color));}
        var inputBox=Box(panel,"Image path",new Vector2(.5f,1),new Vector2(-90,-350),new Vector2(390,100),new Color(.18f,.18f,.18f));
        var input=inputBox.gameObject.AddComponent<InputField>();var inputText=Label(inputBox,"",new Vector2(.5f,.5f),Vector2.zero,new Vector2(365,90),22);input.textComponent=inputText;input.lineType=InputField.LineType.SingleLine;
        Button(panel,"IMPORT",new Vector2(.5f,1),new Vector2(220,-350),new Vector2(180,100),async()=>{await livery.Import(input.text);});
        liveryStatus=Label(panel,"",new Vector2(.5f,1),new Vector2(0,-440),new Vector2(600,70),21);
        Button(panel,"ENGINE / Rp 50k",new Vector2(.5f,1),new Vector2(0,-530),new Vector2(580,90),()=>career.BuyUpgrade(engineUpgrade,bus));
        Button(panel,"BRAKES / Rp 40k",new Vector2(.5f,1),new Vector2(0,-640),new Vector2(580,90),()=>career.BuyUpgrade(brakeUpgrade,bus));
        Button(panel,"SUSPENSION / Rp 35k",new Vector2(.5f,1),new Vector2(0,-750),new Vector2(580,90),()=>career.BuyUpgrade(suspensionUpgrade,bus));
        Button(p,"GALLERY",new Vector2(0,0),new Vector2(180,100),new Vector2(280,110),livery.PickGallery);
        Button(p,"CLEAR",new Vector2(0,0),new Vector2(495,100),new Vector2(280,110),livery.Clear);
        Button(p,"NEXT BUS",new Vector2(0,0),new Vector2(810,100),new Vector2(280,110),()=>fleet.Next());
        Button(p,"SLOT",new Vector2(0,0),new Vector2(180,245),new Vector2(280,110),livery.NextSlot);
        Button(p,"UJI / TEST",new Vector2(0,0),new Vector2(655,245),new Vector2(580,110),()=>Show("drive"));
    }
    void BuildRoutes() { Header(routes,"RUTE");Label(routes.transform,"DEMO: TERMINAL → BANDUNG\n1 km fictional test route • stop at green terminal bays\nOpen doors while stationary to board and unload.\nCity / rural / toll palettes available in Settings.",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1400,420),38);Button(routes.transform,"START ROUTE",new Vector2(.5f,0),new Vector2(0,180),new Vector2(650,120),()=>Show("drive")); }
    void BuildShop() {
        Header(shop,"TOKO");var p=shop.transform;
        Label(p,"Earn Rupiah through completed passenger trips.\nPaid fleet/horn packs require a configured store SDK.",new Vector2(.5f,1),new Vector2(0,-260),new Vector2(1400,180),32);
        Button(p,"LIVERY SLOT / Rp 25k",new Vector2(.5f,.5f),new Vector2(0,80),new Vector2(800,120),()=>career.BuySlot());
        Button(p,"FLEET LICENCE / Rp 150k",new Vector2(.5f,.5f),new Vector2(0,-70),new Vector2(800,120),()=>career.UnlockFleet());
        repair=Button(p,"REPAIR (REWARDED AD)",new Vector2(.5f,.5f),new Vector2(0,-220),new Vector2(800,120),()=>commerce.Reward(()=>career.ClaimRepair(bus)));
    }
    void BuildSettings() {
        Header(settings,"PENGATURAN");var p=settings.transform;
        Button(p,"30 / 60 FPS",new Vector2(.5f,.5f),new Vector2(-380,130),new Vector2(650,120),()=>Application.targetFrameRate=Application.targetFrameRate==60?30:60);
        Button(p,"LOW / HIGH QUALITY",new Vector2(.5f,.5f),new Vector2(380,130),new Vector2(650,120),()=>environment.ToggleQuality());
        Button(p,"AUTO / MANUAL",new Vector2(.5f,.5f),new Vector2(-380,-20),new Vector2(650,120),()=>bus.automatic=!bus.automatic);
        Button(p,"CITY / RURAL / TOLL",new Vector2(.5f,.5f),new Vector2(380,-20),new Vector2(650,120),environment.CycleBiome);
        Button(p,"DAY / NIGHT",new Vector2(.5f,.5f),new Vector2(-380,-170),new Vector2(650,120),environment.ToggleNight);
        Button(p,"RAIN ON / OFF",new Vector2(.5f,.5f),new Vector2(380,-170),new Vector2(650,120),environment.ToggleRain);
    }
    void BuildPayout() {
        Header(payout,"PERJALANAN SELESAI");
        rewardText=Label(payout.transform,"",new Vector2(.5f,.5f),new Vector2(0,100),new Vector2(1300,350),44);
        doubleReward=Button(payout.transform,"×2 LIPAT GANDAKAN / AD",new Vector2(.5f,.5f),new Vector2(0,-150),new Vector2(900,130),()=>commerce.Reward(()=>{career.ClaimDoubleReward();}));
        doubleReward.GetComponent<Image>().color=new Color(1,.76f,.03f);
        Button(payout.transform,"NEW TRIP",new Vector2(.5f,0),new Vector2(0,150),new Vector2(650,120),()=>{Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);});
    }
    void OnEnable() { GameEvents.Telemetry+=Telemetry; GameEvents.TripFinished+=TripFinished; }
    void OnDisable() { GameEvents.Telemetry-=Telemetry;GameEvents.TripFinished-=TripFinished;Time.timeScale=1; }
    void Telemetry(float kmh,float rpm,int gear) { if(speedText) {speedText.text=$"{kmh:0} km/h\n{rpm/1000:0.0} RPM ×1000  D{gear}";dial.SetValue(kmh);} }
    void TripFinished(int count,float comfort,long fare) { rewardText.text=$"Penumpang {count}/45\nComfort {comfort:0}% • Insiden {passengers.Incidents}\nPENDAPATAN Rp {fare:N0}";Show("payout"); }
    void SetSteering() { bus.steer=(rightHeld?1:0)-(leftHeld?1:0); }
    public void Show(string target) {
        if(!hud)return;
        bus.ReleaseControls();leftHeld=rightHeld=false;
        foreach(var p in new[]{hud,menu,garage,routes,shop,settings,payout})p.SetActive(false);
        page=target;var chosen=target=="drive"?hud:target=="garage"?garage:target=="routes"?routes:target=="shop"?shop:target=="settings"?settings:target=="payout"?payout:menu;
        chosen.SetActive(true);Time.timeScale=target=="drive"?1:0;cameraRig.garage=target!="drive";
        cameraRig.cinematic=target!="drive"&&target!="garage";ApplyViewport();
        if(target=="drive")bus.brake=0;
    }
    void Update() {
        FitSafeArea();if(layoutWidth!=Screen.width||layoutHeight!=Screen.height)ApplyLayout();frameTime=Mathf.Lerp(frameTime,Time.unscaledDeltaTime,.04f);
        if(Input.GetKeyDown(KeyCode.Escape))Show(page=="drive"?"menu":"drive");
        if(page=="drive"&&Input.touchCount==0&&!Input.GetMouseButton(0)) {
            bus.throttle=Input.GetKey(KeyCode.W)?1:0;bus.brake=Input.GetKey(KeyCode.S)?1:0;
            if(!leftHeld&&!rightHeld)bus.steer=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0);
            if(Input.GetKeyDown(KeyCode.H))bus.Horn();if(Input.GetKeyDown(KeyCode.O))bus.ToggleDoor();
        }
        if(Time.unscaledTime<refresh)return;refresh=Time.unscaledTime+.2f;
        wallet.text=$"Rp {career.Data.balance:N0}   |   {1/Mathf.Max(.001f,frameTime):0} FPS";
        passengerText.text=$"PENUMPANG {passengers.Count}/45\nComfort {passengers.Comfort:0}% • Rp {passengers.ticketPrice:N0}\n{passengers.Status}";
        passengerText.color=passengers.Comfort>=80?new Color(.3f,.69f,.31f):passengers.Comfort>=50?new Color(1,.6f,0):new Color(.96f,.26f,.21f);
        pressureFill.fillAmount=bus.AirPressure;pressureText.text=$"AIR {bus.AirPressure*100:0}%";
        signalText.text=$"{(bus.signalLeft?"◀":"")}  {(bus.signalRight?"▶":"")}   {(bus.retarder?"RETARDER":"")}   {(bus.doorOpen?"DOOR OPEN":"")}   {(bus.automatic?"AUTO":"MANUAL")}";
        garageText.text=$"Rp {career.Data.balance:N0} • Engine Lv{career.Data.engineLevel} • Brakes Lv{career.Data.brakeLevel} • Suspension Lv{career.Data.suspensionLevel}\nSlots {career.Data.liverySlots} • Fleet {career.Data.fleetOwned} / {fleet.ModelName}";
        liveryStatus.text=livery.Status;
        doubleReward.interactable=commerce.RewardReady&&!career.Data.rewardClaimed&&career.Data.pendingFare>0;
        repair.interactable=commerce.RewardReady&&career.RepairAvailable();
        if(doubleReward.interactable)doubleReward.GetComponent<Image>().color=new Color(1,.76f,.03f,.8f+.2f*Mathf.Sin(Time.unscaledTime*Mathf.PI*2/1.2f));
        foreach(string name in new[]{"CLUTCH","GEAR +","GEAR -"}) {var item=hud.transform.Find(name);if(item)item.gameObject.SetActive(!bus.automatic);}
    }
    void Place(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size) {
        var found=parent.Find(name) as RectTransform;if(!found)return;found.anchorMin=found.anchorMax=anchor;found.anchoredPosition=pos;found.sizeDelta=size;
    }
    void ApplyViewport() { cameraRig.GetComponent<Camera>().rect=page=="garage"?(Screen.height>Screen.width?new Rect(0,.5f,1,.5f):new Rect(0,0,.62f,1)):new Rect(0,0,1,1); }
    void ApplyLayout() {
        bool portrait=Screen.height>Screen.width;layoutWidth=Screen.width;layoutHeight=Screen.height;
        scaler.referenceResolution=portrait?new Vector2(1080,1920):new Vector2(1920,1080);scaler.matchWidthOrHeight=0;
        // Rebuild only when orientation changes, preserving the selected page and live state.
        if(portrait) {
            Place(hud.transform,"MENU",new Vector2(0,1),new Vector2(150,-75),new Vector2(240,100));
            wallet.rectTransform.anchoredPosition=new Vector2(0,-175);
            Place(hud.transform,"Passenger card",new Vector2(1,1),new Vector2(-290,-350),new Vector2(540,240));
            Place(hud.transform,"Instrument",Vector2.zero,new Vector2(190,805),new Vector2(300,280));
            Place(hud.transform,"LEFT",Vector2.zero,new Vector2(110,145),new Vector2(175,200));
            Place(hud.transform,"RIGHT",Vector2.zero,new Vector2(310,145),new Vector2(175,200));
            Place(hud.transform,"BRAKE",new Vector2(1,0),new Vector2(-310,145),new Vector2(175,200));
            Place(hud.transform,"DRIVE",new Vector2(1,0),new Vector2(-110,145),new Vector2(175,200));
            Place(hud.transform,"DOOR",new Vector2(1,0),new Vector2(-310,355),new Vector2(175,140));
            Place(hud.transform,"HORN",new Vector2(1,0),new Vector2(-110,355),new Vector2(175,140));
            string[] utilities={"SIGNAL L","SIGNAL R","RETARDER","LIGHTS","CAMERA","PAUSE"};
            for(int i=0;i<utilities.Length;i++)Place(hud.transform,utilities[i],Vector2.zero,new Vector2(190+i%3*350,510+i/3*135),new Vector2(300,110));
            signalText.rectTransform.anchoredPosition=new Vector2(0,-530);
            Place(hud.transform,"CLUTCH",new Vector2(0,.5f),new Vector2(130,150),new Vector2(200,110));
            Place(hud.transform,"GEAR +",new Vector2(0,.5f),new Vector2(130,280),new Vector2(200,110));
            Place(hud.transform,"GEAR -",new Vector2(0,.5f),new Vector2(130,410),new Vector2(200,110));
            Place(garage.transform,"Garage controls",new Vector2(.5f,0),new Vector2(0,450),new Vector2(1000,840));
            Place(garage.transform,"GALLERY",Vector2.zero,new Vector2(180,960),new Vector2(280,110));
            Place(garage.transform,"CLEAR",Vector2.zero,new Vector2(495,960),new Vector2(280,110));
            Place(garage.transform,"NEXT BUS",Vector2.zero,new Vector2(810,960),new Vector2(280,110));
            Place(garage.transform,"SLOT",Vector2.zero,new Vector2(180,1100),new Vector2(280,110));
            Place(garage.transform,"UJI / TEST",Vector2.zero,new Vector2(655,1100),new Vector2(580,110));
            Place(garage.transform,"ROTASI",Vector2.zero,new Vector2(300,1240),new Vector2(280,110));
            Place(garage.transform,"ZOOM",Vector2.zero,new Vector2(700,1240),new Vector2(280,110));
            string[] names={"30 / 60 FPS","LOW / HIGH QUALITY","AUTO / MANUAL","CITY / RURAL / TOLL","DAY / NIGHT","RAIN ON / OFF"};
            for(int i=0;i<names.Length;i++)Place(settings.transform,names[i],new Vector2(.5f,.5f),new Vector2(0,380-i*150),new Vector2(850,120));
            foreach(var panel in new[]{menu,routes,shop,settings,payout,garage})foreach(RectTransform child in panel.transform)if(child.GetComponent<Text>()&&child.sizeDelta.x>1000)child.sizeDelta=new Vector2(980,child.sizeDelta.y);
        } else if(layoutHeight!=0) {
            // Restore authored landscape coordinates after rotating back.
            RestoreLandscape();
        }
        FitSafeArea();ApplyViewport();
    }
    void RestoreLandscape() {
        Place(hud.transform,"MENU",new Vector2(0,1),new Vector2(150,-80),new Vector2(240,100));wallet.rectTransform.anchoredPosition=new Vector2(0,-65);
        Place(hud.transform,"Passenger card",new Vector2(1,1),new Vector2(-270,-135),new Vector2(500,225));
        Place(hud.transform,"Instrument",Vector2.zero,new Vector2(205,460),new Vector2(330,300));
        Place(hud.transform,"LEFT",Vector2.zero,new Vector2(125,135),new Vector2(180,180));Place(hud.transform,"RIGHT",Vector2.zero,new Vector2(335,135),new Vector2(180,180));
        Place(hud.transform,"BRAKE",new Vector2(1,0),new Vector2(-350,145),new Vector2(185,210));Place(hud.transform,"DRIVE",new Vector2(1,0),new Vector2(-125,145),new Vector2(185,210));
        Place(hud.transform,"DOOR",new Vector2(1,0),new Vector2(-350,355),new Vector2(180,130));Place(hud.transform,"HORN",new Vector2(1,0),new Vector2(-125,355),new Vector2(180,130));
        string[] utilities={"SIGNAL L","SIGNAL R","RETARDER","LIGHTS","CAMERA","PAUSE"};for(int i=0;i<utilities.Length;i++)Place(hud.transform,utilities[i],new Vector2(.5f,0),new Vector2(-210+i%3*210,125+i/3*150),new Vector2(180,110));
        signalText.rectTransform.anchoredPosition=new Vector2(0,-150);
        string[] manual={"CLUTCH","GEAR +","GEAR -"};for(int i=0;i<manual.Length;i++)Place(hud.transform,manual[i],new Vector2(0,.5f),new Vector2(480,20+i*130),new Vector2(180,110));
        Place(garage.transform,"Garage controls",new Vector2(1,.5f),new Vector2(-345,-30),new Vector2(650,840));
        string[] bottom={"GALLERY","CLEAR","NEXT BUS"};for(int i=0;i<3;i++)Place(garage.transform,bottom[i],Vector2.zero,new Vector2(180+i*315,100),new Vector2(280,110));
        Place(garage.transform,"SLOT",Vector2.zero,new Vector2(180,245),new Vector2(280,110));Place(garage.transform,"UJI / TEST",Vector2.zero,new Vector2(655,245),new Vector2(580,110));
        Place(garage.transform,"ROTASI",new Vector2(0,.5f),new Vector2(180,40),new Vector2(280,110));Place(garage.transform,"ZOOM",new Vector2(0,.5f),new Vector2(495,40),new Vector2(280,110));
        string[] names={"30 / 60 FPS","LOW / HIGH QUALITY","AUTO / MANUAL","CITY / RURAL / TOLL","DAY / NIGHT","RAIN ON / OFF"};for(int i=0;i<names.Length;i++)Place(settings.transform,names[i],new Vector2(.5f,.5f),new Vector2(i%2==0?-380:380,130-i/2*150),new Vector2(650,120));
    }
    void OnApplicationPause(bool paused) { if(paused&&hud)Show("menu"); }
    void OnApplicationFocus(bool focus) { if(!focus&&hud)Show("menu"); }
    void FitSafeArea() { var area=Screen.safeArea;safe.anchorMin=new Vector2(area.x/Screen.width,area.y/Screen.height);safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height); }
    Sprite RoundSprite() {
        var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);texture.name="UI rounded corners";
        for(int y=0;y<32;y++)for(int x=0;x<32;x++) {float dx=Mathf.Max(8-x,x-23),dy=Mathf.Max(8-y,y-23);bool inside=dx<=0||dy<=0||dx*dx+dy*dy<=64;texture.SetPixel(x,y,new Color(1,1,1,inside?1:0));}
        texture.Apply();return Sprite.Create(texture,new Rect(0,0,32,32),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,8));
    }
    void OnDestroy() { if(rounded) {Destroy(rounded.texture);Destroy(rounded);} }
}
}
