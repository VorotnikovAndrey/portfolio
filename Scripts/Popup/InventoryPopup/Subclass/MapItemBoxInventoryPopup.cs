using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using PlayVibe.Subclass.CharacterInventoryPopupElements;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe.Subclass
{
    public class MapItemBoxInventoryPopup : InteractiveInventoryPopup
    {
        [SerializeField] private GameObject otherInventoriesScrollView;
        [SerializeField] private RectTransform otherInventoriesParent;
        [SerializeField] private WaiterController waiter;
        
        [Inject] private ViewsHandler viewsHandler;

        private CancellationTokenSource otherCancellationToken;
        private CompositeDisposable otherCompositeDisposableRefresh;
        private CompositeDisposable otherCompositeDisposableSurcessful;
        private readonly List<OtherInventoriesButton> otherInventoriesButtons = new();

        protected override AbstractInteractiveObject InteractiveObject { get; set; }
        protected override bool DragAllowed => InteractiveObject.CanInteract(gameplayStage.LocalGameplayData.RoleType);
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.MapItemBox;

        protected override UniTask OnShow(object data = null)
        {
            base.OnShow(data);
            
            RefreshOther();

            waiter.Hide(true);
            InputDisabler.Clear();

            return UniTask.CompletedTask;
        }
        
        protected override UniTask OnHide()
        {
            base.OnHide();
            
            otherCompositeDisposableRefresh?.Dispose();
            otherCompositeDisposableRefresh = null;
            
            InputDisabler.Disable();
                
            return UniTask.CompletedTask;
        }
        
        protected override void OnHiden()
        {
            base.OnHiden();
            
            ClearOther();
            
            otherCompositeDisposableSurcessful?.Dispose();
            otherCompositeDisposableSurcessful = null;
        }
        
        protected override void OnInitialized()
        {
            InteractiveObject = viewsHandler.MapItemBoxes.FirstOrDefault(x => x.NetworkKey == popupData.OwnerId);
            Capacity = (InteractiveObject as MapItemboxInteractiveObject).ItemCount;
            
            otherCompositeDisposableRefresh = new CompositeDisposable();
            otherCompositeDisposableSurcessful = new CompositeDisposable();

            BeginObservableOtherInventories();
            BeginObservablePositionHandle();
        }
        
        protected override void Subscribes()
        {
            base.Subscribes();
            
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            
            eventAggregator.Add<NextDayEvent>(OnNextDayEvent);
        }
        
        protected override void UnSubscribes()
        {
            base.UnSubscribes();
            
            eventAggregator.Remove<NextDayEvent>(OnNextDayEvent);
        }
        
        protected override void SetupSlots()
        {
            var index = 0;
            
            slots.ForEach(slot =>
            {
                slot.Initialize(popupData.OwnerId, popupData.InventoryType);
                slot.SetSelectState(false);
                slot.EmitClick.Subscribe(OnSlotHasBeenClicked).AddTo(otherCompositeDisposableSurcessful);
                slot.EmitTrySnap.Subscribe(OnTrySnap).AddTo(otherCompositeDisposableSurcessful);
                slot.gameObject.SetActive(index < Capacity);

                index++;
            });
        }

        protected override void UpdateTitle()
        {
            title.text = $"MapItemBox [id:{popupData.OwnerId}]";
        }
        
        private void OnNextDayEvent(NextDayEvent sender)
        {
            InputDisabler.Disable();
            
            Hide().Forget();
        }
        
        protected void BeginObservableOtherInventories()
        {
            if (InteractiveObject == null)
            {
                Hide().Forget();
                
                return;
            }
            
            Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                RefreshOther();
            }).AddTo(otherCompositeDisposableSurcessful);
        }
        
        protected override void BeginObservablePositionHandle()
        {
            if (InteractiveObject == null)
            {
                Hide().Forget();
                
                return;
            }
            
            var view = gameplayStage.LocalGameplayData.CharacterView as CharacterView;
            var radius = view.InteractiveRadius;

            Observable.EveryUpdate().Where(_ =>
            {
                var colliders = Physics.OverlapSphere(view.Center.position, radius, balance.Interactive.InteractiveLayer, QueryTriggerInteraction.Collide);
                
                return !colliders.Contains(InteractiveObject.InteractiveCollider);
                
            }).Subscribe(_ => Hide().Forget()).AddTo(otherCompositeDisposableSurcessful);
        }

        private void RefreshOther()
        {
            var view = gameplayStage.LocalGameplayData.CharacterView as CharacterView;
            var other = view.GetInteractiveObjects<MapItemboxInteractiveObject>();
            
            if (other.Count <= 1)
            {
                otherInventoriesScrollView.SetActive(false);
                return;
            }
            
            otherInventoriesScrollView.SetActive(true);

            var currentIds = new HashSet<int>();
            var otherIds = new HashSet<int>();
            
            otherInventoriesButtons.ForEach(x => currentIds.Add(x.InteractiveObject.NetworkKey));
            other.ForEach(x => otherIds.Add(x.NetworkKey));
            
            if (currentIds.SetEquals(otherIds))
            {
                return;
            }
                
            ClearOther();
            
            otherCancellationToken = new CancellationTokenSource();
            otherCompositeDisposableRefresh = new CompositeDisposable();
            
            CreateOther(otherCancellationToken.Token, other).Forget();
        }

        private async UniTask CreateOther(CancellationToken token, List<MapItemboxInteractiveObject> array)
        {
            foreach (var interactiveObject in array)
            {
                var view = await objectPoolService.GetOrCreateView<OtherInventoriesButton>(Constants.Views.OtherInventoriesButton, otherInventoriesParent);

                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(view);
                    return;
                }
                
                diContainer.InjectGameObject(view.gameObject);
                otherInventoriesButtons.Add(view);

                view.Setup(interactiveObject);
                view.SetColor(interactiveObject == InteractiveObject);
                view.gameObject.SetActive(true);

                if (otherCompositeDisposableRefresh == null || otherCompositeDisposableRefresh.IsDisposed)
                {
                    otherCompositeDisposableRefresh = new CompositeDisposable();
                }
                
                view.EmitClick.Subscribe(OnOtherButtonClick).AddTo(otherCompositeDisposableRefresh);
            }
        }

        private void OnOtherButtonClick(OtherInventoriesButton button)
        {
            if (button.InteractiveObject == InteractiveObject)
            {
                return;
            }
            
            waiter.Show();
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.GetMapItemBoxItemsRequest,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                button.InteractiveObject.NetworkKey,
                response =>
                {
                    if (State != PopupState.Shown)
                    {
                        return;
                    }
                    
                    otherCompositeDisposableSurcessful?.Dispose();
                    otherCompositeDisposableSurcessful = new CompositeDisposable();
                    
                    popupData = response.Data as InventoryPopupData;
                    InteractiveObject = viewsHandler.MapItemBoxes.FirstOrDefault(x => x.NetworkKey == popupData.OwnerId);
                    Capacity = (InteractiveObject as MapItemboxInteractiveObject).ItemCount;
                    
                    UpdateTitle();
                    SetupSlots();
                    BeginObservablePositionHandle();
                    BeginObservableOtherInventories();
                    
                    foreach (var element in otherInventoriesButtons)
                    {
                        element.SetColor(element.InteractiveObject == InteractiveObject);
                    }
                    
                    cancellationTokenSource?.Dispose();
                    cancellationTokenSource = new CancellationTokenSource();
            
                    Refresh(cancellationTokenSource.Token, GetItemsFromSource()).Forget();
                    
                    waiter.Hide();
                });
        }

        private void ClearOther()
        {
            otherCompositeDisposableRefresh?.Dispose();
            otherCancellationToken?.Dispose();
            otherInventoriesButtons.ForEach(x => objectPoolService.ReturnToPool(x));
            otherInventoriesButtons.Clear();
        }
    }
}