using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableAssetTracker : MonoBehaviour
{
    
    private Dictionary<AssetReference, Sprite> _loadedItemAddressableDictionary = new();
    private Dictionary<AssetReference, AsyncOperationHandle<Sprite>> _loadedItemHandleDictionary = new();
    
    private Dictionary<AssetReference, GameObject> _loadedUnitPrefab = new();
    private Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _loadedUnitPrefabHandleDictionary = new();
    
    public void LoadItemSpriteData(AssetReference asset, Action<Sprite> onComplete)
    {
        if (asset == null)
        {
            onComplete?.Invoke(null);
            return;
        }
        
        if (_loadedItemAddressableDictionary.TryGetValue(asset, out var loadedSprite))
            onComplete(loadedSprite);
        else
        {
            if (!asset.RuntimeKeyIsValid()) return;
            var loadHandle = Addressables.LoadAssetAsync<Sprite>(asset);
            _loadedItemHandleDictionary.TryAdd(asset, loadHandle);
            loadHandle.Completed += handle =>
            {
                _loadedItemAddressableDictionary.TryAdd(asset, handle.Result);
                onComplete(handle.Result);
            };
        }
    }   
    
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
    
    public async UniTask<GameObject> LoadUnitPrefab(AssetReference asset)
    {
        if (asset == null) return null;
        
        if (_loadedUnitPrefab.TryGetValue(asset, out var loadedUnit))
            return loadedUnit;
        
        if (!asset.RuntimeKeyIsValid()) return null;
        
        var loadHandle = Addressables.LoadAssetAsync<GameObject>(asset);
        _loadedUnitPrefabHandleDictionary.TryAdd(asset, loadHandle);

        var unitPrefab = await loadHandle;
        if (unitPrefab == null) return null;

        _loadedUnitPrefab.TryAdd(asset, unitPrefab);
        return unitPrefab;
    }

    public GameObject GetPreloadedUnitPrefab(AssetReference asset)
    {
        if (_loadedUnitPrefab.TryGetValue(asset, out var loadedUnit))
            return loadedUnit;
        return null;
    }  
}
