using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.Spells;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class SpellShopPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private Button hideButton;

        [Inject] private SpellsSettings spellsSettings;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private GameplayController gameplayController;

        private CompositeDisposable containersCompositeDisposable;
        private CancellationTokenSource cancellationTokenSource;

        private readonly List<SpellShopContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);

            Clear();
            
            cancellationTokenSource = new CancellationTokenSource();
            containersCompositeDisposable = new CompositeDisposable();
            
            Create(cancellationTokenSource.Token).Forget();

            Subscribes();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
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
            eventAggregator.Add<AddSpellEvent>(OnAddSpellEvent);

            gameplayStage.LocalGameplayData.Wallet.HasChanged.Subscribe(_ =>
            {
                containers.ForEach(x => x.Refresh());
            }).AddTo(CompositeDisposable);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<AddSpellEvent>(OnAddSpellEvent);
        }

        private async UniTask Create(CancellationToken token)
        {
            foreach (var spellData in spellsSettings.Data)
            {
                if (!spellData.AvailableFor.Contains(gameplayStage.LocalGameplayData.RoleType))
                {
                    continue;
                }
                
                var container = await objectPoolService.GetOrCreateView<SpellShopContainer>(Constants.Views.SpellShopContainer, content);

                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }
            
                containers.Add(container);
                diContainer.Inject(container);

                container.Setup(spellData);
                container.OnClick.Subscribe(HandleContainerClick).AddTo(containersCompositeDisposable);
                container.gameObject.SetActive(true);
            }
        }

        private void HandleContainerClick(SpellShopContainer container)
        {
            gameplayController.GetEventHandler<SpellsNetworkEventHandler>().SendTryBuySpell(container.CurrentSpellData.SpellType);
        }
        
        private void OnAddSpellEvent(AddSpellEvent sender)
        {
            if (sender.Data.ActorNumber != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }

            foreach (var container in containers)
            {
                if (container.CurrentSpellData.SpellType != sender.Data.SpellType)
                {
                    continue;
                }
                
                container.Refresh();
                    
                break;
            }
        }

        private void Clear()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            containersCompositeDisposable?.Dispose();
            containersCompositeDisposable = null;
            containers.ForEach(container => objectPoolService.ReturnToPool(container));
            containers.Clear();
        }
    }
}