using UnityEngine;
namespace Nusantara {
[CreateAssetMenu(menuName="Nusantara/Bus upgrade")]
public class BusUpgradeSO : ScriptableObject {
    public string upgradeId="engine";
    public long price=50000;
    [Range(1,2)] public float torqueMultiplier=1.15f, brakeMultiplier=1, suspensionMultiplier=1;
}
}
