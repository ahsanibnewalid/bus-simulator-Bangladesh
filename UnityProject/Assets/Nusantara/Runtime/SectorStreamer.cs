using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
namespace Nusantara {
[System.Serializable] public class SectorReference { public Vector2Int cell; public string address; }
public class SectorStreamer : MonoBehaviour {
    public Transform player;
    public SectorReference[] sectors;
    public float sectorSize=500;
    public int loadRadius=1, unloadRadius=2;
    readonly Dictionary<Vector2Int,AsyncOperationHandle<SceneInstance>> loaded=new Dictionary<Vector2Int,AsyncOperationHandle<SceneInstance>>();
    readonly HashSet<Vector2Int> unloading=new HashSet<Vector2Int>();
    readonly Dictionary<Vector2Int,float> retry=new Dictionary<Vector2Int,float>();
    float nextCheck;
    bool closing;
    void Update() {
        if(!player||Time.unscaledTime<nextCheck||closing)return; nextCheck=Time.unscaledTime+.5f;
        var cell=new Vector2Int(Mathf.FloorToInt(player.position.x/sectorSize),Mathf.FloorToInt(player.position.z/sectorSize));
        foreach(var sector in sectors) {
            int range=Mathf.Max(Mathf.Abs(cell.x-sector.cell.x),Mathf.Abs(cell.y-sector.cell.y));
            if(range<=loadRadius&&!loaded.ContainsKey(sector.cell)&&!unloading.Contains(sector.cell)&&(!retry.TryGetValue(sector.cell,out var at)||Time.unscaledTime>at))Load(sector);
        }
        var release=new List<Vector2Int>();
        foreach(var pair in loaded) {
            int range=Mathf.Max(Mathf.Abs(cell.x-pair.Key.x),Mathf.Abs(cell.y-pair.Key.y));
            if(range>unloadRadius&&pair.Value.IsDone)release.Add(pair.Key);
        }
        foreach(var key in release)Unload(key);
    }
    void Load(SectorReference sector) {
        var key=sector.cell;
        var handle=Addressables.LoadSceneAsync(sector.address,LoadSceneMode.Additive);
        loaded.Add(key,handle);
        handle.Completed+=h=> {
            if(h.Status==AsyncOperationStatus.Succeeded) { if(closing)Unload(key); }
            else { loaded.Remove(key); retry[key]=Time.unscaledTime+10; Debug.LogError("Sector failed: "+sector.address); if(h.IsValid())Addressables.Release(h); }
        };
    }
    void Unload(Vector2Int key) {
        if(!loaded.TryGetValue(key,out var handle))return;
        loaded.Remove(key);
        if(handle.Status==AsyncOperationStatus.Succeeded) {
            unloading.Add(key);
            Addressables.UnloadSceneAsync(handle,true).Completed+=_=>unloading.Remove(key);
        } else if(handle.IsValid())Addressables.Release(handle);
    }
    void OnDestroy() { closing=true; var keys=new List<Vector2Int>(loaded.Keys); foreach(var key in keys)if(loaded[key].IsDone)Unload(key); }
}
}
