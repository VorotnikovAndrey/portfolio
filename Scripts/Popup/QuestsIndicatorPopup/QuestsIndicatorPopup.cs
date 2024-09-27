using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Player.Quests;
using UnityEngine;
using Zenject;

namespace PlayVibe.QuestsIndicatorPopup
{
    public class QuestsIndicatorPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;
        
        [Inject] private GameplayStage gameplayStage;
        [Inject] private GameplayController gameplayController;

        private CancellationTokenSource cancellationTokenSource;
        
        private readonly List<QuestsIndicatorView> indicatorViews = new();
        
        protected override UniTask OnShow(object data = null)
        {
            Subscribes();
            Refresh();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            UnSubscribes();
            Clear();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void Subscribes()
        {
            eventAggregator.Add<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }

        private void UnSubscribes()
        {
            eventAggregator.Remove<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }
        
        private void OnQuestsUpdatedEvent(QuestsUpdatedEvent sender)
        {
            Refresh();
        }

        private void Refresh()
        {
            Clear();
            
            cancellationTokenSource = new CancellationTokenSource();
            
            foreach (var questData in gameplayStage.LocalGameplayData.Quests.Values)
            {
                CreateIndicator(cancellationTokenSource.Token, questData).Forget();
            }
        }

        private async UniTask CreateIndicator(CancellationToken token, QuestData questData)
        {
            var view = await objectPoolService.GetOrCreateView<QuestsIndicatorView>(Constants.Views.QuestsIndicatorView, content);

            if (token.IsCancellationRequested)
            {
                objectPoolService.ReturnToPool(view);
                
                return;
            }
            
            diContainer.Inject(view);
            indicatorViews.Add(view);
            
            view.Setup(questData);
            view.gameObject.SetActive(true);
        }

        private void Clear()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            
            foreach (var view in indicatorViews)
            {
                objectPoolService.ReturnToPool(view);
            }
            
            indicatorViews.Clear();
        }
    }
}