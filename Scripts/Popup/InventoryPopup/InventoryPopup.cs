using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using Services;
using Services.Gameplay;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public abstract class InventoryPopup : AbstractBasePopup
    {
        [SerializeField] protected List<InventorySlot> slots;

        [Inject] protected GameplayStage gameplayStage;
        [Inject] protected ItemsSettings itemsSettings;
        [Inject] protected GameplayController gameplayController;
        [Inject] protected Balance balance;
        [Inject] protected ItemTransitionService itemTransitionService;
        [Inject] protected PopupService popupService;

        protected InventoryPopupData popupData;
        protected CancellationTokenSource cancellationTokenSource;
        
        protected abstract bool DragAllowed { get; }
        protected abstract int Capacity { get; set; }
        protected abstract InventoryType InventoryType { get; }

        public InventoryPopupData PopupData => popupData;

        protected override UniTask OnShow(object data = null)
        {
            if (data is not InventoryPopupData castData)
            {
                Hide().Forget();
                
                return UniTask.CompletedTask;
            }

            popupData = castData;
            
            OnInitialized();
            SetupSlots();
            
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            
            Refresh(cancellationTokenSource.Token, GetItemsFromSource()).Forget();
            
            Subscribes();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            ReturnAllItemToPrevPosition();
                
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            
            UnSubscribes();
            ClearItems();
        }
        
        protected virtual void Subscribes()
        {
            eventAggregator.Add<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
        }

        protected virtual void UnSubscribes()
        {
            eventAggregator.Remove<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
        }

        protected abstract void OnInitialized();

        protected abstract IEnumerable<ItemModel> GetItemsFromSource();

        private void OnUpdateInventoryItemsEvent(UpdateInventoryItemsEvent sender)
        {
            if (sender.PopupData == null || popupData == null ||
                sender.PopupData.InventoryType != popupData.InventoryType ||
                sender.PopupData.OwnerId != popupData.OwnerId)
            {
                return;
            }
            
            popupData = sender.PopupData;

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
                
            Refresh(cancellationTokenSource.Token, popupData.Items).Forget();
        }
        
        protected virtual void SetupSlots()
        {
            var index = 0;
            
            slots.ForEach(slot =>
            {
                slot.Initialize(popupData.OwnerId, popupData.InventoryType);
                slot.SetSelectState(false);
                slot.EmitClick.Subscribe(OnSlotHasBeenClicked).AddTo(CompositeDisposable);
                slot.EmitTrySnap.Subscribe(OnTrySnap).AddTo(CompositeDisposable);
                slot.gameObject.SetActive(index < Capacity);

                index++;
            });
        }

        protected virtual void OnSlotHasBeenClicked(ItemTransitionData itemTransitionData)
        {
            var requestData = itemTransitionService.CreateClickTransitionRequestData(itemTransitionData);

            if (requestData == null)
            {
                return;
            }
            
            ApplyTransition(itemTransitionData, requestData);
        }

        protected virtual void OnTrySnap(ItemTransitionData itemTransitionData)
        {
            ApplyTransition(itemTransitionData, new ItemTransitionRequestData
            {
                ItemModel = itemTransitionData.ItemView.ItemModel,
                FromInventoryType = itemTransitionData.FromSlot.InventoryType,
                ToInventoryType = itemTransitionData.ToSlot.InventoryType,
                FromSlot = itemTransitionData.FromSlot.Index,
                ToSlot = itemTransitionData.ToSlot.Index,
                FromNetworkId = itemTransitionData.FromSlot.OwnerId,
                ToNetworkId = itemTransitionData.ToSlot.OwnerId
            });
        }

        protected virtual bool AdditiveCheckApplyTransition(ItemTransitionData itemTransitionData, ItemTransitionRequestData requestTransitionData)
        {
            return true;
        }

        protected virtual void ApplyTransition(ItemTransitionData itemTransitionData, ItemTransitionRequestData requestTransitionData)
        {
            if (!AdditiveCheckApplyTransition(itemTransitionData, requestTransitionData))
            {
                itemTransitionData.ItemView.ReturnPrevPosition();
                
                return;
            }
            
            var result = itemTransitionService.FakeTransition(itemTransitionData);

            if (result != ItemTransitionResult.Successfully)
            {
                itemTransitionData.ItemView.ReturnPrevPosition();
                
                return;
            }
            
            OnRefreshed();
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.TransitionItem,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                requestTransitionData,
                response =>
                {
                    if (response.Data is not ItemTransitionResult responseTransitionResult)
                    {
                        return;
                    }

                    if (responseTransitionResult == ItemTransitionResult.Successfully)
                    {
                        return;
                    }
                    
                    cancellationTokenSource?.Dispose();
                    cancellationTokenSource = new CancellationTokenSource();
                
                    Refresh(cancellationTokenSource.Token, popupData.Items).Forget();
                });
        }

        protected virtual async UniTask Refresh(CancellationToken token, IEnumerable<ItemModel> items)
        {
            ClearItems();
            
            foreach (var item in items)
            {
                var slotParent = slots[item.Slot];
                var view = await objectPoolService.GetOrCreateView<ItemView>(Constants.Views.ItemView, slotParent.SlotRect, true);
                
                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(view);
                    return;
                }
                
                diContainer.Inject(view);
                
                view.Setup(item, DragAllowed, InventoryType);
                slotParent.TrySnap(view);
            }

            OnRefreshed();
        }

        protected virtual void OnRefreshed()
        {
            
        }

        public ISlot GetSlot(int index)
        {
            return slots.FirstOrDefault(x => x.Index == index);
        }
        
        protected void ClearItems()
        {
            slots.ForEach(slot =>
            {
                slot.DropItem();
            });
        }

        public ISlot GetFirstFreeSlot()
        {
            return slots.FirstOrDefault(x => x.CurrentItem == null && x.Index < Capacity);
        }

        public void ReturnAllItemToPrevPosition()
        {
            slots.ForEach(x => x.CurrentItem?.InterruptDrag());
        }
        
        protected void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }
    }
}