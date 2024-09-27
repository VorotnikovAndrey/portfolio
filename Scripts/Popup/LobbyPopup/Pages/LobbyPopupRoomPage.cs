using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.Pages
{
    [Serializable]
    public class LobbyPopupRoomPage : AbstractLobbyPopupPage
    {
        [SerializeField] private RectTransform userContainerParent;
        [SerializeField] private TextMeshProUGUI roomNameText;
        [SerializeField] private TextMeshProUGUI locationNameText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button exitButton;

        [Inject] private DiContainer diContainer;
        
        private CancellationTokenSource userCancellationTokenSource;
        
        private readonly List<LobbyRoomUserContainer> userContainers = new();
        
        protected override UniTask OnInitialize()
        {
            playButton.gameObject.SetActive(false);
            playButton.OnClickAsObservable().Subscribe(_ => OnPlayButtonClick()).AddTo(CompositeDisposable);
            exitButton.OnClickAsObservable().Subscribe(_ => OnExitButtonClick()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnDeinitialize()
        {
            ClearUserContainers();
            
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

        public void Refresh()
        {
            playButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
        
        public void SetRoomName(string name)
        {
            roomNameText.text = name;
        }
        
        public void SetLocationName(string name)
        {
            locationNameText.text = name;
        }

        private void OnExitButtonClick()
        {
            PhotonNetwork.LeaveRoom();
        }

        public async UniTask RefreshUserContainers()
        {
            ClearUserContainers();

            var cancellationToken = userCancellationTokenSource.Token;

            foreach (var user in PhotonNetwork.PlayerList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var container = await objectPoolService.GetOrCreateView<LobbyRoomUserContainer>(
                    Constants.Views.LobbyRoomUserContainer,
                    userContainerParent,
                    true);

                diContainer.Inject(container);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    break;
                }
                
                userContainers.Add(container);

                if (container != null)
                {
                    container.Set(user);
                }
            }
        }
        
        private void ClearUserContainers()
        {
            userCancellationTokenSource?.Cancel();
            userCancellationTokenSource?.Dispose();
            userCancellationTokenSource = new CancellationTokenSource();

            foreach (var container in userContainers)
            {
                objectPoolService.ReturnToPool(container);
            }

            userContainers.Clear();
        }
        
        private void OnPlayButtonClick()
        {
            eventAggregator.SendEvent(new StartGameplayEvent());
        }
    }
}