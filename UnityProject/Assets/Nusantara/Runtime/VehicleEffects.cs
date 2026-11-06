using UnityEngine;
namespace Nusantara {
public class VehicleEffects : MonoBehaviour {
    public BusController bus;
    public GameObject[] headlightCones;
    public ParticleSystem exhaust,airPuff;
    public TrailRenderer[] dust;
    public AudioSource airSound,grindSound,engineSound;
    public Renderer bodyRenderer;
    public Renderer[] brakeLamps,indicatorLeft,indicatorRight;
    MaterialPropertyBlock block;
    void Awake() {block=new MaterialPropertyBlock();}
    void OnEnable() {GameEvents.AirRelease+=Release;GameEvents.GearGrind+=Grind;}
    void OnDisable() {GameEvents.AirRelease-=Release;GameEvents.GearGrind-=Grind;}
    void Release() {if(airPuff)airPuff.Play();if(airSound&&airSound.clip)airSound.Play();}
    void Grind() {if(grindSound&&grindSound.clip)grindSound.Play();}
    void Update() {
        if(headlightCones!=null)foreach(var cone in headlightCones)if(cone)cone.SetActive(bus.headlights);
        if(exhaust) {var emission=exhaust.emission;emission.enabled=bus.throttle>.1f&&bus.Speed>1;}
        bool dirt=Physics.Raycast(transform.position+Vector3.up,Vector3.down,out var hit,4,1<<8)&&hit.collider.CompareTag("Respawn");
        if(dust!=null)foreach(var trail in dust)if(trail)trail.emitting=dirt&&bus.Speed>1.389f;
        if(engineSound&&engineSound.clip){engineSound.pitch=Mathf.Lerp(.7f,1.7f,bus.Rpm/2600);engineSound.volume=.1f+bus.throttle*.25f;}
        if(bodyRenderer) {bodyRenderer.GetPropertyBlock(block);block.SetFloat("_DirtAmount",Mathf.Min(.6f,bus.Distance/20000));bodyRenderer.SetPropertyBlock(block);}
        Lamp(brakeLamps,bus.brake>.1f?Color.red:new Color(.15f,0,0));
        bool flash=Time.time%1<.5f;
        Lamp(indicatorLeft,bus.signalLeft&&flash?new Color(1,.6f,0):new Color(.12f,.06f,0));
        Lamp(indicatorRight,bus.signalRight&&flash?new Color(1,.6f,0):new Color(.12f,.06f,0));
    }
    void Lamp(Renderer[] lamps,Color color) {if(lamps==null)return;foreach(var lamp in lamps){if(!lamp)continue;lamp.GetPropertyBlock(block);block.SetColor("_BaseColor",color);block.SetColor("_EmissionColor",color);lamp.SetPropertyBlock(block);}}
}
}
