using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using PlayVibe.Photon;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils.Extensions;
using Zenject;

namespace PlayVibe.Pages
{
    [Serializable]
    public class LobbyPopupMenuPage : AbstractLobbyPopupPage
    {
        [SerializeField] private Button debugButton;
        [SerializeField] private Button quickMatchButton;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button findRoomButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        
        protected override UniTask OnInitialize()
        {
            quickMatchButton.OnClickAsObservable().Subscribe(_ => OnQuickMatchButtonClick()).AddTo(CompositeDisposable);
            createRoomButton.OnClickAsObservable().Subscribe(_ => OnCreateRoomButtonClick()).AddTo(CompositeDisposable);
            findRoomButton.OnClickAsObservable().Subscribe(_ => OnFindRoomButtonClick()).AddTo(CompositeDisposable);
            settingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsClick()).AddTo(CompositeDisposable);
            exitButton.OnClickAsObservable().Subscribe(_ => OnExitButtonClick()).AddTo(CompositeDisposable);
            debugButton.OnClickAsObservable().Subscribe(_ => OnDebugButtonClick().Forget()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnDeinitialize()
        {
            return UniTask.CompletedTask;
        }
        
        protected override UniTask OnShow()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }
        
        private void OnQuickMatchButtonClick()
        {
            if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
            {
                return;
            }
            
            var possibleRooms = mainStage.Rooms.Where(room =>
            {
                var hasPassword = room.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Password, out var hashedPasswordObject) && !string.IsNullOrEmpty((string)hashedPasswordObject);
                var isOpen = room.IsOpen && room.IsVisible && room.PlayerCount < room.MaxPlayers;

                return isOpen && !hasPassword;
            }).ToList();

            if (possibleRooms.Count > 0)
            {
                PhotonNetwork.JoinRoom(possibleRooms.GetRandom().Name);
            }
            else
            {
                ShowInfoPopup($"There are no rooms available for quick search");
            }
        }

        private void OnCreateRoomButtonClick()
        {
            LobbyPopup.SetPage(LobbyPopupPageType.CreateRoom);
        }

        private void OnFindRoomButtonClick()
        {
            LobbyPopup.FindRoomPage.RefreshRoomContainers(mainStage.Rooms).Forget();
            LobbyPopup.SetPage(LobbyPopupPageType.FindRoom);
        }

        private void OnSettingsClick()
        {
            LobbyPopup.SetPage(LobbyPopupPageType.Settings);
        }

        private void OnExitButtonClick()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        private async UniTask OnDebugButtonClick()
        {
            eventAggregator.SendEvent(new TryCreateRoomEvent
            {
                OwnerName = PhotonNetwork.LocalPlayer.NickName,
                RoomName = Guid.NewGuid().ToString(),
                RoomPassword = "dev",
                EnableAdminPopup = true,
                AutoRoleBalanceEnabled = false,
                Location = "Location1"
            });

            await UniTask.WaitUntil(() => PhotonNetwork.InRoom);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            eventAggregator.SendEvent(new StartGameplayEvent());
        }
    }
}