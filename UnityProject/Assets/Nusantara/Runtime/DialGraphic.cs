using UnityEngine;
using UnityEngine.UI;
namespace Nusantara {
public class DialGraphic : Graphic {
    public float value,maximum=120;
    public void SetValue(float next) { value=next; SetVerticesDirty(); }
    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear(); Vector2 center=rectTransform.rect.center; float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.46f;
        for(int i=0;i<=24;i++) {
            float angle=(210-i*10)*Mathf.Deg2Rad;
            Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)), side=new Vector2(-dir.y,dir.x)*1.5f;
            Quad(vh,center+dir*radius+side,center+dir*radius-side,center+dir*(radius-(i%4==0?20:9))-side,center+dir*(radius-(i%4==0?20:9))+side,new Color(.75f,.75f,.75f));
        }
        float a=(210-Mathf.Clamp01(value/maximum)*240)*Mathf.Deg2Rad;
        Vector2 needle=new Vector2(Mathf.Cos(a),Mathf.Sin(a)), cross=new Vector2(-needle.y,needle.x)*4;
        Quad(vh,center+cross,center-cross,center+needle*radius*.78f-cross*.2f,center+needle*radius*.78f+cross*.2f,new Color(.96f,.26f,.21f));
    }
    static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint) {
        int n=vh.currentVertCount; vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddVert(d,tint,Vector2.zero);
        vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
}
}
