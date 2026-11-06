using UnityEngine;
namespace Nusantara {
public class FleetManager : MonoBehaviour {
    public BusController bus;
    public Career career;
    public PassengerManager passengers;
    public int Selected {get;private set;}
    public string ModelName => new[]{"Antarkota 12T","Touring 13T","Executive 14T"}[Selected];
    public void Initialize() { Apply(Mathf.Clamp(career.Data.selectedFleet,0,Mathf.Clamp(career.Data.fleetOwned,1,3)-1)); }
    public bool Next() {
        if(passengers.TripActive||passengers.Count>0)return false;
        Apply((Selected+1)%Mathf.Clamp(career.Data.fleetOwned,1,3)); career.Data.selectedFleet=Selected; career.Save(); return true;
    }
    void Apply(int index) {
        Selected=index;
        float[] masses={12000,13000,14000};float[] torques={1800,2100,2400};
        bus.Body.mass=masses[Selected];bus.engineTorque=torques[Selected]*Mathf.Pow(1.15f,career.Data.engineLevel);
    }
}
}
