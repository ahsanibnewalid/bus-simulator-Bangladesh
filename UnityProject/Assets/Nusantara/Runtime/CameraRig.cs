using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Nusantara {
public class CameraRig : MonoBehaviour {
    public Transform target;
    public int Mode {get;private set;}
    public bool garage,cinematic;
    float garageRadius=8;
    public void RotateGarage() { orbit+=45; }
    public void ZoomGarage() { garageRadius=garageRadius>=12?6:garageRadius+2; }
    public Volume volume;
    Vector3 velocity; float orbit;
    Camera cameraComponent;
    void Awake() { cameraComponent=GetComponent<Camera>(); }
    public void Cycle() { Mode=(Mode+1)%3; velocity=Vector3.zero; }
    void LateUpdate() {
        if(!target)return;
        Vector3 offset=Mode==0?new Vector3(0,4.5f,-9):Mode==1?new Vector3(.65f,2.1f,3.8f):new Vector3(0,1.2f,5.9f);
        if(garage) { orbit+=Time.unscaledDeltaTime*12; offset=Quaternion.Euler(0,orbit,0)*new Vector3(0,4,-garageRadius); }
        Vector3 desired=target.TransformPoint(offset);
        if(Mode==0||garage) {
            // Spherecast prevents chase camera moving through scenery.
            Vector3 look=target.position+Vector3.up*2;
            Vector3 direction=desired-look;
            if(Physics.SphereCast(look,.2f,direction.normalized,out var hit,direction.magnitude,1<<8,QueryTriggerInteraction.Ignore))desired=hit.point+hit.normal*.3f;
            transform.position=Vector3.SmoothDamp(transform.position,desired,ref velocity,.15f,Mathf.Infinity,Time.unscaledDeltaTime);
            transform.LookAt(look);
        } else { transform.SetPositionAndRotation(desired,target.rotation); }
        cameraComponent.fieldOfView=garage?(cinematic?50:55):Mode==0?65:Mode==1?75:80;
        if(volume&&volume.profile&&volume.profile.TryGet<Vignette>(out var vignette))vignette.intensity.value=!garage&&Mode==1? .25f:0;
    }
}
}
