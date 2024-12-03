using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AddressableSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DemoSystem
{
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

        public void SpawnPreTutorialUnits()
        {
            SpawnUnitToAnchor(preTutorialReferenceList, preTutorialUnitAnchor);
        }

        public void SpawnPostTutorialUnits()
        {
            SpawnUnitToAnchor(postTutorialReferenceList, postTutorialUnitAnchor);
        }

        private void SpawnUnitToAnchor(List<AssetReference> assetReferences, Transform anchorTransform)
        {
            for (var index = 0; index < assetReferences.Count; index++)
            {
                var assetReference = assetReferences[index];
                var targetSpawnPosition = new Vector3(anchorTransform.position.x + index, anchorTransform.position.y,
                    anchorTransform.position.z);
                addressableAssetTracker.LoadUnitPrefab(assetReference, (prefab) =>
                {
                    OnUnitLoaded(targetSpawnPosition,prefab,anchorTransform);
                });
            }
        }

        private void OnUnitLoaded(Vector3 targetSpawnPosition, GameObject prefab, Transform anchorTransform)
        {
            var unitInstance = Instantiate(prefab, anchorTransform);
            unitInstance.transform.position = targetSpawnPosition;
            unitInstance.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
}