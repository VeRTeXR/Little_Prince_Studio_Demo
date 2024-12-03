using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SplashScreenPage : MonoBehaviour
{
    
    [Header("Loading")]
    [SerializeField] private Image loadingProgressFill;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private TextMeshProUGUI loadingStatusText;
    
    [Header("Tap To Start")]
    [SerializeField] private GameObject tapToStartContainer;
    [SerializeField] private Button tapToStartOverlayButton;

    [SerializeField] private AddressableHandler addressableHandler;
    private RectTransform _loadingBarTransform;
    private float _loadingFillValue;
    private bool _isClosing;
    private UnityEvent _onCloseEvent = new();
    private Action _onLoadingFillComplete;
    
    private Tweener _loadingFillTween;

    private void Awake()
    {    
        _loadingBarTransform = (RectTransform) loadingProgressFill.transform;
        SetupButtonCallback();
    }

    
    private void SetupButtonCallback()
    {   
        tapToStartOverlayButton.onClick.RemoveAllListeners();
        tapToStartOverlayButton.onClick.AddListener(OnTapToStartClicked);
    }
    
    private void OnTapToStartClicked()
    {
        //TODO:: Hide and show loaded addressable for this demo purpose, Code below are previous project authentication flow
        // Global.UserDataHandler.GetActiveSession((deviceId) =>
        // {
        //     if (string.IsNullOrEmpty(deviceId))
        //     {
        //         Global.UserDataHandler.CreateNewSession(MaintenanceChecker);
        //     }
        //     else if (deviceId != SystemInfo.deviceUniqueIdentifier)
        //     {
        //         PopupHelper.TriggerConfirmationActiveDevicePopup(this,
        //             onConfirm:()=>
        //             {
        //                 Global.UserDataHandler.CreateNewSession(MaintenanceChecker);
        //             },
        //             onClose: Application.Quit);
        //     }
        //     else
        //     {
        //         Global.UserDataHandler.CreateNewSession(MaintenanceChecker);
        //     }
        // });
    }


    private void StartWaitAddressableDownloadAndCloseSequence()
    {
        if (_isClosing) return;
        WaitAddressableDownloadAndCloseSequence().Forget();
    }

    private async UniTaskVoid WaitAddressableDownloadAndCloseSequence()
    {
        if (_isClosing) return;
        _isClosing = true;
        loadingOverlay.SetActive(true);
        Resources.UnloadUnusedAssets();
        
        
        var addressableUpdateCheckTask = addressableHandler.HasExternalAddressableToDownload();
        Debug.Log( "[AddressableDownload] Checking Addressable Update....");
        var isAddressableHasUpdate = await addressableUpdateCheckTask;
        if (isAddressableHasUpdate)
        {
            Debug.Log("[AddressableDownload] Detected new addressable assets to download ");
            WaitAddressableDownloadAndClose();
            
            //TODO:: Code below are from previous project authentication flow, We could trigger addressable download in background in case player account already existed
            // if (Global.UserDataHandler.CheckForUserProperty(Global.TutorialFinishedKey))
            // {
            //     WaitAddressableDownloadAndClose();
            // }
            // else
            // {
            //     string checkpointId = string.Empty;
            //     if (Global.UserDataHandler.ContainsUserProperty(Global.TutorialSequenceCheckpointKey))
            //         checkpointId = Global.UserDataHandler.CachedCloudData.UserProperty[Global.TutorialSequenceCheckpointKey];
            //     
            //     if (string.IsNullOrEmpty(checkpointId))
            //     {
            //         StartAddressableDownloadInBackground();
            //         _onCloseEvent?.Invoke();
            //     }
            //     else
            //     {   
            //         WaitAddressableDownloadAndClose();
            //     }
            // }
        }
        else
        {
            Debug.Log("[AddressableDownload] There's nothing to download... Closing Splash Screen.... ");
            _onCloseEvent?.Invoke();
        }
    }

    private void StartAddressableDownloadInBackground()
    {
        addressableHandler.StartLoadingAddressableSequence();
    }

    private void WaitAddressableDownloadAndClose()
    {
        addressableHandler.OnLoadingProgressUpdated += UpdateDownloadAddressableProgress;
        addressableHandler.SetAddressableLoadedCallback(() =>
        {
            addressableHandler.OnLoadingProgressUpdated -= UpdateDownloadAddressableProgress;
            _onCloseEvent?.Invoke();
        });
        addressableHandler.StartLoadingAddressableSequence();
    }
    
    private void UpdateDownloadAddressableProgress(float progress)
    {
        _loadingFillValue = progress;
        UpdateLoadingFill(_loadingFillValue, "Downloading...");
    }
    
    private void UpdateLoadingFill(float progress, string loadingStatus = null)
    {
        if (!loadingStatusText.gameObject.activeSelf)
            loadingStatusText.gameObject.SetActive(true);
        if (!loadingProgressFill.gameObject.activeSelf)
            loadingProgressFill.gameObject.SetActive(true);
        
        var floatValue = progress * 100;
        var progressFormat = Mathf.Approximately(floatValue, 1.0f) ? "N0" : "N2";
        loadingStatusText.text = loadingStatus == null ? $"({floatValue.ToString(progressFormat)}%)" : $"{loadingStatus} ({floatValue.ToString(progressFormat)}%)";

        if (_loadingBarTransform != null)
        {
            _loadingBarTransform.localScale = new Vector3(progress, 1,1);
            var localScale = _loadingBarTransform.localScale;
            var inverseScale = new Vector3(1 / localScale.x, 1 / localScale.y, 1 / localScale.z);
            loadingStatusText.transform.localScale = inverseScale;
        }

        if (progress >= 1f)
        {
            if (_onLoadingFillComplete != null)
            {
                _onLoadingFillComplete.Invoke();
                _onLoadingFillComplete = null;          
            }
            
            loadingStatusText.gameObject.SetActive(false); 
            loadingProgressFill.gameObject.SetActive(false);
        }
    }


}
