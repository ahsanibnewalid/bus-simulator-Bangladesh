using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.SceneManagement;
namespace Nusantara.Editor {
public static class ProjectBuilder {
    const string Generated="Assets/Nusantara/Generated";
    public const string BootPath=Generated+"/Boot.unity";
    static Material red,white,gold,glass,black,road,grass,concrete,roof,leaf;
    static void EnsureFolder() {Directory.CreateDirectory(Generated);AssetDatabase.Refresh();}
    static T Asset<T>(T item,string filename) where T:UnityEngine.Object {
        string path=Generated+"/"+filename;
        var existing=AssetDatabase.LoadAssetAtPath<T>(path);
        if(existing) {UnityEngine.Object.DestroyImmediate(item);return existing;}
        AssetDatabase.CreateAsset(item,path);return item;
    }
    static Material Mat(string name,Color color,string shader="Universal Render Pipeline/Lit") {
        var m=new Material(Shader.Find(shader));m.color=color;m.enableInstancing=true;return Asset(m,name+".mat");
    }
    static GameObject Shape(string name,PrimitiveType primitive,Vector3 position,Vector3 scale,Material material,Transform parent=null,bool collide=false,int layer=8) {
        var go=GameObject.CreatePrimitive(primitive);go.name=name;go.layer=layer;
        if(parent){go.transform.SetParent(parent,false);go.transform.localPosition=position;}else go.transform.position=position;
        go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
        if(!collide)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    static BusUpgradeSO Upgrade(string id,long price,float torque,float brakes,float suspension) {
        var u=ScriptableObject.CreateInstance<BusUpgradeSO>();u.upgradeId=id;u.price=price;u.torqueMultiplier=torque;u.brakeMultiplier=brakes;u.suspensionMultiplier=suspension;
        return Asset(u,id+"Upgrade.asset");
    }
    [MenuItem("Nusantara/Generate playable project")]
    public static void Generate() {
        if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode first");
        EnsureFolder();Configure();
        red=Mat("BusBody",new Color(.718f,.11f,.11f),"Nusantara/BusLivery");
        white=Mat("White",Color.white);gold=Mat("Gold",new Color(.788f,.635f,.153f));
        // Opaque tinted windows avoid layered transparent overdraw in the placeholder bus.
        glass=Mat("TintedWindows",new Color(.08f,.14f,.18f));black=Mat("Rubber",new Color(.035f,.035f,.035f));
        road=Mat("Asphalt",new Color(.227f,.227f,.235f));grass=Mat("Grass",new Color(.49f,.70f,.26f));
        concrete=Mat("Concrete",new Color(.81f,.78f,.72f));roof=Mat("Terracotta",new Color(.72f,.36f,.22f));leaf=Mat("Palm",new Color(.18f,.36f,.23f));
        var engine=Upgrade("engine",50000,1.15f,1,1);var brakes=Upgrade("brakes",40000,1,1.12f,1);var suspension=Upgrade("suspension",35000,1,1,1.1f);
        var boot=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;
        Shape("Ground",PrimitiveType.Cube,new Vector3(0,-.3f,600),new Vector3(300,.5f,1400),grass,null,true);
        Shape("Road",PrimitiveType.Cube,new Vector3(0,-.05f,600),new Vector3(14,.1f,1400),road,null,true);
        for(int edge=-1;edge<=1;edge+=2) {var dirt=Shape("Dirt verge",PrimitiveType.Cube,new Vector3(edge*8.5f,-.01f,600),new Vector3(3,.1f,1400),roof,null,true);dirt.tag="Respawn";}
        for(int z=-80;z<1300;z+=15)Shape("Lane marking",PrimitiveType.Cube,new Vector3(0,.015f,z),new Vector3(.15f,.015f,6),white);
        for(int z=-80;z<1300;z+=50) {
            Shape("Guardrail",PrimitiveType.Cube,new Vector3(8,.65f,z),new Vector3(.15f,.3f,48),gold);
        }
        var busRoot=new GameObject("Player Bus");busRoot.layer=9;busRoot.transform.position=new Vector3(-10,1,30);
        busRoot.AddComponent<Rigidbody>();var bus=busRoot.AddComponent<BusController>();
        var body=Shape("Body",PrimitiveType.Cube,new Vector3(0,1.9f,0),new Vector3(2.5f,2.8f,11),red,busRoot.transform,true,9);
        Shape("Window band",PrimitiveType.Cube,new Vector3(0,2.65f,0),new Vector3(2.53f,1.05f,10.6f),glass,busRoot.transform,false,9);
        Shape("Front lower",PrimitiveType.Cube,new Vector3(0,.9f,5.52f),new Vector3(2.5f,.7f,.1f),white,busRoot.transform,false,9);
        Shape("Wood dashboard",PrimitiveType.Cube,new Vector3(0,1.65f,4.6f),new Vector3(2.3f,.35f,.9f),roof,busRoot.transform,false,9);
        Shape("Gold trim",PrimitiveType.Cube,new Vector3(0,2.03f,0),new Vector3(2.56f,.08f,10.9f),gold,busRoot.transform,false,9);
        bus.wheels=new WheelCollider[6];bus.wheelVisuals=new Transform[6];
        for(int i=0;i<6;i++) {
            float x=i%2==0?-1.2f:1.2f,z=i<2?3.6f:i<4?-2.8f:-4;
            var wheel=new GameObject("WheelCollider "+i);wheel.layer=9;wheel.transform.SetParent(busRoot.transform,false);wheel.transform.localPosition=new Vector3(x,.6f,z);
            var wc=wheel.AddComponent<WheelCollider>();wc.radius=.52f;wc.mass=90;wc.suspensionDistance=.3f;wc.wheelDampingRate=2;
            var spring=wc.suspensionSpring;spring.spring=70000;spring.damper=11000;spring.targetPosition=.5f;wc.suspensionSpring=spring;
            bus.wheels[i]=wc;
            var visual=Shape("Wheel visual "+i,PrimitiveType.Cylinder,new Vector3(x,.6f,z),new Vector3(1.04f,.2f,1.04f),black,busRoot.transform,false,9);
            visual.transform.localRotation=Quaternion.Euler(0,0,90);bus.wheelVisuals[i]=visual.transform;
        }
        var passengerMaterial=Mat("Passenger flipbook",Color.white);
        passengerMaterial.SetFloat("_AlphaClip",1);passengerMaterial.SetFloat("_Cull",0);passengerMaterial.EnableKeyword("_ALPHATEST_ON");
        passengerMaterial.SetTexture("_BaseMap",PassengerAtlas());passengerMaterial.SetTextureScale("_BaseMap",new Vector2(.25f,1));
        var passengers=busRoot.AddComponent<PassengerManager>();passengers.bus=bus;passengers.passengerPool=new GameObject[45];
        for(int i=0;i<45;i++) {
            float x=i%4<2?-.65f:.65f,z=-4.3f+i/4*.7f;
            var p=Shape("Pooled passenger "+i,PrimitiveType.Quad,new Vector3(x,1.5f,z),new Vector3(.6f,1.2f,1),passengerMaterial,busRoot.transform,false,9);
            var actor=p.AddComponent<PassengerVisual>();actor.seat=p.transform.localPosition;actor.visual=p.GetComponent<Renderer>();p.SetActive(false);passengers.passengerPool[i]=p;
        }
        var effects=busRoot.AddComponent<VehicleEffects>();effects.bus=bus;effects.bodyRenderer=body.GetComponent<Renderer>();
        effects.exhaust=Particles("Exhaust",busRoot.transform,new Vector3(-.8f,.6f,-5.6f),20,new Color(.25f,.25f,.25f,.2f),false);
        effects.airPuff=Particles("Air brake puff",busRoot.transform,new Vector3(1,.4f,1),8,new Color(.8f,.8f,.8f,.3f),true);
        effects.airSound=Audio(busRoot,Noise("Air release",.35f,1),false);effects.grindSound=Audio(busRoot,Noise("Gear grind",.25f,2),false);
        effects.engineSound=Audio(busRoot,Noise("Diesel placeholder",1,3),true);bus.hornAudio=Audio(busRoot,Noise("Horn placeholder",.6f,4),false);
        var lampMaterial=Mat("Vehicle lamps",Color.white);lampMaterial.EnableKeyword("_EMISSION");
        effects.brakeLamps=new Renderer[2];effects.indicatorLeft=new Renderer[2];effects.indicatorRight=new Renderer[2];
        bus.lamps=new Light[2];effects.headlightCones=new GameObject[2];
        var coneMaterial=Mat("Headlight haze",new Color(1,.92f,.72f,.035f),"Universal Render Pipeline/Unlit");
        coneMaterial.SetFloat("_Surface",1);coneMaterial.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);coneMaterial.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);coneMaterial.SetFloat("_ZWrite",0);coneMaterial.SetFloat("_Cull",0);coneMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");coneMaterial.renderQueue=3000;
        var coneMesh=new Mesh{name="Fake headlight cone"};coneMesh.vertices=new[]{Vector3.zero,new Vector3(-4,-1,20),new Vector3(4,-1,20),new Vector3(4,3,20),new Vector3(-4,3,20)};
        coneMesh.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4};coneMesh.RecalculateNormals();coneMesh.RecalculateBounds();coneMesh=Asset(coneMesh,"HeadlightCone.asset");
        for(int i=0;i<2;i++) {
            effects.brakeLamps[i]=Shape("Brake lamp",PrimitiveType.Cube,new Vector3(i==0?-.9f:.9f,1,-5.56f),new Vector3(.4f,.2f,.08f),lampMaterial,busRoot.transform,false,9).GetComponent<Renderer>();
            effects.indicatorLeft[i]=Shape("Left signal",PrimitiveType.Cube,new Vector3(-1.27f,1.2f,i==0?5:-5),new Vector3(.1f,.2f,.3f),lampMaterial,busRoot.transform,false,9).GetComponent<Renderer>();
            effects.indicatorRight[i]=Shape("Right signal",PrimitiveType.Cube,new Vector3(1.27f,1.2f,i==0?5:-5),new Vector3(.1f,.2f,.3f),lampMaterial,busRoot.transform,false,9).GetComponent<Renderer>();
            var lamp=new GameObject("Headlight");lamp.transform.SetParent(busRoot.transform,false);lamp.transform.localPosition=new Vector3(i==0?-.8f:.8f,1,5.6f);
            var l=lamp.AddComponent<Light>();l.type=LightType.Spot;l.range=25;l.spotAngle=45;l.intensity=3;l.shadows=LightShadows.None;l.enabled=false;bus.lamps[i]=l;
            var cone=new GameObject("Fake beam haze");cone.transform.SetParent(lamp.transform,false);cone.AddComponent<MeshFilter>().sharedMesh=coneMesh;cone.AddComponent<MeshRenderer>().sharedMaterial=coneMaterial;cone.SetActive(false);effects.headlightCones[i]=cone;
        }
        effects.dust=new TrailRenderer[2];
        for(int i=0;i<2;i++) {var go=new GameObject("Wheel dust trail");go.transform.SetParent(busRoot.transform,false);go.transform.localPosition=new Vector3(i==0?-1.2f:1.2f,.1f,-4);
            var trail=go.AddComponent<TrailRenderer>();trail.sharedMaterial=roof;trail.time=.7f;trail.startWidth=.35f;trail.endWidth=.02f;trail.emitting=false;effects.dust[i]=trail;}
        Terminal("TERMINAL",30,false);Terminal("BANDUNG",1030,true);
        for(int z=200;z<900;z+=210) {var pothole=Shape("Pothole",PrimitiveType.Cube,new Vector3(-3,.03f,z),new Vector3(1.5f,.08f,2),black,null,true);pothole.GetComponent<BoxCollider>().isTrigger=true;pothole.AddComponent<Pothole>();}
        var cameraGO=new GameObject("Main Camera");cameraGO.tag="MainCamera";cameraGO.transform.position=busRoot.transform.position+new Vector3(0,4.5f,-9);
        var camera=cameraGO.AddComponent<Camera>();camera.nearClipPlane=.08f;camera.farClipPlane=260;camera.clearFlags=CameraClearFlags.SolidColor;
        cameraGO.AddComponent<AudioListener>();cameraGO.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;
        var rig=cameraGO.AddComponent<CameraRig>();rig.target=busRoot.transform;
        var volumeGO=new GameObject("Mobile post processing");var volume=volumeGO.AddComponent<Volume>();volume.isGlobal=true;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom=profile.Add<Bloom>();bloom.intensity.Override(.4f);bloom.threshold.Override(1.2f);
        var vignette=profile.Add<Vignette>();vignette.intensity.Override(0);
        profile.Add<ColorLookup>().contribution.Override(.35f);
        volume.sharedProfile=Asset(profile,"MobileVolume.asset");rig.volume=volume;
        // Volume components must be subassets to survive editor reload.
        foreach(var component in volume.sharedProfile.components)if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,volume.sharedProfile);
        var env=new GameObject("Environment").AddComponent<EnvironmentController>();env.cameraComponent=camera;env.sun=sun;env.road=road;env.grass=grass;env.volume=volume;
        env.biomeLuts=new[]{Lut(0),Lut(1),Lut(2)};
        env.rain=Particles("Rain",null,new Vector3(0,12,0),160,new Color(.65f,.75f,.85f,.25f),false);
        var rainMain=env.rain.main;rainMain.startSpeed=15;rainMain.startLifetime=.8f;rainMain.startSize=.04f;
        env.rain.transform.rotation=Quaternion.Euler(90,0,0);var rainShape=env.rain.shape;rainShape.shapeType=ParticleSystemShapeType.Box;rainShape.scale=new Vector3(20,20,.1f);env.rain.Stop();
        var career=new GameObject("Career").AddComponent<Career>();var commerce=career.gameObject.AddComponent<Commerce>();
        var liveryGO=new GameObject("Livery");var livery=liveryGO.AddComponent<LiveryManager>();livery.body=body.GetComponent<Renderer>();livery.career=career;
        var ui=new GameObject("Mobile UI").AddComponent<MobileUI>();ui.bus=bus;ui.passengers=passengers;ui.career=career;ui.commerce=commerce;ui.cameraRig=rig;ui.livery=livery;ui.environment=env;
        var fleet=career.gameObject.AddComponent<FleetManager>();fleet.bus=bus;fleet.career=career;fleet.passengers=passengers;ui.fleet=fleet;
        ui.engineUpgrade=engine;ui.brakeUpgrade=brakes;ui.suspensionUpgrade=suspension;
        var graph=new GameObject("RoadGraph").AddComponent<RoadGraph>();graph.segments=new RoadSegment[4];
        graph.segments[0]=new RoadSegment{a=new Vector3(-3,.15f,-80),b=new Vector3(-3,.15f,350),c=new Vector3(-3,.15f,850),d=new Vector3(-3,.15f,1250),next=1};
        graph.segments[1]=new RoadSegment{a=new Vector3(-3,.15f,1250),b=new Vector3(-3,.15f,1260),c=new Vector3(3,.15f,1260),d=new Vector3(3,.15f,1250),next=2};
        graph.segments[2]=new RoadSegment{a=new Vector3(3,.15f,1250),b=new Vector3(3,.15f,850),c=new Vector3(3,.15f,350),d=new Vector3(3,.15f,-80),next=3};
        graph.segments[3]=new RoadSegment{a=new Vector3(3,.15f,-80),b=new Vector3(3,.15f,-90),c=new Vector3(-3,.15f,-90),d=new Vector3(-3,.15f,-80),next=0};
        var signalGO=new GameObject("Traffic signal");var signal=signalGO.AddComponent<TrafficLightManager>();
        signal.lamp=Shape("Signal lamp",PrimitiveType.Sphere,new Vector3(-7,3,1190),Vector3.one*.6f,white).GetComponent<Renderer>();var segment=graph.segments[0];segment.light=signal;graph.segments[0]=segment;
        var spawner=new GameObject("Traffic pool").AddComponent<TrafficSpawner>();spawner.player=busRoot.transform;spawner.graph=graph;spawner.pool=new TrafficVehicle[20];
        for(int i=0;i<20;i++) {
            var npc=new GameObject("Pooled traffic "+i);npc.layer=10;npc.AddComponent<Rigidbody>();var vehicle=npc.AddComponent<TrafficVehicle>();vehicle.graph=graph;vehicle.obstacleMask=(1<<9)|(1<<10);vehicle.cruiseSpeed=9+i%5;
            Shape("NPC chassis",PrimitiveType.Cube,new Vector3(0,.9f,0),new Vector3(1.8f,1.5f,i%3==0?6:4),i%2==0?gold:white,npc.transform,true,10);
            Shape("NPC glass",PrimitiveType.Cube,new Vector3(0,1.7f,0),new Vector3(1.6f,.5f,2),glass,npc.transform,false,10);
            spawner.pool[i]=vehicle;npc.SetActive(false);
        }
        var streamer=new GameObject("Sector streamer").AddComponent<SectorStreamer>();streamer.player=busRoot.transform;streamer.sectors=new SectorReference[4];
        for(int i=0;i<4;i++)streamer.sectors[i]=new SectorReference{cell=new Vector2Int(0,i),address="sector_0_"+i};
        // Static occluder proxies are optional: these persistent visual masses match generated buildings.
        // They are not claimed to fix any engine-version-specific occlusion issue.
        for(int i=0;i<8;i++) {
            var proxy=Shape("Boot occluder building",PrimitiveType.Cube,new Vector3(30,6,50+i*140),new Vector3(18,12,22),concrete,null,true);
            GameObjectUtility.SetStaticEditorFlags(proxy,StaticEditorFlags.OccluderStatic|StaticEditorFlags.BatchingStatic);
        }
        EditorSceneManager.SaveScene(boot,BootPath);
        for(int i=0;i<4;i++)GenerateSector(i,boot);
        SceneManager.SetActiveScene(boot);EditorSceneManager.SaveScene(boot);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(BootPath,true)};
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Nusantara generated. Open Boot and press Play.");
    }
    static void Terminal(string title,float z,bool destination) {
        Shape(title+" bay",PrimitiveType.Cube,new Vector3(-11,.01f,z),new Vector3(10,.1f,25),grass);
        var go=new GameObject(title+" trigger");go.transform.position=new Vector3(-10,2,z);var trigger=go.AddComponent<BoxCollider>();trigger.size=new Vector3(12,5,25);go.AddComponent<TerminalStop>().destination=destination;
        var sign=new GameObject(title+" sign");sign.transform.position=new Vector3(-18,4,z);sign.transform.rotation=Quaternion.Euler(0,90,0);var text=sign.AddComponent<TextMesh>();text.text=title;text.characterSize=.4f;text.fontSize=60;text.color=Color.white;text.anchor=TextAnchor.MiddleCenter;
    }
    static void GenerateSector(int cell,Scene boot) {
        var sector=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(sector);
        for(int j=0;j<25;j++) {
            float z=cell*500+j*20;float x=j%2==0?-35:40;
            var variants=new GameObject("Biome variants").AddComponent<BiomeProp>();variants.variants=new GameObject[3];
            variants.variants[0]=Shape("Urban tower",PrimitiveType.Cube,new Vector3(x*1.8f,15,z),new Vector3(16,30,14),glass);
            variants.variants[1]=Shape("Rice paddy",PrimitiveType.Cube,new Vector3(x*1.6f,0,z),new Vector3(25,.1f,18),grass);
            variants.variants[2]=Shape("Toll sign",PrimitiveType.Cube,new Vector3(0,7,z),new Vector3(12,2,.2f),leaf);
            foreach(var variant in variants.variants)variant.transform.SetParent(variants.transform);
            var house=Shape("Sector prop",PrimitiveType.Cube,new Vector3(x,2.5f,z),new Vector3(8,5,7),concrete,null,true);
            Shape("Roof",PrimitiveType.Cube,new Vector3(x,5.1f,z),new Vector3(9,.5f,8),roof);
            GameObjectUtility.SetStaticEditorFlags(house,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic);
            var tree=new GameObject("Palm LOD");tree.transform.position=new Vector3(x*.6f,0,z+8);
            var trunk=Shape("Trunk",PrimitiveType.Cylinder,new Vector3(0,3,0),new Vector3(.45f,3,.45f),roof,tree.transform);
            var crown=Shape("Crown LOD0",PrimitiveType.Sphere,new Vector3(0,6,0),new Vector3(4,1,4),leaf,tree.transform);
            var mid=Shape("Crown LOD1",PrimitiveType.Cube,new Vector3(0,6,0),new Vector3(3.5f,.6f,3.5f),leaf,tree.transform);
            var lod=tree.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.15f,new[]{trunk.GetComponent<Renderer>(),crown.GetComponent<Renderer>()}),new LOD(.045f,new[]{mid.GetComponent<Renderer>()}),new LOD(.01f,Array.Empty<Renderer>())});lod.RecalculateBounds();
        }
        string path=Generated+"/Sector_0_"+cell+".unity";EditorSceneManager.SaveScene(sector,path);
        var settings=AddressableAssetSettingsDefaultObject.GetSettings(true);var entry=settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path),settings.DefaultGroup);entry.address="sector_0_"+cell;
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified,entry,true);
        EditorSceneManager.CloseScene(sector,true);SceneManager.SetActiveScene(boot);
    }
    static ParticleSystem Particles(string name,Transform parent,Vector3 position,int cap,Color color,bool burst) {
        var go=new GameObject(name);if(parent){go.transform.SetParent(parent,false);go.transform.localPosition=position;}else go.transform.position=position;
        var p=go.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=p.main;main.maxParticles=cap;main.startColor=color;main.startLifetime=.8f;main.startSize=.3f;main.startSpeed=.5f;main.loop=!burst;main.playOnAwake=!burst;
        var emission=p.emission;emission.rateOverTime=burst?0:cap/2;if(burst)emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)cap)});
        var material=Mat("Particle",new Color(1,1,1,.3f),"Universal Render Pipeline/Particles/Unlit");
        material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
        var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.enableGPUInstancing=true;
        return p;
    }
    static AudioSource Audio(GameObject go,AudioClip clip,bool loop) {var source=go.AddComponent<AudioSource>();source.clip=clip;source.loop=loop;source.playOnAwake=loop;source.volume=.3f;source.spatialBlend=.5f;source.maxDistance=30;return source;}
    static AudioClip Noise(string name,float length,int kind) {
        // WAV files are generated in the Editor; imported AudioClips serialize safely into builds.
        string path=Generated+"/"+name.Replace(' ','_')+".wav";
        if(!File.Exists(path)) {
            int rate=22050,count=(int)(length*rate);var random=new System.Random(kind);
            using(var writer=new BinaryWriter(File.Create(path))) {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                for(int i=0;i<count;i++){float t=i/(float)rate;float sample=kind==1?(float)(random.NextDouble()*2-1)*(1-i/(float)count):kind==4?Mathf.Sin(t*440*2*Mathf.PI)*.4f+Mathf.Sin(t*554*2*Mathf.PI)*.25f:Mathf.Sin(t*(kind==3?65:150)*2*Mathf.PI)*.35f+(float)(random.NextDouble()-.5)*.2f;writer.Write((short)(sample*10000));}
            }
            AssetDatabase.ImportAsset(path);
        }
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    static Texture2D PassengerAtlas() {
        string path=Generated+"/PassengerFlipbook.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture)return texture;
        texture=new Texture2D(256,64,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Point;
        for(int frame=0;frame<4;frame++)for(int y=0;y<64;y++)for(int x=0;x<64;x++) {
            float dx=x-32,dy=y-51;bool head=dx*dx+dy*dy<49;
            bool torso=x>23&&x<41&&y>22&&y<44;
            int step=frame%2==0?3:-3;bool legs=y>4&&y<=22&&((x>23+step&&x<29+step)||(x>35-step&&x<41-step));
            texture.SetPixel(frame*64+x,y,head?new Color(.75f,.53f,.36f):torso?new Color(.18f,.35f,.55f):legs?new Color(.15f,.15f,.19f):Color.clear);
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,path);return texture;
    }
    static Texture2D Lut(int biome) {
        const int n=32;string path=Generated+"/BiomeLUT"+biome+".asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(texture)return texture;
        texture=new Texture2D(n*n,n,TextureFormat.RGB24,false,true);texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;
        for(int b=0;b<n;b++)for(int g=0;g<n;g++)for(int r=0;r<n;r++) {
            var color=new Color(r/31f,g/31f,b/31f);if(biome==0)color=Color.Lerp(color,new Color(.8f,.75f,.65f),.06f);if(biome==1)color.g=Mathf.Min(1,color.g*1.04f);if(biome==2)color.r=Mathf.Min(1,color.r*1.06f);texture.SetPixel(b*n+r,g,color);
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,path);return texture;
    }
    static void Configure() {
        PlayerSettings.companyName="RawdyProgrammer";PlayerSettings.productName="Nusantara Heavy";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.rawdy.nusantaraheavy");
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.bundleVersion="0.2.0";
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
        PlayerSettings.stripEngineCode=true;PlayerSettings.Android.useCustomKeystore=false;
        var project=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=project.FindProperty("activeInputHandler");if(input!=null){input.intValue=0;project.ApplyModifiedPropertiesWithoutUndo();}
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");layers.GetArrayElementAtIndex(8).stringValue="Environment";layers.GetArrayElementAtIndex(9).stringValue="PlayerVehicle";layers.GetArrayElementAtIndex(10).stringValue="Traffic";tags.ApplyModifiedPropertiesWithoutUndo();
        var renderer=Asset(ScriptableObject.CreateInstance<UniversalRendererData>(),"MobileRenderer.asset");
        renderer.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
        var settingsType=typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineGlobalSettings");
        var ensure=settingsType?.GetMethod("Ensure",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        ensure?.Invoke(null,new object[]{true});
        EditorUtility.SetDirty(renderer);
        var pipeline=Asset(UniversalRenderPipelineAsset.Create(renderer),"MobileURP.asset");pipeline.renderScale=.8f;pipeline.msaaSampleCount=2;pipeline.shadowDistance=50;
        GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;QualitySettings.vSyncCount=0;
        Time.fixedDeltaTime=.02f;
    }
    [MenuItem("Nusantara/Build Android APK")]
    public static void BuildAndroid() {
        if(!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))throw new Exception("Android build support is not installed");
        Generate();
        AddressableAssetSettings.BuildPlayerContent(out var addressables);
        if(!string.IsNullOrEmpty(addressables.Error))throw new Exception(addressables.Error);
        Directory.CreateDirectory("Builds/Android");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{BootPath},locationPathName="Builds/Android/NusantaraHeavy.apk",target=BuildTarget.Android,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed: "+report.summary.result);
    }
}
}
