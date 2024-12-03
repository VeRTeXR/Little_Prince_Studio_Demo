using UI.SplashScreen;
using UnityEngine;

namespace DemoSystem
{
    public class DemoSequencer : MonoBehaviour
    {
        [SerializeField] private SplashScreenPage splashScreenPage;
        [SerializeField] private UnitSpawner unitSpawner;
        private void Awake()
        {
            unitSpawner.SpawnPreTutorialUnits();        

            splashScreenPage.SetOnCloseEvent(() =>
            {
                splashScreenPage.gameObject.SetActive(false);
                unitSpawner.SpawnPostTutorialUnits();        
            });
        }
    }
}