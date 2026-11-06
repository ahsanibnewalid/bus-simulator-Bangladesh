using System;
using UnityEngine;
namespace Nusantara {
[Serializable] public struct RoadSegment {
    public Vector3 a,b,c,d;
    public int next;
    public TrafficLightManager light;
    public Vector3 Point(float t) { t=Mathf.Clamp01(t); float u=1-t; return u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d; }
    public Vector3 Tangent(float t) { t=Mathf.Clamp01(t); float u=1-t; return (3*u*u*(b-a)+6*u*t*(c-b)+3*t*t*(d-c)).normalized; }
    public float ApproxLength => Vector3.Distance(a,b)+Vector3.Distance(b,c)+Vector3.Distance(c,d);
}
public class RoadGraph : MonoBehaviour {
    public RoadSegment[] segments;
    public Vector3 Point(int segment,float t) => transform.TransformPoint(segments[segment].Point(t));
    public Vector3 Direction(int segment,float t) => transform.TransformDirection(segments[segment].Tangent(t));
}
}
