using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.GameplayHudPopup
{
    public class GameplaySettingsPopup : AbstractBasePopup
    {
        [SerializeField] private Button returnToLobbyButton;
        [SerializeField] private Button exitToDesktopButton;
        [SerializeField] private Button hideButton;

        [Inject] private PopupService popupService;
        [Inject] private GameplayStage gameplayStage;
        
        protected override UniTask OnShow(object data = null)
        {
            returnToLobbyButton.OnClickAsObservable().Subscribe(_ => OnReturnToLobbyClick()).AddTo(CompositeDisposable);
            exitToDesktopButton.OnClickAsObservable().Subscribe(_ => OnExitToDesktopClick()).AddTo(CompositeDisposable);
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void OnReturnToLobbyClick()
        {
            Hide().Forget();

            gameplayStage.ReturnToLobby();
        }
        
        private void OnExitToDesktopClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}