using UnityEngine;
namespace Nusantara {
[RequireComponent(typeof(BoxCollider))]
public class TerminalStop : MonoBehaviour {
    public bool destination;
    void Awake() { GetComponent<BoxCollider>().isTrigger=true; }
    void OnTriggerStay(Collider other) {
        var bus=other.GetComponentInParent<BusController>();
        if(bus) { var manager=bus.GetComponent<PassengerManager>(); if(manager) manager.AtStop(destination); }
    }
}
}
