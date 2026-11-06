using UnityEngine;
namespace Nusantara {
[RequireComponent(typeof(Rigidbody))]
public class TrafficVehicle : MonoBehaviour {
    public RoadGraph graph;
    public int segment;
    public float progress, cruiseSpeed=10;
    public LayerMask obstacleMask;
    public float Speed {get;private set;}
    Rigidbody body;
    float panicUntil,yieldUntil;
    readonly RaycastHit[] hits=new RaycastHit[8];
    void Awake() { body=GetComponent<Rigidbody>(); body.isKinematic=true; body.interpolation=RigidbodyInterpolation.Interpolate; }
    void OnEnable() { GameEvents.Horn+=OnHorn; }
    void OnDisable() { GameEvents.Horn-=OnHorn; }
    void OnHorn(Vector3 point) {
        if((point-transform.position).sqrMagnitude>25)return;
        if(Random.value<.3f)panicUntil=Time.time+2; else yieldUntil=Time.time+3;
    }
    public void Place(int index,float t) {
        segment=index; progress=t; Speed=0;
        Vector3 position=graph.Point(segment,progress);
        transform.SetPositionAndRotation(position,Quaternion.LookRotation(graph.Direction(segment,progress)));
        if(body) { body.position=position; body.rotation=transform.rotation; }
    }
    void FixedUpdate() {
        if(!graph||graph.segments==null||graph.segments.Length==0)return;
        var road=graph.segments[segment];
        bool blocked=Time.time<panicUntil||(road.light&&!road.light.Green&&progress>.88f);
        int count=Physics.SphereCastNonAlloc(transform.position+transform.forward*3+Vector3.up,.8f,transform.forward,hits,4+Speed*.7f,obstacleMask,QueryTriggerInteraction.Ignore);
        for(int i=0;i<count;i++)if(hits[i].rigidbody!=body)blocked=true;
        float target=blocked?0:Time.time<yieldUntil?cruiseSpeed*.35f:cruiseSpeed;
        Speed=Mathf.MoveTowards(Speed,target,Time.fixedDeltaTime*(blocked?8:2));
        progress+=Speed*Time.fixedDeltaTime/Mathf.Max(1,road.ApproxLength);
        if(progress>=1) { progress-=1; segment=Mathf.Clamp(road.next,0,graph.segments.Length-1); }
        Vector3 point=graph.Point(segment,progress);
        if(Time.time<yieldUntil) point+=Vector3.Cross(Vector3.up,graph.Direction(segment,progress))*.5f;
        body.MovePosition(point); body.MoveRotation(Quaternion.LookRotation(graph.Direction(segment,progress)));
    }
}
}
