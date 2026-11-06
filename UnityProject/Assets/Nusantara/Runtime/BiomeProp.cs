using UnityEngine;
namespace Nusantara {
public class BiomeProp : MonoBehaviour {
    public GameObject[] variants;
    void OnEnable() {EnvironmentController.BiomeChanged+=Apply;Apply(EnvironmentController.ActiveBiome);}
    void OnDisable() {EnvironmentController.BiomeChanged-=Apply;}
    void Apply(int biome) {if(variants==null)return;for(int i=0;i<variants.Length;i++)if(variants[i])variants[i].SetActive(i==biome);}
}
}
