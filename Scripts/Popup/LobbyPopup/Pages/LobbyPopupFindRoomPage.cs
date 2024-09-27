using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.Pages
{
    [Serializable]
    public class LobbyPopupFindRoomPage : AbstractLobbyPopupPage
    {
        [SerializeField] private RectTransform roomContainerParent;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button backButton;

        [Inject] private DiContainer diContainer;

        private CancellationTokenSource roomCancellationTokenSource;

        private readonly List<RoomContainer> roomContainers = new();
        
        protected override UniTask OnInitialize()
        {
            refreshButton.OnClickAsObservable().Subscribe(_ => RefreshRoomContainers(mainStage.Rooms).Forget()).AddTo(CompositeDisposable);
            backButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnDeinitialize()
        {
            ClearRoomContainers();
            
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

        private void OnBackButtonClick()
        {
            LobbyPopup.SetPage(LobbyPopupPageType.Menu);
        }
        
        public async UniTask RefreshRoomContainers(HashSet<RoomInfo> roomList)
        {
            ClearRoomContainers();

            var cancellationToken = roomCancellationTokenSource.Token;
            var roomCount = 0;
            var currentRegion = PhotonRegionExtensions.CurrentRegion;

            foreach (var room in roomList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (roomCount >= 30)
                {
                    break;
                }

                /*if (room.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Region, out var region))
                {
                    if (region.ToString() != currentRegion)
                    {
                        continue;
                    }
                }*/
                
                if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
                {
                    continue;
                }

                var container = await objectPoolService.GetOrCreateView<RoomContainer>(
                    Constants.Views.RoomContainer,
                    roomContainerParent,
                    true);

                diContainer.Inject(container);

                if (cancellationToken.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    break;
                }
        
                roomContainers.Add(container);

                if (container != null)
                {
                    container.Set(room);
                }

                roomCount++;
            }
        }
        
        private void ClearRoomContainers()
        {
            roomCancellationTokenSource?.Cancel();
            roomCancellationTokenSource?.Dispose();
            roomCancellationTokenSource = new CancellationTokenSource();

            foreach (var container in roomContainers)
            {
                objectPoolService.ReturnToPool(container);
            }

            roomContainers.Clear();
        }
    }
}