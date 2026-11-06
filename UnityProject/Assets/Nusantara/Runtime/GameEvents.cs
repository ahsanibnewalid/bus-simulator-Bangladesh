using System;
using UnityEngine;
namespace Nusantara {
public static class GameEvents {
    public static event Action<float,float,int> Telemetry;
    public static event Action<Vector3> Horn;
    public static event Action AirRelease, GearGrind;
    public static event Action<float> Impact;
    public static event Action<int,float,long> TripFinished;
    public static void PublishTelemetry(float speed,float rpm,int gear) => Telemetry?.Invoke(speed,rpm,gear);
    public static void SoundHorn(Vector3 point) => Horn?.Invoke(point);
    public static void ReleaseAir() => AirRelease?.Invoke();
    public static void Grind() => GearGrind?.Invoke();
    public static void Hit(float severity) => Impact?.Invoke(severity);
    public static void Complete(int passengers,float comfort,long fare) => TripFinished?.Invoke(passengers,comfort,fare);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() { Telemetry=null; Horn=null; AirRelease=null; GearGrind=null; Impact=null; TripFinished=null; }
}
public static class EconomyRules {
    public static long Payout(int count,long ticket,float comfort) {
        if(count<=0||ticket<=0||comfort<40||float.IsNaN(comfort)) return 0;
        return (long)Math.Floor(count*(double)ticket*Math.Min(100,comfort)/100d);
    }
    public static float ComfortDelta(float longitudinalG,float lateralG,float kmh,bool signal) {
        float penalty=Math.Max(0,-longitudinalG-.4f)*12+Math.Max(0,Math.Abs(lateralG)-.25f)*10+Math.Max(0,kmh-80)*.035f;
        return penalty>0 ? -penalty : .15f+(signal&&Math.Abs(lateralG)>.04f? .1f:0);
    }
}
}
