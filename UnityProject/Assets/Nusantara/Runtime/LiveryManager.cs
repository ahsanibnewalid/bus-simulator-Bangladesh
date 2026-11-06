using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
namespace Nusantara {
[UnityEngine.Scripting.Preserve]
public class LiveryManager : MonoBehaviour {
    public Renderer body;
    public Career career;
    public string Status {get;private set;}="Choose a PNG/JPG (max 4 MB; power-of-two, max 2048 px).";
    Texture2D current;
    MaterialPropertyBlock block;
    bool busy, dead;
    void Awake() { block=new MaterialPropertyBlock(); }
    async void Start() { SetColor(career.Data.busColor); if(!string.IsNullOrEmpty(career.Data.liveryPath))await Import(career.Data.liveryPath,false); }
    public void SetColor(Color color) { body.GetPropertyBlock(block); block.SetColor("_BaseColor",color); body.SetPropertyBlock(block); career.Data.busColor=color; career.Save(); }
    public void PickGallery() {
#if UNITY_ANDROID && !UNITY_EDITOR
        using(var bridge=new AndroidJavaClass("com.rawdy.nusantara.GalleryBridge"))bridge.CallStatic("pick",gameObject.name);
#else
        Status="In the Editor, enter an image path and press IMPORT.";
#endif
    }
    [UnityEngine.Scripting.Preserve]
    public async void OnGalleryPicked(string path) { await Import(path); }
    [UnityEngine.Scripting.Preserve]
    public void OnGalleryError(string message) { Status=message; }
    public async Task<bool> Import(string path,bool copy=true) {
        if(busy||dead)return false; busy=true;
        Texture2D decoded=null, output=null;
        try {
            // Only file IO runs on the worker. Unity texture APIs stay on the captured main context.
            byte[] bytes=await Task.Run(()=> {
                var file=new FileInfo(path); if(!file.Exists||file.Length<=0||file.Length>=4*1024*1024)throw new IOException("Image must be smaller than 4 MB.");
                return File.ReadAllBytes(path);
            });
            if(dead)return false;
            ReadDimensions(bytes,out int w,out int h);
            if(w<32||h<32||w>2048||h>2048||!Mathf.IsPowerOfTwo(w)||!Mathf.IsPowerOfTwo(h))throw new IOException("Use power-of-two dimensions from 32 to 2048.");
            decoded=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!ImageConversion.LoadImage(decoded,bytes,false))throw new IOException("Image could not be decoded.");
            var rt=RenderTexture.GetTemporary(1024,1024,0,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try { Graphics.Blit(decoded,rt); RenderTexture.active=rt; output=new Texture2D(1024,1024,TextureFormat.RGBA32,true); output.ReadPixels(new Rect(0,0,1024,1024),0,0); output.Apply(true,false); }
            finally { RenderTexture.active=previous; RenderTexture.ReleaseTemporary(rt); }
            string saved=SlotPath();
            if(copy)File.WriteAllBytes(saved,output.EncodeToPNG()); else saved=path;
            output.Apply(true,true);
            body.GetPropertyBlock(block); block.SetTexture("_DecalTex",output); block.SetFloat("_DecalEnabled",1); body.SetPropertyBlock(block);
            if(current)Destroy(current); current=output; output=null;
            career.Data.liveryPath=saved; career.Save(); Status="Livery applied"; return true;
        } catch(Exception e) { if(!dead)Status=e.Message; return false; }
        finally { if(decoded)Destroy(decoded); if(output)Destroy(output); busy=false; }
    }
    public static void ReadDimensions(byte[] b,out int width,out int height) {
        width=height=0;
        if(b.Length>=24&&b[0]==137&&b[1]==80&&b[2]==78&&b[3]==71) {
            width=(b[16]<<24)|(b[17]<<16)|(b[18]<<8)|b[19]; height=(b[20]<<24)|(b[21]<<16)|(b[22]<<8)|b[23];return;
        }
        if(b.Length>4&&b[0]==255&&b[1]==216) {
            int i=2;
            while(i+8<b.Length) {
                if(b[i++]!=255)continue;
                int marker=b[i++]; if(marker==255){i--;continue;}
                if(marker==217||marker==218)break;
                if(marker==216||marker==1||(marker>=208&&marker<=215))continue;
                int length=(b[i]<<8)|b[i+1]; if(length<2||i+length>b.Length)break;
                if((marker>=192&&marker<=195)||(marker>=197&&marker<=199)||(marker>=201&&marker<=203)||(marker>=205&&marker<=207)) {
                    height=(b[i+3]<<8)|b[i+4]; width=(b[i+5]<<8)|b[i+6]; return;
                }
                i+=length;
            }
        }
        throw new IOException("Only valid PNG and JPEG images are supported.");
    }
    string SlotPath() => Path.Combine(Application.persistentDataPath,"livery_"+career.Data.selectedLiverySlot+".png");
    public async void NextSlot() {
        if(busy)return;
        career.Data.selectedLiverySlot=(career.Data.selectedLiverySlot+1)%Mathf.Max(1,career.Data.liverySlots);
        string path=SlotPath();
        if(File.Exists(path))await Import(path,false);else Clear(false);
        career.Save();Status="Livery slot "+(career.Data.selectedLiverySlot+1)+" / "+career.Data.liverySlots;
    }
    public void Clear() {Clear(true);}
    void Clear(bool removeFile) { body.GetPropertyBlock(block); block.SetFloat("_DecalEnabled",0); block.SetTexture("_DecalTex",Texture2D.whiteTexture); body.SetPropertyBlock(block); if(current)Destroy(current); current=null; if(removeFile&&File.Exists(SlotPath()))File.Delete(SlotPath()); career.Data.liveryPath=""; career.Save(); }
    void OnDestroy() { dead=true; if(current)Destroy(current); }
}
}
