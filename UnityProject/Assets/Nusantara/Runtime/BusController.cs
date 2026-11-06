using UnityEngine;
namespace Nusantara {
[RequireComponent(typeof(Rigidbody))]
public class BusController : MonoBehaviour {
    public WheelCollider[] wheels;
    public Transform[] wheelVisuals;
    public bool automatic=true, doorOpen, signalLeft, signalRight, headlights, retarder;
    public float throttle, brake, steer, clutch;
    public float engineTorque=1800, maximumBrake=28000, suspensionScale=1;
    public AudioSource hornAudio;
    public Light[] lamps;
    public Rigidbody Body { get; private set; }
    public float Speed => Body==null?0:Body.linearVelocity.magnitude;
    public float Rpm { get; private set; }=700;
    public int Gear { get; private set; }=1;
    public float AirPressure { get; private set; }=1;
    public float Health { get; private set; }=100;
    public float Distance { get; private set; }
    public float BrakeTorque => brakeTorque;
    readonly float[] ratios={0,4.8f,3.1f,2.1f,1.5f,1.05f,.78f};
    float brakeTorque, brakeVelocity, shiftTimer, telemetryTimer, steerSmoothed, hornCooldown;
    bool wasBraking;
    void Awake() {
        Body=GetComponent<Rigidbody>(); Body.mass=12000;
        Body.centerOfMass=new Vector3(0,-.5f,-.7f);
        Body.angularDamping=.25f; Body.linearDamping=.03f;
        Body.interpolation=RigidbodyInterpolation.Interpolate;
        Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
    }
    void FixedUpdate() {
        float dt=Time.fixedDeltaTime;
        hornCooldown-=dt; shiftTimer-=dt;
        float pedal=doorOpen||Health<=0?0:Mathf.Clamp01(throttle);
        float braking=doorOpen||Health<=0?1:Mathf.Clamp01(brake);
        brakeTorque=Mathf.SmoothDamp(brakeTorque,braking*maximumBrake,ref brakeVelocity,.4f,Mathf.Infinity,dt);
        bool applied=braking>.05f;
        if(wasBraking&&!applied) { AirPressure=Mathf.Max(.2f,AirPressure-.08f); GameEvents.ReleaseAir(); }
        wasBraking=applied;
        AirPressure=Mathf.MoveTowards(AirPressure,1,dt*.025f);
        float rpm=0; int driven=0;
        for(int i=2;i<wheels.Length;i++) { rpm+=Mathf.Abs(wheels[i].rpm); driven++; }
        Rpm=Mathf.Clamp(driven==0?700:rpm/driven*ratios[Gear]*4.1f,700,2600);
        if(automatic&&shiftTimer<=0) {
            if(Rpm>1800&&Gear<6) Shift(1);
            else if(Rpm<1200&&Gear>1) Shift(-1);
        }
        steerSmoothed=Mathf.MoveTowards(steerSmoothed,Mathf.Clamp(steer,-1,1),dt*1.8f);
        float angle=steerSmoothed*Mathf.Lerp(34,9,Mathf.Clamp01(Speed/28));
        float torque=shiftTimer>0?0:engineTorque*ratios[Gear]*4.1f*pedal*(automatic?1:1-Mathf.Clamp01(clutch));
        float retarding=retarder&&pedal<.05f?maximumBrake*.16f:0;
        for(int i=0;i<wheels.Length;i++) {
            wheels[i].steerAngle=i<2?angle:0;
            wheels[i].motorTorque=i>=2?torque/Mathf.Max(1,wheels.Length-2):0;
            wheels[i].brakeTorque=brakeTorque*AirPressure+(i>=2?retarding:0);
        }
        float yaw=Vector3.Dot(Body.angularVelocity,transform.up);
        Body.AddTorque(-transform.up*yaw*18000f,ForceMode.Force);
        Distance+=Speed*dt;
        telemetryTimer+=dt;
        if(telemetryTimer>=.1f) { telemetryTimer=0; GameEvents.PublishTelemetry(Speed*3.6f,Rpm,Gear); }
    }
    void LateUpdate() {
        if(wheelVisuals!=null) for(int i=0;i<Mathf.Min(wheels.Length,wheelVisuals.Length);i++) {
            wheels[i].GetWorldPose(out var p,out var r); wheelVisuals[i].SetPositionAndRotation(p,r*Quaternion.Euler(0,0,90));
        }
    }
    public void Shift(int direction) {
        if(shiftTimer>0) return;
        if(!automatic&&clutch<.8f) { GameEvents.Grind(); shiftTimer=.5f; return; }
        Gear=Mathf.Clamp(Gear+direction,1,6); shiftTimer=.5f;
    }
    public void ToggleDoor() { if(Speed<.5f) doorOpen=!doorOpen; }
    public void Horn() { if(hornCooldown>0) return; hornCooldown=.5f; if(hornAudio&&hornAudio.clip) hornAudio.Play(); GameEvents.SoundHorn(transform.position); }
    public void ToggleLights() { headlights=!headlights; if(lamps!=null) foreach(var light in lamps) if(light) light.enabled=headlights; }
    public void ReleaseControls() { throttle=steer=0; brake=1; clutch=0; }
    public void Repair() { Health=100; }
    public void ApplyUpgrade(BusUpgradeSO u) {
        engineTorque*=u.torqueMultiplier; maximumBrake*=u.brakeMultiplier; suspensionScale*=u.suspensionMultiplier;
        foreach(var wheel in wheels) { var spring=wheel.suspensionSpring; spring.spring*=u.suspensionMultiplier; wheel.suspensionSpring=spring; }
    }
    void OnCollisionEnter(Collision c) { float severity=c.relativeVelocity.magnitude; if(severity>1) { Health=Mathf.Max(0,Health-severity*2); GameEvents.Hit(severity); } }
    void OnApplicationFocus(bool focus) { if(!focus) ReleaseControls(); }
    void OnDisable() { ReleaseControls(); }
}
}
