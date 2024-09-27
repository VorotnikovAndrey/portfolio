using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Network.NetworkEventHandlers;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class GameplayReadyPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private TextMeshProUGUI timerText;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;

        private DateTime endTime;
        private CancellationTokenSource cancellationTokenSource;

        private readonly CompositeDisposable timerCompositeDisposable = new();
        private readonly List<GameplayReadyContainer> containers = new();

        public bool Ready { get; private set; }

        protected override UniTask OnShow(object data = null)
        {
            Subscribes();
            StartTimer();
            
            cancellationTokenSource = new CancellationTokenSource();
            Create(cancellationTokenSource.Token).Forget();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            timerCompositeDisposable?.Dispose();
            cancellationTokenSource?.Dispose();
            
            UnSubscribes();

            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            Clear();
        }
        
        private void Subscribes()
        {
            eventAggregator.Add<UpdateReadyPopupEvent>(OnUpdateReadyPopupEvent);
        }

        private void UnSubscribes()
        {
            eventAggregator.Remove<UpdateReadyPopupEvent>(OnUpdateReadyPopupEvent);
        }
        
        private void OnUpdateReadyPopupEvent(UpdateReadyPopupEvent sender)
        {
            foreach (var container in containers)
            {
                container.SetStatus(gameplayStage.GameplayDataDic[container.ActorNumber].ReadyStatus);
            }
        }

        public async UniTask Create(CancellationToken token)
        {
            foreach (var data in gameplayStage.GameplayDataDic)
            {
                var container = await objectPoolService.GetOrCreateView<GameplayReadyContainer>(Constants.Views.GameplayReadyContainer, content);
                
                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }

                containers.Add(container);
                
                container.SetActor(data.Value.ActorNumber);
                container.SetNickname(data.Value.Nickname + " " + $"id:{data.Value.ActorNumber}");
                container.SetStatus(data.Value.ReadyStatus);
                container.gameObject.SetActive(true);
            }

            Ready = true;
        }

        private void Clear()
        {
            containers.ForEach(container => objectPoolService.ReleaseView(container));
            containers.Clear();
        }
        
        private void StartTimer()
        {
            endTime = DateTime.UtcNow + TimeSpan.FromSeconds(balance.Main.LoadLevelMaxTime);
            
            UpdateTimer();
            
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => UpdateTimer()).AddTo(timerCompositeDisposable);
        }

        private void UpdateTimer()
        {
            var remainingTime = endTime - DateTimeOffset.UtcNow;

            timerText.text = $"{remainingTime:mm\\:ss}";

            if (remainingTime > TimeSpan.Zero)
            {
                return;
            }

            TimeOut();
        }

        private void TimeOut()
        {
            timerCompositeDisposable?.Dispose();
            InputDisabler.Disable();
            timerText.text = string.Empty;
            gameplayStage.ReturnToLobby();
        }
    }
}