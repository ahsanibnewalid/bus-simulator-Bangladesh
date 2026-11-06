using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Nusantara {
public class EnvironmentController : MonoBehaviour {
    public Camera cameraComponent;
    public Light sun;
    public Material road,grass;
    public ParticleSystem rain;
    public Volume volume;
    public Texture2D[] biomeLuts;
    public static int ActiveBiome;
    public static event System.Action<int> BiomeChanged;
    public int biome;
    public bool night;
    bool low;
    public void CycleBiome() { biome=(biome+1)%3;Apply(); }
    public void ToggleNight() { night=!night;Apply(); }
    public void ToggleRain() { if(rain.isPlaying)rain.Stop();else rain.Play(); }
    public void ToggleQuality() { low=!low;Apply(); }
    void Start() { Application.targetFrameRate=30; QualitySettings.vSyncCount=0;Time.fixedDeltaTime=.02f;Apply(); }
    void Apply() {
        ActiveBiome=biome;BiomeChanged?.Invoke(biome);
        Color[] sky={new Color(.72f,.77f,.84f),new Color(.55f,.83f,.91f),new Color(.95f,.65f,.35f)};
        cameraComponent.backgroundColor=night?new Color(.025f,.04f,.08f):sky[biome];
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=90;RenderSettings.fogEndDistance=240;RenderSettings.fogColor=cameraComponent.backgroundColor;
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=night?new Color(.13f,.17f,.25f):new Color(.5f,.55f,.6f);
        sun.intensity=night? .12f:1.1f;sun.transform.rotation=Quaternion.Euler(night?8:45,biome*35+20,0);
        road.color=biome==2?new Color(.17f,.17f,.18f):new Color(.227f,.227f,.235f);
        grass.color=biome==1?new Color(.49f,.70f,.26f):new Color(.18f,.36f,.23f);
        var pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if(pipeline) { pipeline.renderScale=low? .7f:1;pipeline.shadowDistance=low?35:70;pipeline.msaaSampleCount=low?1:2; }
        if(volume&&volume.profile) {
            if(volume.profile.TryGet<Bloom>(out var bloom))bloom.active=!low;
            if(volume.profile.TryGet<ColorLookup>(out var lookup)) {lookup.texture.overrideState=true;lookup.texture.value=biomeLuts!=null&&biome<biomeLuts.Length?biomeLuts[biome]:null;lookup.contribution.value=lookup.texture.value != null ? .35f : 0;}
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEvents() {ActiveBiome=0;BiomeChanged=null;}
    void LateUpdate() { if(rain)rain.transform.position=cameraComponent.transform.position+Vector3.up*8; }
}
}
