using UnityEngine;
namespace Nusantara {
public class Pothole : MonoBehaviour {
    float until;
    void OnTriggerEnter(Collider other) {
        var bus=other.GetComponentInParent<BusController>();if(!bus||Time.time<until)return;until=Time.time+1;
        bus.Body.AddForceAtPosition(Vector3.up*bus.Speed*180,bus.transform.position+bus.transform.right,ForceMode.Impulse);
        var p=bus.GetComponent<PassengerManager>();if(p)p.Pothole(bus.Speed*.12f);
    }
}
}
