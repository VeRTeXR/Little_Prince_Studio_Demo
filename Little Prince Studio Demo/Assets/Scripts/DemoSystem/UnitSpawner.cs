using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class UnitSpawner:MonoBehaviour
{
    [SerializeField] private AddressableAssetTracker addressableAssetTracker;
    
    [SerializeField] private List<AssetReference> unitAssetReferenceList;
    public async void SpawnAllAvailableUnits()
    {
        foreach (var assetReference in unitAssetReferenceList)
            await addressableAssetTracker.LoadUnitPrefab(assetReference);
    }
}