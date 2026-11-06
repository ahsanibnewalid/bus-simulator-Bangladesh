using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Nusantara {
public class HoldControl : MonoBehaviour, IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
    public Action<bool> Changed;
    Image image; bool held; int pointer;
    void Awake() { image=GetComponent<Image>(); }
    public void OnPointerDown(PointerEventData e) { if(held)return; pointer=e.pointerId; Set(true); }
    public void OnPointerUp(PointerEventData e) { if(held&&pointer==e.pointerId)Set(false); }
    public void OnPointerExit(PointerEventData e) { if(held&&pointer==e.pointerId)Set(false); }
    void Set(bool value) { held=value; transform.localScale=value?Vector3.one*.95f:Vector3.one; if(image)image.color=value?new Color(.72f,.11f,.11f):new Color(.83f,.18f,.18f); Changed?.Invoke(value); }
    void OnDisable() { if(held)Set(false); }
}
}
