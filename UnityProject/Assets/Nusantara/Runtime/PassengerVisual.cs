using UnityEngine;
namespace Nusantara {
public class PassengerVisual : MonoBehaviour {
    public Vector3 seat;
    public Renderer visual;
    bool walking;
    float frame;
    MaterialPropertyBlock block;
    void Awake() {block=new MaterialPropertyBlock();}
    public void BeginBoarding() {transform.localPosition=new Vector3(1.7f,1.2f,3.5f);walking=true;frame=0;}
    void Update() {
        if(!walking)return;
        transform.localPosition=Vector3.MoveTowards(transform.localPosition,seat,Time.deltaTime*2.5f);
        frame+=Time.deltaTime*8;
        visual.GetPropertyBlock(block);block.SetVector("_BaseMap_ST",new Vector4(.25f,1,Mathf.FloorToInt(frame)%4*.25f,0));visual.SetPropertyBlock(block);
        if(Vector3.Distance(transform.localPosition,seat)<.02f)walking=false;
    }
}
}
