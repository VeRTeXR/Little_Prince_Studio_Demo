using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressableSystem
{
    public class AddressableHandler : MonoBehaviour
    {
        private Action _onAddressableLoaded;
        public Action<float> OnLoadingProgressUpdated;

        private int _assetsLoadedCount;
        private int _assetToLoadCount;

        private long _totalBytesDownloaded;
        private long _totalBytesToDownload;

        private bool _isUpdateInProgress;
        private List<IResourceLocation> _loadingReferenceList;

        public void SetAddressableLoadedCallback(Action onAddressableLoaded)
        {
            _onAddressableLoaded = null;
            _onAddressableLoaded = onAddressableLoaded;
        }

        public void StartLoadingAddressableSequence()
        {
            _loadingReferenceList = GetAllAddressableLocation();
            _assetToLoadCount = _loadingReferenceList.Distinct().ToList().Count;
            StartLoadingAddressableAssets(_loadingReferenceList);
        }

        private static List<IResourceLocation> GetAllAddressableLocation()
        {
            var allLocations = new List<IResourceLocation>();
            foreach (var resourceLocator in Addressables.ResourceLocators)
                if (resourceLocator is ResourceLocationMap map)
                    foreach (var locations in map.Locations.Values)
                        allLocations.AddRange(locations);
            return allLocations;
        }

        private async void StartLoadingAddressableAssets(List<IResourceLocation> resourceLocations)
        {
            if (resourceLocations == null)
            {
                Debug.LogError("Failed to load addressable resource locations.");
                return;
            }

            var uniqueResourceList = resourceLocations.Distinct().ToList();

            var hasAddressableToDownload = await HasAddressableToDownload(uniqueResourceList);
            if (!hasAddressableToDownload) return;

            _isUpdateInProgress = true;
            _totalBytesToDownload = 0;
            _totalBytesDownloaded = 0;

            ProcessDownload(uniqueResourceList);
        }

        private async void ProcessDownload(List<IResourceLocation> resourceLocations)
        {
            var downloadFailedList = new List<IResourceLocation>();
            var downloadTasks = new List<Task>();
            foreach (var irl in resourceLocations)
            {
                var loadHandle = Addressables.DownloadDependenciesAsync(irl.PrimaryKey, true);
                TrackDownloadSize(loadHandle).Forget();

                var taskCompletionSource = new TaskCompletionSource<bool>();
                loadHandle.Completed += loadingOperationHandle =>
                {
                    var downloadError = GetDownloadError(loadHandle);
                    if (!string.IsNullOrEmpty(downloadError))
                        Debug.LogError($"Load Handle Error: {downloadError}");

                    if (loadingOperationHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (loadingOperationHandle.Result == null) return;
                        _assetsLoadedCount++;
                        Debug.Log($"({_assetsLoadedCount}/{_assetToLoadCount}) Asset Loaded : " + irl.PrimaryKey);
                        taskCompletionSource.SetResult(true);
                    }
                    else if (loadingOperationHandle.Status == AsyncOperationStatus.Failed)
                    {
                        downloadFailedList.Add(irl);
                        taskCompletionSource.SetResult(false);
                    }
                };

                downloadTasks.Add(taskCompletionSource.Task);
            }

            await Task.WhenAll(downloadTasks);

            if (downloadFailedList.Any())
            {
                //TODO:: Trigger Popup on Download failed to allow player retry
                // PopupHelper.TriggerDisconnectionPopup(this, () => { ProcessDownload(downloadFailedList); });
            }
            else
            {
                _isUpdateInProgress = false;
                if (_assetsLoadedCount >= _assetToLoadCount)
                {
                    OnLoadingProgressUpdated?.Invoke(1);

                    Debug.Log($"Asset Download completed: {_totalBytesDownloaded}/{_totalBytesToDownload}");
                    Debug.Log($"Asset Loading completed: {_assetsLoadedCount}, Total Asset: {_assetToLoadCount}");
                    Debug.Log("Download and asset loading completed.");
                    _onAddressableLoaded?.Invoke();
                    _isUpdateInProgress = false;
                }
                else
                {
                    Debug.LogError($"Asset Loaded Count is less than the Total Asset Needed");
                }
            }
        }

        private string GetDownloadError(AsyncOperationHandle handle)
        {
            if (handle.Status != AsyncOperationStatus.Failed)
                return null;

            var exception = handle.OperationException;
            while (exception != null)
            {
                if (exception is RemoteProviderException remoteException)
                    return remoteException.WebRequestResult.Error;
                exception = exception.InnerException;
            }

            return "Unknown error occurred.";
        }

        private async UniTaskVoid TrackDownloadSize(AsyncOperationHandle loadHandle)
        {
            long lastDownloadedBytes = 0;
            _totalBytesToDownload += loadHandle.GetDownloadStatus().TotalBytes;

            while (loadHandle.IsValid() && !loadHandle.IsDone)
            {
                var currentDownloadedBytes = loadHandle.GetDownloadStatus().DownloadedBytes;
                var incrementalDownloadSize = currentDownloadedBytes - lastDownloadedBytes;
                _totalBytesDownloaded += incrementalDownloadSize;
                lastDownloadedBytes = currentDownloadedBytes;

                await UniTask.Delay(100);
            }
        }


        private async Task<bool> HasAddressableToDownload(List<IResourceLocation> resourceLocations)
        {
            var totalBytesToDownload = await GetTotalDownloadSize(resourceLocations);
            if (totalBytesToDownload > 0)
                return true;

            return false;
        }

        private async Task<long> GetTotalDownloadSize(List<IResourceLocation> resourceLocations)
        {
            var downloadSizeTasks = new List<Task<long>>();
            foreach (var irl in resourceLocations)
            {
                var sizeTask = Addressables.GetDownloadSizeAsync(irl.PrimaryKey).Task;
                downloadSizeTasks.Add(sizeTask);
            }

            await Task.WhenAll(downloadSizeTasks);

            var totalSize = downloadSizeTasks.Sum(task => task.Result);
            _totalBytesToDownload = totalSize;

            return totalSize;
        }

        private void Update()
        {
            if (!_isUpdateInProgress) return;

            var downloadProgress = (float)_totalBytesDownloaded / _totalBytesToDownload * 0.90f;
            var assetCountProgress = (float)_assetsLoadedCount / _assetToLoadCount * 0.10f;
            var progress = downloadProgress + assetCountProgress;

            OnLoadingProgressUpdated?.Invoke(progress);
        }

        public async Task<bool> HasExternalAddressableToDownload()
        {
            var resourceLocations = GetAllAddressableLocation();
            return await HasAddressableToDownload(resourceLocations);
        }

        public bool CheckIfDownloadInProgress()
        {
            if (_isUpdateInProgress)
                if (_assetsLoadedCount < _assetToLoadCount)
                    return true;
            return false;
        }
    }
}