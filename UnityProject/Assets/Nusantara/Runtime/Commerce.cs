using System;
using UnityEngine;
namespace Nusantara {
public interface IRewardedAds { bool IsReady {get;} void Show(Action<bool> completed); }
public interface IStore { void Purchase(string productId,Action<bool> verified); }
// No fake rewards: live buttons stay disabled until a real, verified SDK adapter is supplied.
public class Commerce : MonoBehaviour {
    public IRewardedAds Ads;
    public IStore Store;
    bool showing;
    public bool RewardReady => !showing&&Ads!=null&&Ads.IsReady;
    public void Reward(Action success) {
        if(!RewardReady)return; showing=true;
        bool completed=false;
        try { Ads.Show(earned=> { if(completed)return; completed=true; showing=false; if(earned)success?.Invoke(); }); }
        catch(Exception e) { showing=false; Debug.LogWarning("Reward service unavailable: "+e.Message); }
    }
}
}
