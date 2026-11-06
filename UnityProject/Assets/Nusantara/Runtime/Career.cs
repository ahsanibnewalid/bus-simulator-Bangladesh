using System;
using System.IO;
using UnityEngine;
namespace Nusantara {
[Serializable] public class CareerData {
    public long balance=245500;
    public Color busColor=new Color(.718f,.11f,.11f);
    public int engineLevel, brakeLevel, suspensionLevel, liverySlots=1, fleetOwned=1;
    public string liveryPath="", repairDay="";
    public int dailyRepairs, selectedFleet, selectedLiverySlot;
    public long pendingFare;
    public bool rewardClaimed;
}
public class Career : MonoBehaviour {
    public CareerData Data {get;private set;}
    public string LastError {get;private set;}
    string PathName => Path.Combine(Application.persistentDataPath,"career.json");
    void Awake() { Load(); }
    void OnEnable() { GameEvents.TripFinished+=Finish; }
    void OnDisable() { GameEvents.TripFinished-=Finish; }
    void Load() {
        try { Data=File.Exists(PathName)?JsonUtility.FromJson<CareerData>(File.ReadAllText(PathName)):new CareerData(); }
        catch(Exception e) {
            Debug.LogWarning("Save could not be read: "+e.Message);
            try { if(File.Exists(PathName+".bak"))Data=JsonUtility.FromJson<CareerData>(File.ReadAllText(PathName+".bak")); }catch(Exception) { Data=null; }
        }
        if(Data==null) Data=new CareerData();
    }
    public void Save() {
        try { string temp=PathName+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(Data));
            if(File.Exists(PathName)) File.Copy(PathName,PathName+".bak",true);
            File.Copy(temp,PathName,true); File.Delete(temp); LastError=null;
        } catch(Exception e) { LastError=e.Message; Debug.LogError("Save failed: "+e.Message); }
    }
    void Finish(int count,float comfort,long fare) { Data.balance+=fare; Data.pendingFare=fare; Data.rewardClaimed=false; Save(); }
    public bool Spend(long price) { if(price<=0||Data.balance<price)return false; Data.balance-=price; return true; }
    public bool BuyUpgrade(BusUpgradeSO upgrade,BusController bus) {
        if(!upgrade||!bus)return false;
        int level=upgrade.upgradeId=="engine"?Data.engineLevel:upgrade.upgradeId=="brakes"?Data.brakeLevel:Data.suspensionLevel;
        if(level>=5||!Spend(upgrade.price))return false;
        if(upgrade.upgradeId=="engine")Data.engineLevel++; else if(upgrade.upgradeId=="brakes")Data.brakeLevel++; else Data.suspensionLevel++;
        bus.ApplyUpgrade(upgrade); Save(); return true;
    }
    public void ApplySaved(BusController bus,BusUpgradeSO engine,BusUpgradeSO brakes,BusUpgradeSO suspension) {
        for(int i=0;i<Mathf.Clamp(Data.engineLevel,0,5);i++)bus.ApplyUpgrade(engine);
        for(int i=0;i<Mathf.Clamp(Data.brakeLevel,0,5);i++)bus.ApplyUpgrade(brakes);
        for(int i=0;i<Mathf.Clamp(Data.suspensionLevel,0,5);i++)bus.ApplyUpgrade(suspension);
    }
    public bool ClaimDoubleReward() { if(Data.rewardClaimed||Data.pendingFare<=0)return false; Data.balance+=Data.pendingFare; Data.rewardClaimed=true; Save(); return true; }
    public bool RepairAvailable() { RefreshDay(); return Data.dailyRepairs<5; }
    void RefreshDay() { string day=DateTime.UtcNow.ToString("yyyy-MM-dd"); if(day!=Data.repairDay) { Data.repairDay=day; Data.dailyRepairs=0; } }
    public bool ClaimRepair(BusController bus) { if(!RepairAvailable())return false; Data.dailyRepairs++; bus.Repair(); Save(); return true; }
    public bool BuySlot() { if(Data.liverySlots>=10||!Spend(25000))return false; Data.liverySlots++; Save(); return true; }
    public bool UnlockFleet() { if(Data.fleetOwned>=3||!Spend(150000))return false; Data.fleetOwned++; Save(); return true; }
}
}
