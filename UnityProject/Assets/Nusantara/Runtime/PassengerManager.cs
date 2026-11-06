using System.Collections;
using UnityEngine;
namespace Nusantara {
public class PassengerManager : MonoBehaviour {
    public BusController bus;
    public GameObject[] passengerPool;
    public int capacity=45;
    public long ticketPrice=18000;
    public int Count {get;private set;}
    public float Comfort {get;private set;}=100;
    public bool TripActive {get;private set;}
    public int Incidents {get;private set;}
    public bool Completed {get;private set;}
    public string Status {get;private set;}="Stop at TERMINAL and open door";
    Vector3 previousVelocity; float sampleTime; bool busy;
    void OnEnable() { GameEvents.Impact+=Impact; }
    void OnDisable() { GameEvents.Impact-=Impact; }
    void Start() { previousVelocity=bus.Body.linearVelocity; foreach(var p in passengerPool) if(p)p.SetActive(false); }
    void FixedUpdate() {
        Vector3 delta=bus.Body.linearVelocity-previousVelocity; previousVelocity=bus.Body.linearVelocity;
        if(!TripActive) return;
        Vector3 g=bus.transform.InverseTransformDirection(delta/Time.fixedDeltaTime)/9.81f;
        sampleTime+=Time.fixedDeltaTime;
        // Integrate every physics frame so brief harsh events cannot disappear between UI samples.
        float change=EconomyRules.ComfortDelta(g.z,g.x,bus.Speed*3.6f,bus.signalLeft||bus.signalRight);
        Comfort=Mathf.Clamp(Comfort+change*Time.fixedDeltaTime,0,100);
        if(sampleTime>=.5f) { sampleTime=0; if(g.z<-.4f||Mathf.Abs(g.x)>.25f) Incidents++; }
    }
    void Impact(float severity) { if(TripActive) { Comfort=Mathf.Max(0,Comfort-severity*1.5f); Incidents++; } }
    public void Pothole(float severity) { Impact(severity); }
    public void AtStop(bool destination) {
        if(busy||bus.Speed>=.5f||!bus.doorOpen) return;
        if(destination&&TripActive) StartCoroutine(DropOff());
        else if(!destination&&!TripActive&&!Completed) StartCoroutine(Board());
    }
    IEnumerator Board() {
        busy=true; Status="Boarding / automatic ticketing";
        int target=Mathf.Min(capacity,passengerPool.Length);
        for(int i=Count;i<target;i++) {
            if(!bus.doorOpen||bus.Speed>=.5f) { busy=false; Status="Boarding interrupted"; yield break; }
            passengerPool[i].SetActive(true); var actor=passengerPool[i].GetComponent<PassengerVisual>(); if(actor)actor.BeginBoarding(); Count++; yield return new WaitForSeconds(.12f);
        }
        Comfort=100; TripActive=true; busy=false; Status="Drive to BANDUNG / open door to finish";
    }
    IEnumerator DropOff() {
        busy=true; Status="Dropping off passengers";
        int paying=Count;
        for(int i=Count-1;i>=0;i--) {
            // Door-open interlock in BusController prevents acceleration throughout unloading.
            if(!bus.doorOpen||bus.Speed>=.5f) { busy=false; Status="Drop-off interrupted"; yield break; }
            passengerPool[i].SetActive(false); yield return new WaitForSeconds(.08f);
        }
        Count=0; TripActive=false; Completed=true; busy=false;
        long fare=EconomyRules.Payout(paying,ticketPrice,Comfort);
        Status="Trip complete"; GameEvents.Complete(paying,Comfort,fare);
    }
}
}
