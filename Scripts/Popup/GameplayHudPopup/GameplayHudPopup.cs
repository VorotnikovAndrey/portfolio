using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.GameplayHudPopup
{
    public class GameplayHudPopup : AbstractBasePopup
    {
        [SerializeField] private Button settingsButton;

        [Inject] private PopupService popupService;
        
        protected override UniTask OnShow(object data = null)
        {
            settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsClick()).AddTo(CompositeDisposable);
            
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
        
        private void OnSettingsClick()
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.GameplaySettingsPopup)).Forget();
        }
    }
}