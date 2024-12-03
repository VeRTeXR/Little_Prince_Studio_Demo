using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

public class UnitSpawner:MonoBehaviour
{
    [Header("Dependencies")]   
    [SerializeField] private AddressableAssetTracker addressableAssetTracker;
    
    [Header("Addressable References ")]
    [SerializeField] private List<AssetReference> preTutorialReferenceList;
    [SerializeField] private List<AssetReference> postTutorialReferenceList;
   
    [Header("World Space Anchor")]
    [SerializeField] private Transform preTutorialUnitAnchor;
    [SerializeField] private Transform postTutorialUnitAnchor;
    
    public async void SpawnPreTutorialUnits()
    {
        try
        {
            await SpawnUnitToAnchor(preTutorialReferenceList, preTutorialUnitAnchor);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
        }
    }
    
    public async void SpawnPostTutorialUnits()
    {
        try
        {
            await SpawnUnitToAnchor(postTutorialReferenceList, postTutorialUnitAnchor);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    private async Task SpawnUnitToAnchor(List<AssetReference> assetReferences, Transform anchorTransform)
    {
        for (var index = 0; index < assetReferences.Count; index++)
        {
            var assetReference = assetReferences[index];
            var unitPrefab = await addressableAssetTracker.LoadUnitPrefab(assetReference);
            var unitInstance = Instantiate(unitPrefab, anchorTransform);
            unitInstance.transform.position = new Vector3(anchorTransform.position.x+index, anchorTransform.position.y, anchorTransform.position.z);
            unitInstance.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}