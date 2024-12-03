using UnityEngine;

public class DemoSequencer : MonoBehaviour
{
    [SerializeField] private SplashScreenPage splashScreenPage;
    [SerializeField] private UnitSpawner unitSpawner;
    private void Awake()
    {
        splashScreenPage.SetOnCloseEvent(() =>
        {
            unitSpawner.SpawnAllAvailableUnits();        
        });
    }
}