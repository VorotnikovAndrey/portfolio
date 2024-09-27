using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe.QuestsPopup
{
    public class QuestsPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private PopupService popupService;
        
        private CancellationTokenSource cancellationTokenSource;

        private readonly List<QuestContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            Subscribes();
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Tab)).Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            
            cancellationTokenSource = new CancellationTokenSource();
            Create(cancellationTokenSource.Token).Forget();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            popupService.TryHidePopup(Constants.Popups.SelfCraftPopup).Forget();
            
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
        
        protected void Subscribes()
        {
            eventAggregator.Add<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }
        
        private void OnQuestsUpdatedEvent(QuestsUpdatedEvent sender)
        {
            Clear();

            cancellationTokenSource = new CancellationTokenSource();
            Create(cancellationTokenSource.Token).Forget();
        }

        private async UniTask Create(CancellationToken token)
        {
            foreach (var questData in gameplayStage.LocalGameplayData.Quests.Values)
            {
                var view = await objectPoolService.GetOrCreateView<QuestContainer>(Constants.Views.QuestContainer, content, true);
                
                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(view);
                    return;
                }
                
                diContainer.InjectGameObject(view.gameObject);
                
                containers.Add(view);

                view.Setup(questData);
            }
        }

        private void Clear()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            foreach (var container in containers)
            {
                objectPoolService.ReturnToPool(container);
            }
            
            containers.Clear();
        }
    }
}