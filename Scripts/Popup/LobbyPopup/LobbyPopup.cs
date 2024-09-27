using System;
using Cysharp.Threading.Tasks;
using PlayVibe.Pages;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class LobbyPopup : AbstractBasePopup
    {
        [SerializeField] private LobbyPopupMenuPage menuPage;
        [SerializeField] private LobbyPopupCreateRoomPage createRoomPage;
        [SerializeField] private LobbyPopupRoomPage roomPage;
        [SerializeField] private LobbyPopupFindRoomPage findRoomPage;
        [SerializeField] private LobbySettingsPage settingsPage;
        [Space] 
        [SerializeField] private Button friendsButton;
        [Space]
        [SerializeField] private UnityEvent<string> emitLobbyState;

        [Inject] private PopupService popupService;

        private readonly Subject<LobbyPopupPageType> emitPageChanged = new();
        
        public LobbyPopupMenuPage MenuPage => menuPage;
        public LobbyPopupCreateRoomPage CreateRoomPage => createRoomPage;
        public LobbyPopupRoomPage RoomPage => roomPage;
        public LobbyPopupFindRoomPage FindRoomPage => findRoomPage;
        public IObservable<LobbyPopupPageType> EmitPageChanged => emitPageChanged;
        
        protected override UniTask OnShow(object data = null)
        {
            diContainer.Inject(menuPage);
            diContainer.Inject(createRoomPage);
            diContainer.Inject(roomPage);
            diContainer.Inject(findRoomPage);
            diContainer.Inject(settingsPage);
            
            menuPage.Initialize(this).Forget();
            createRoomPage.Initialize(this).Forget();
            roomPage.Initialize(this).Forget();
            findRoomPage.Initialize(this).Forget();
            settingsPage.Initialize(this).Forget();

            SetPage(LobbyPopupPageType.Menu);
            
            friendsButton.OnClickAsObservable().Subscribe(_ => ShowFriendsPopup()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        private void ShowFriendsPopup()
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.FriendsPopup, null, PopupGroup.Overlay)).Forget();
        }

        protected override UniTask OnHide()
        {
            menuPage.Deinitialize().Forget();
            createRoomPage.Deinitialize().Forget();
            roomPage.Deinitialize().Forget();
            findRoomPage.Deinitialize().Forget();
            settingsPage.Deinitialize().Forget();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }

        public void SetPage(LobbyPopupPageType type)
        {
            emitLobbyState?.Invoke(type.ToString());
            emitPageChanged.OnNext(type);
        }
    }
}