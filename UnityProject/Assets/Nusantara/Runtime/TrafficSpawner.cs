using UnityEngine;
namespace Nusantara {
public class TrafficSpawner : MonoBehaviour {
    public Transform player;
    public TrafficVehicle[] pool;
    public RoadGraph graph;
    float timer;
    void Start() { for(int i=0;i<pool.Length;i++) { pool[i].graph=graph; pool[i].Place(i%2==0?0:2,(i+1f)/(pool.Length+1)); pool[i].gameObject.SetActive(true); } }
    void Update() {
        timer+=Time.deltaTime; if(timer<1)return; timer=0;
        foreach(var vehicle in pool) {
            if((vehicle.transform.position-player.position).sqrMagnitude<=150*150)continue;
            // Search a safe road point ahead, outside the camera's immediate vicinity.
            for(int attempt=0;attempt<12;attempt++) {
                int s=Random.value<.5f?0:2; float t=Random.value;
                Vector3 candidate=graph.Point(s,t), delta=candidate-player.position;
                if(delta.sqrMagnitude<80*80||delta.sqrMagnitude>145*145||Vector3.Dot(player.forward,delta)<0)continue;
                if(Physics.CheckSphere(candidate+Vector3.up,3,(1<<9)|(1<<10),QueryTriggerInteraction.Ignore))continue;
                vehicle.Place(s,t); break;
            }
        }
    }
}
}
