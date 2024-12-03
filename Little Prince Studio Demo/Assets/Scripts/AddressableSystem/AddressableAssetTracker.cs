using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableSystem
{
    public class AddressableAssetTracker : MonoBehaviour
    {
        private Dictionary<AssetReference, GameObject> _loadedUnitPrefab = new();
        private Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _loadedUnitPrefabHandleDictionary = new();
        
        public void LoadUnitPrefab(AssetReference asset, Action<GameObject> onComplete)
        {
            if (asset == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            if (_loadedUnitPrefab.TryGetValue(asset, out var loadedUnit))
                onComplete(loadedUnit);
            else
            {
                if (!asset.RuntimeKeyIsValid()) return;
                var loadHandle = Addressables.LoadAssetAsync<GameObject>(asset);
                _loadedUnitPrefabHandleDictionary.TryAdd(asset, loadHandle);
                loadHandle.Completed += handle =>
                {
                    _loadedUnitPrefab.TryAdd(asset, handle.Result);
                    onComplete(handle.Result);
                };
            }
        }
    }
}