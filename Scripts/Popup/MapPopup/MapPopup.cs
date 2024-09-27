using Cysharp.Threading.Tasks;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.MapPopup
{
    public class MapPopup : AbstractBasePopup
    {
        [SerializeField] private RawImage rawImage;
        
        [Inject] private MapCameraController mapCameraController;
        [Inject] private MapCanvas mapCanvas;
        [Inject] private GameplayStage gameplayStage;
        
        protected override UniTask OnShow(object data = null)
        {
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.InMapPopup);
            
            mapCameraController.gameObject.SetActive(true);
            mapCanvas.gameObject.SetActive(true);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.InMapPopup);
            
            mapCameraController.gameObject.SetActive(false);
            mapCanvas.gameObject.SetActive(false);
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
    }
}