using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Gameplay.Player;
using Photon.Pun;
using Photon.Realtime;
using Zenject;

namespace PlayVibe
{
    public class GameplayStage : AbstractStageBase, IInRoomCallbacks
    {
        public override string StageType => Constants.Stages.Gameplay;

        [Inject] private PopupService popupService;
        [Inject] private StageService stageService;
        [Inject] private ObjectPoolService objectPoolService;

        private bool leaved;
        
        public Dictionary<int, GameplayData> GameplayDataDic { get; } = new();
        public GameplayData LocalGameplayData { get; private set; }
        public MasterData MasterData { get; private set; }
        public LevelData LevelData { get; private set; }
        public StatisticData StatisticData { get; private set; }
        public int CurrentDay { get; private set; }
        public int TimeOfDayChangeCounter { get; private set; }

        public override async UniTask Initialize(object data = null)
        {
            base.Initialize(data);
            
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

            leaved = false;
            CurrentDay = 0;
            TimeOfDayChangeCounter = -1;
            GameplayDataDic.Clear();
            LocalGameplayData = null;
            LevelData = new LevelData();
            StatisticData = new StatisticData();
            MasterData = null;
            
            Subscribes();
            Prepare();
            
            popupService.TryHidePopup(Constants.Popups.ChatPopup).Forget();
            var loadingPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.NetworkLoadingPopup, null, PopupGroup.Overlay));
            
            popupService.TryHidePopup(Constants.Popups.LobbyPopup).Forget();
            
            var gameplayReadyPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.GameplayReadyPopup)) as GameplayReadyPopup;
            await UniTask.WaitUntil(() => gameplayReadyPopup.Ready);

            loadingPopup.Hide().Forget();

            if (PhotonNetwork.IsMasterClient)
            {
                MasterData = new MasterData();
                
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Constants.Room.CustomProperties.SelectedLocation, out var location);
                PhotonNetwork.LoadLevel(location.ToString());
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
        }

        private void UnSubscribes()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        
        private void Prepare()
        {
            GameplayDataDic.Clear();
            
            foreach (var element in PhotonNetwork.CurrentRoom.Players)
            {
                GameplayDataDic.Add(element.Value.ActorNumber, new GameplayData
                {
                    ActorNumber = element.Value.ActorNumber,
                    Nickname = element.Value.NickName
                });
            }

            LocalGameplayData = GameplayDataDic[PhotonNetwork.LocalPlayer.ActorNumber];
        }

        public void ReturnToLobby()
        {
            if (leaved)
            {
                return;
            }
            
            leaved = true;
            
            eventAggregator.SendEvent(new LeaveRoomEvent());

            popupService.HideGroup(PopupGroup.Hud, true);
            popupService.HideGroup(PopupGroup.Gameplay, true);
            popupService.HideGroup(PopupGroup.Overlay, true);
            popupService.HideGroup(PopupGroup.Tutorial, true);
            
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                
                foreach (var player in PhotonNetwork.PlayerListOthers)
                {
                    if (player.IsMasterClient)
                    {
                        continue;
                    }
                    
                    PhotonNetwork.CloseConnection(player);
                }
            }

            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            
            stageService.SetStageAsync<MainStage>().Forget();
        }
        
        private void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (otherPlayer.ActorNumber != 1)
            {
                return;
            }
            
            ShowInfoPopup("The master has left the room, you have been returned to the game lobby.");
            ReturnToLobby();
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            
        }

        public void OverrideDay(int value)
        {
            CurrentDay = value;
        }
        
        public void IncrementationTimeOfDayChangeCounter()
        {
            TimeOfDayChangeCounter++;
        }
    }
}