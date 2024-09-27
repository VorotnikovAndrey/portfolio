    using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using PlayVibe.Subclass;
using Services.Gameplay.Craft;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public sealed class SelfCraftPopup : AbstractBasePopup
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button craftButton;

        [Inject] private CraftBank craftBank;
        [Inject] private PopupService popupService;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;
        [Inject] private GameplayController gameplayController;

        private string selectedKey;
        private CompositeDisposable containersCompositeDisposable;
        private CancellationTokenSource cancellationTokenSource;
        private Tweener craftTweener;
        private InventoryPopupData lastPopupData;
        private SelfCraftContainer CurrentCraftContainer;
        
        private readonly List<SelfCraftContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is SelfCraftPopupData popupData)
            {
                selectedKey = popupData.ItemKey;
            }
            else
            {
                selectedKey = string.Empty;
            }
            
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.InSelfCraftPopup);
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Tab)).Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);

            ScrollToStart();
            
            craftButton.interactable = false;
            craftButton.OnClickAsObservable().Subscribe(_ => OnCraftButtonClick()).AddTo(CompositeDisposable);
            
            var inventory = popupService.GetPopups<CharacterInventoryPopup>(Constants.Popups.Inventory.CharacterInventoryPopup).FirstOrDefault();

            if (inventory == null)
            {
                Hide().Forget();
                
                return UniTask.CompletedTask;
            }
            
            Refresh(inventory.PopupData);
            Subscribes();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.InSelfCraftPopup);
            popupService.TryHidePopup(Constants.Popups.QuestsPopup).Forget();
            
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
            eventAggregator.Add<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
        }

        private void UnSubscribes()
        {
            eventAggregator.Remove<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
        }
        
        private void OnUpdateInventoryItemsEvent(UpdateInventoryItemsEvent sender)
        {
            if (sender.PopupData.InventoryType != InventoryType.Character || sender.PopupData.OwnerId != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            Refresh(sender.PopupData);
        }

        private void Refresh(InventoryPopupData data)
        {
            Clear();
            
            cancellationTokenSource = new CancellationTokenSource();
            Create(cancellationTokenSource.Token, data).Forget();
        }

        private async UniTask Create(CancellationToken token, InventoryPopupData data)
        {
            if (data == null)
            {
                return;
            }

            lastPopupData = data;
            containersCompositeDisposable = new CompositeDisposable();
            
            var itemsDictionary = data.Items
                .Select(x => x.ItemKey)
                .Where(key => !string.IsNullOrEmpty(key))
                .GroupBy(key => key)
                .ToDictionary(g => g.Key, g => g.Count());
            
            foreach (var pair in craftBank.Data)
            {
                var container = await objectPoolService.GetOrCreateView<SelfCraftContainer>(Constants.Views.SelfCraftContainer, content, true);

                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }
            
                diContainer.Inject(container);
            
                containers.Add(container);
            
                container.Setup(pair);
                
                var allElementsExist = pair.ComponentsKeys
                    .Where(key => !string.IsNullOrEmpty(key))
                    .GroupBy(key => key)
                    .All(g => itemsDictionary.TryGetValue(g.Key, out var count) && count >= g.Count());
                
                container.SetState(allElementsExist);
                container.SetSelected(container.CurrentPair.ItemKey == selectedKey);
                container.SetProgress01(0);
                container.Button.OnClickAsObservable().Subscribe(_ =>
                {
                    selectedKey = container.CurrentPair.ItemKey;
                    UpdateSelected();
                }).AddTo(containersCompositeDisposable);
            }

            UpdateSelected();
            SortAndReorderContainers();

            CurrentCraftContainer = null;
        }

        private void UpdateSelected()
        {
            foreach (var container in containers)
            {
                container.SetSelected(container.CurrentPair.ItemKey == selectedKey);
            }
            
            var checkKey = containers.FirstOrDefault(x => x.CurrentPair.ItemKey == selectedKey)?.State ?? false;

            craftButton.interactable = checkKey && CurrentCraftContainer== null;
        }

        public void SelectContainer(string key)
        {
            selectedKey = key;

            UpdateSelected();
        }
        
        private void Clear()
        {
            StopCrafting();
            
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            containersCompositeDisposable?.Dispose();
            containersCompositeDisposable = null;
            
            foreach (var container in containers)
            {
                objectPoolService.ReturnToPool(container);
            }
            
            containers.Clear();
        }
        
        private void OnCraftButtonClick()
        {
            if (CurrentCraftContainer != null)
            {
                return;
            }
            
            CurrentCraftContainer = containers.FirstOrDefault(x => x.CurrentPair.ItemKey == selectedKey);

            if (CurrentCraftContainer == null)
            {
                return;
            }

            craftButton.interactable = false;
            
            CurrentCraftContainer.SetState(false);
            CurrentCraftContainer.SetProgress01(0);
            
            craftTweener?.Kill();
            craftTweener = DOTween.To(() => 0f, x => CurrentCraftContainer.SetProgress01(x), 1f, balance.Inventory.SelfCraftDuration).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    CurrentCraftContainer.SetProgress01(0);
                    selectedKey = string.Empty;
                    
                    var array = new List<int>();
                    
                    foreach (var requiredItem in CurrentCraftContainer.CurrentPair.ComponentsKeys)
                    {
                        foreach (var item in lastPopupData.Items)
                        {
                            if (item.ItemKey != requiredItem)
                            {
                                continue;
                            }

                            if (array.Contains(item.NetworkId))
                            {
                                continue;
                            }
                            
                            array.Add(item.NetworkId);
                            
                            break;
                        }
                    }
                    
                    gameplayController.GetEventHandler<CraftNetworkEventHandler>().SendTryCraftItem(new CraftNetworkData
                    {
                        Owner = gameplayStage.LocalGameplayData.ActorNumber,
                        ItemKey = CurrentCraftContainer.CurrentPair.ItemKey,
                        ComponentsNetworkId = array
                    });

                    CurrentCraftContainer = null;
                });
        }
        
        public void StopCrafting()
        {
            craftTweener?.Kill();
            craftTweener = null;
        }
        
        private void ScrollToStart()
        {
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.horizontalNormalizedPosition = 0f;
        }
        
        public void SortAndReorderContainers()
        {
            var sortedContainers = containers.OrderBy(container => container.State).ToList();

            foreach (var container in sortedContainers)
            {
                if (container.State)
                {
                    container.transform.SetAsFirstSibling();
                }
                else
                {
                    container.transform.SetAsLastSibling();
                }
            }
        }
    }
}