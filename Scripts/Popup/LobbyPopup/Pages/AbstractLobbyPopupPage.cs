using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe.Pages
{
    public abstract class AbstractLobbyPopupPage
    {
        [SerializeField] protected LobbyPopupPageType pageType;
        
        [Inject] protected ObjectPoolService objectPoolService;
        [Inject] protected MainStage mainStage;
        [Inject] protected EventAggregator eventAggregator;
        [Inject] protected PopupService popupService;
        
        protected LobbyPopup LobbyPopup { get; set; }
        protected CompositeDisposable CompositeDisposable { get; set; }

        public async UniTask Initialize(LobbyPopup lobbyPopup)
        {
            LobbyPopup = lobbyPopup;
            CompositeDisposable = new CompositeDisposable();

            LobbyPopup.EmitPageChanged.Subscribe(pageType =>
            {
                if (this.pageType == pageType)
                {
                    OnShow();
                }
                else
                {
                    OnHide();
                }
                
            }).AddTo(CompositeDisposable);

            await OnInitialize();
        }

        public async UniTask Deinitialize()
        {
            await OnDeinitialize();
            
            CompositeDisposable?.Dispose();
            CompositeDisposable = null;
        }

        protected abstract UniTask OnInitialize();
        protected abstract UniTask OnDeinitialize();
        protected abstract UniTask OnShow();
        protected abstract UniTask OnHide();
        
        protected void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }
    }
}