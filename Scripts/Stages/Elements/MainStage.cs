using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.Photon;
using PlayVibe.RolePopup;
using Services;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

namespace PlayVibe
{
    public class MainStage : AbstractStageBase, ILobbyCallbacks, IInRoomCallbacks, IMatchmakingCallbacks, IOnEventCallback
    {
        public override string StageType => Constants.Stages.Main;

        [Inject] private PopupService popupService;
        [Inject] private StartupService startupService;
        [Inject] private StageService stageService;
        [Inject] private Balance balance;
        [Inject] private ControlSettingsManager controlSettingsManager;

        private UniTaskCompletionSource joinedLobbyCompletionSource;
        private LobbyPopup lobbyPopup;

        public readonly HashSet<RoomInfo> Rooms = new();
        
        private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequestedCallback;

        public override async UniTask Initialize(object data = null)
        {
            base.Initialize(data);
            
            ApplyDefaultSettings();
            Subscribes();

            var mainPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.MainPopup));
            var connectionPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.ConnectionPopup));

            await UniTask.WaitUntil(() => SteamManager.Initialized);
            Debug.Log($"Steam Initialized".AddColorTag(Color.cyan));
            
            await startupService.Run();
            joinedLobbyCompletionSource = new UniTaskCompletionSource();
            
            await SceneManager.LoadSceneAsync(Constants.Scenes.Lobby);
            await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
            
            /*await popupService.ShowPopup(new PopupOptions(Constants.Popups.RegionPopup), () =>
            {
                PhotonNetwork.JoinLobby();
            });*/

            if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            
            await UniTask.WaitUntil(() => PhotonNetwork.InLobby);
            await joinedLobbyCompletionSource.Task;
            await connectionPopup.Hide();

            if (SteamManager.Initialized)
            {
                PhotonNetwork.NickName = SteamFriends.GetPersonaName();
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "SteamID", SteamUser.GetSteamID().m_SteamID } });
            }
                
            lobbyPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.LobbyPopup)) as LobbyPopup;
            mainPopup.Hide().Forget();

            CheckSteamInvite();
            
            gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        }

        private void ApplyDefaultSettings()
        {
            Application.targetFrameRate = 60;
            controlSettingsManager.LoadControlSettings();
        }
        
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            if (!PhotonNetwork.InLobby)
            {
                return;
            }
            
            CheckSteamInvite();
        }

        private void CheckSteamInvite()
        {
            var args = Environment.GetCommandLineArgs();
            
            foreach (var arg in args)
            {
                if (!arg.StartsWith("join?room="))
                {
                    continue;
                }
                
                var roomName = arg["join?room=".Length..];
                PhotonNetwork.JoinRoom(roomName);
            }
        }
        
        public override UniTask DeInitialize()
        {
            base.DeInitialize();

            UnSubscribes();
            
            return UniTask.CompletedTask;
        }

        private void Subscribes()
        {
            PhotonNetwork.AddCallbackTarget(this);
            
            eventAggregator.Add<TryCreateRoomEvent>(OnTryCreateRoomEvent);
            eventAggregator.Add<StartGameplayEvent>(OnStartGameplayEvent);
        }

        private void UnSubscribes()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            
            eventAggregator.Remove<TryCreateRoomEvent>(OnTryCreateRoomEvent);
            eventAggregator.Remove<StartGameplayEvent>(OnStartGameplayEvent);
        }
        
        private void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }

        private void ShowInfoPopup(short returnCode, string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = $"[Code:{returnCode}] {message}"
            }, PopupGroup.System)).Forget();
        }
        
        public void OnTryCreateRoomEvent(TryCreateRoomEvent sender)
        {
            if (!PhotonNetwork.InLobby || !PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(sender.RoomName) || string.IsNullOrEmpty(sender.OwnerName))
            {
                return;
            }

            var roomOptions = new RoomOptions
            {
                MaxPlayers = balance.Main.MaxPlayersInRoom,
                IsVisible = true,
                IsOpen = true,
                CustomRoomPropertiesForLobby = new[]
                {
                    Constants.Room.CustomProperties.Owner,
                    Constants.Room.CustomProperties.Password,
                    Constants.Room.CustomProperties.Region,
                    Constants.Room.CustomProperties.EnableAdminPopup,
                    Constants.Room.CustomProperties.AutoRoleBalanceEnabled,
                    Constants.Room.CustomProperties.SelectedLocation,
                }
            };

            var customProperties = new Hashtable
            {
                { Constants.Room.CustomProperties.Owner, sender.OwnerName },
                { Constants.Room.CustomProperties.Region, sender.Region },
                { Constants.Room.CustomProperties.EnableAdminPopup, sender.EnableAdminPopup },
                { Constants.Room.CustomProperties.AutoRoleBalanceEnabled, sender.AutoRoleBalanceEnabled },
                { Constants.Room.CustomProperties.SelectedLocation, sender.Location },
            };

            if (!string.IsNullOrEmpty(sender.RoomPassword))
            {
                customProperties.Add(Constants.Room.CustomProperties.Password, sender.RoomPassword.ToHashPassword());
            }

            roomOptions.CustomRoomProperties = customProperties;

            PhotonNetwork.CreateRoom(sender.RoomName, roomOptions, TypedLobby.Default);
        }
        
        private void OnStartGameplayEvent(StartGameplayEvent sender)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.LoadLevelEvent;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }

        #region Lobby

        public void OnJoinedLobby()
        {
            Debug.Log($"PhotonNetwork OnJoinedLobby".AddColorTag(Color.cyan));
            
            joinedLobbyCompletionSource?.TrySetResult();
        }

        public void OnLeftLobby()
        {
            Debug.Log($"PhotonNetwork OnLeftLobby".AddColorTag(Color.cyan));
        }
        
        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log($"PhotonNetwork OnRoomListUpdate".AddColorTag(Color.cyan));

            foreach (var item in roomList)
            {
                if (!item.IsVisible || !item.IsOpen || item.RemovedFromList)
                {
                    Rooms.RemoveWhere(room => room.Name == item.Name);
                    continue;
                }
 
                Rooms.RemoveWhere(room => room.Name == item.Name);
                Rooms.Add(item);
            }
        }
        
        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            Debug.Log($"PhotonNetwork OnLobbyStatisticsUpdate".AddColorTag(Color.cyan));
        }

        #endregion

        #region Room

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (lobbyPopup == null)
            {
                return;
            }

            lobbyPopup.RoomPage.RefreshUserContainers().Forget();
            
            Debug.Log($"PhotonNetwork OnPlayerEnteredRoom".AddColorTag(Color.cyan));
        }

        public async void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (otherPlayer.ActorNumber == 1)
            {
                ShowInfoPopup($"The master has left the room, you have been returned to the game lobby.");
                await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
                PhotonNetwork.LeaveRoom();
                return;
            }
            
            if (lobbyPopup == null)
            {
                return;
            }

            lobbyPopup.RoomPage.RefreshUserContainers().Forget();
            
            Debug.Log($"PhotonNetwork OnPlayerLeftRoom".AddColorTag(Color.cyan));
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log($"PhotonNetwork OnRoomPropertiesUpdate".AddColorTag(Color.cyan));
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log($"PhotonNetwork OnPlayerPropertiesUpdate".AddColorTag(Color.cyan));
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"PhotonNetwork OnMasterClientSwitched".AddColorTag(Color.cyan));
        }

        #endregion

        #region Matchmaking

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            Debug.Log($"PhotonNetwork OnFriendListUpdate".AddColorTag(Color.cyan));
        }

        public void OnCreatedRoom()
        {
            if (lobbyPopup == null)
            {
                return;
            }
            
            Debug.Log($"PhotonNetwork OnCreatedRoom".AddColorTag(Color.cyan));
        }

        public async void OnCreateRoomFailed(short returnCode, string message)
        {
            ShowInfoPopup(returnCode, message);
            
            await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
            
            if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            
            Debug.Log($"PhotonNetwork OnCreateRoomFailed".AddColorTag(Color.cyan));
        }

        public async void OnJoinedRoom()
        {
            if (lobbyPopup == null)
            {
                return;
            }

            var source = new UniTaskCompletionSource();
            
            await popupService.ShowPopup(new PopupOptions(Constants.Popups.NetworkLoadingPopup, new NetworkLoadingPopupData
            {
                Source = source
            }));
            
            await lobbyPopup.RoomPage.RefreshUserContainers();
            
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Constants.Room.CustomProperties.SelectedLocation, out var location);
            lobbyPopup.RoomPage.SetLocationName(location.ToString());
            lobbyPopup.RoomPage.SetRoomName(PhotonNetwork.CurrentRoom.Name);
            lobbyPopup.RoomPage.Refresh();
            lobbyPopup.SetPage(LobbyPopupPageType.Room);
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.ChatPopup, RoleType.None)).Forget();
            
            source.TrySetResult();
            
            SteamFriends.SetRichPresence("status", "In a game");
            
            Debug.Log($"PhotonNetwork OnJoinedRoom".AddColorTag(Color.cyan));
        }

        public async void OnJoinRoomFailed(short returnCode, string message)
        {
            ShowInfoPopup(returnCode, message);
            
            await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
            
            if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            
            Debug.Log($"PhotonNetwork OnJoinRoomFailed".AddColorTag(Color.cyan));
        }

        public async void OnJoinRandomFailed(short returnCode, string message)
        {
            ShowInfoPopup(returnCode, message);
            
            await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
            
            if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            
            Debug.Log($"PhotonNetwork OnJoinRandomFailed".AddColorTag(Color.cyan));
        }

        public async void OnLeftRoom()
        {
            Debug.Log($"PhotonNetwork OnLeftRoom".AddColorTag(Color.cyan));

            var source = new UniTaskCompletionSource();
            
            await popupService.ShowPopup(new PopupOptions(Constants.Popups.NetworkLoadingPopup, new NetworkLoadingPopupData
            {
                Source = source
            }));
            
            await popupService.TryHidePopup(Constants.Popups.ChatPopup);
            
            await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady);
            
            if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
            {
                PhotonNetwork.JoinLobby();
            }
           
            await UniTask.WaitUntil(() => PhotonNetwork.InLobby);

            source.TrySetResult();
            
            if (lobbyPopup == null)
            {
                return;
            }
            
            lobbyPopup.SetPage(LobbyPopupPageType.Menu);
        }

        #endregion

        #region Events

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != PhotonPeerService.UniversalEventCode)
            {
                return;
            }
            
            if (photonEvent.CustomData is not PhotonPeerData peerData)
            {
                return;
            }
            
            if (peerData.Code == PhotonPeerEvents.LoadLevelEvent)
            {
                stageService.SetStageAsync<GameplayStage>().Forget();
            }
        }
        
        #endregion
    }
}