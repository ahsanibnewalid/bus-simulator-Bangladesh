using UnityEngine;
namespace Nusantara {
public class TrafficLightManager : MonoBehaviour {
    public float greenSeconds=16, redSeconds=10, offset;
    public bool Green => (Time.time+offset)%(greenSeconds+redSeconds)<greenSeconds;
    public Renderer lamp;
    readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
    void Update() { if(!lamp)return; block.SetColor("_BaseColor",Green?Color.green:Color.red); lamp.SetPropertyBlock(block); }
}
}
