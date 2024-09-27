using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Inventory;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    public class TradePopup : InventoryPopup
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [Space]
        [SerializeField] private Image localLocker;
        [SerializeField] private Image otherLocker;
        [SerializeField] private Color confirmColor;
        [SerializeField] private Color cancelColor;
        [Space]
        [SerializeField] private TextMeshProUGUI localNicknameText;
        [SerializeField] private TextMeshProUGUI otherNicknameText;
        [SerializeField] private GameObject inputLocker;
        [Space] 
        [SerializeField] private StateSwitcher statusStateSwitcher;
        [Space]
        [SerializeField] private List<TradeOtherSlot> otherSlots;
        
        private int syncActor;
        private bool isLocalConfirm;
        private bool isOtherConfirm;
        private TradeResult tradeResult;
        private InventoryNetworkEventHandler inventoryNetworkEventHandler;
        private CancellationTokenSource tryConfirmCancellationTokenSource;
        
        protected override bool DragAllowed => true;
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.Trade;
        
        protected override UniTask OnShow(object data = null)
        {
            popupService.TryHidePopup(Constants.Popups.TradeWaitPopup).Forget();
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.InTrade);
            
            inventoryNetworkEventHandler = gameplayController.GetEventHandler<InventoryNetworkEventHandler>();
            tradeResult = TradeResult.Failed;
            localNicknameText.text = string.Empty;
            otherNicknameText.text = string.Empty;
            localLocker.gameObject.SetActive(false);
            otherLocker.gameObject.SetActive(false);
            otherSlots.ForEach(x => x.Clear());
            isLocalConfirm = false;
            isOtherConfirm = false;
            inputLocker.SetActive(false);
            
            base.OnShow(data);
            
            confirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(CompositeDisposable);
            cancelButton.OnClickAsObservable().Subscribe(_ => OnCancelButtonClick()).AddTo(CompositeDisposable);

            UpdateOtherStatus(TradeStatus.Wait);
            Subscribes();
            
            InputDisabler.Clear();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.InTrade);
            
            tryConfirmCancellationTokenSource?.Cancel();
            tryConfirmCancellationTokenSource = null;
            
            InputDisabler.Disable();
            
            base.OnHide();
            
            UnSubscribes();
            
            if (tradeResult == TradeResult.Successful)
            {
                inventoryNetworkEventHandler.SendSuccessfulTrade(syncActor);
            }
            else
            {
                inventoryNetworkEventHandler.SendInterruptTrade(syncActor);
                inventoryNetworkEventHandler.SendCancelTrade();
            }
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        protected override void OnInitialized()
        {
            Capacity = slots.Count;
        }
        
        protected override void OnRefreshed()
        {
            base.OnRefreshed();
            
            inventoryNetworkEventHandler.RequestGetAndSendMyTradeItems(syncActor);
        }

        protected override IEnumerable<ItemModel> GetItemsFromSource()
        {
            return popupData.Items;
        }

        private void UpdateOtherStatus(TradeStatus status)
        {
            statusStateSwitcher.Set(status);
        }

        public void Sync(int targetActorNumber)
        {
            syncActor = targetActorNumber;
            
            localNicknameText.text = $"{PhotonNetwork.NickName} [{gameplayStage.LocalGameplayData.ActorNumber}]";

            var targetData = gameplayStage.GameplayDataDic[targetActorNumber];
            otherNicknameText.text = $"{targetData.Nickname} [{targetData.ActorNumber}]";

            BeginObservablePositionHandle();
        }

        private void OnConfirmButtonClick()
        {
            confirmButton.interactable = false;
            WaitTryConfirmBehavior().Forget();
        }

        private async UniTask WaitTryConfirmBehavior()
        {
            tryConfirmCancellationTokenSource?.Cancel();
            tryConfirmCancellationTokenSource = new CancellationTokenSource();
            
            var result = await inventoryNetworkEventHandler.SendTryConfirmTrade(syncActor, tryConfirmCancellationTokenSource.Token);

            if (result)
            {
                isLocalConfirm = true;
                localLocker.gameObject.SetActive(true);
                localLocker.color = confirmColor;
                inputLocker.SetActive(true);
            
                CheckSuccessfulTrade();
            }
            else
            {
                confirmButton.interactable = true;

                ShowInfoPopup(string.Format(Constants.Messages.Trade.DontHasPlace, gameplayStage.GameplayDataDic[syncActor].Nickname));
            }
        }

        private void OnCancelButtonClick()
        {
            isLocalConfirm = false;
            tradeResult = TradeResult.Failed;
            
            Hide().Forget();
        }
        
        public void TryInterrupt(int actorNumber)
        {
            if (syncActor != actorNumber)
            {
                return;
            }

            isOtherConfirm = false;
            
            otherLocker.gameObject.SetActive(true);
            otherLocker.color = cancelColor;
            
            UpdateOtherStatus(TradeStatus.Cancel);
            OnCancelButtonClick();
        }

        public void SyncConfirm(int actorNumber)
        {
            if (syncActor != actorNumber)
            {
                return;
            }
            
            isOtherConfirm = true;
            
            otherLocker.gameObject.SetActive(true);
            otherLocker.color = confirmColor;
            
            UpdateOtherStatus(TradeStatus.Confirm);
            CheckSuccessfulTrade();
        }

        public void TryUpdateItems(int actorNumber, List<ItemModel> data)
        {
            if (syncActor != actorNumber)
            {
                return;
            }

            otherSlots.ForEach(x => x.Clear());

            if (data.IsNullOrEmpty())
            {
                return;
            }

            foreach (var itemModel in data)
            {
                var slot = otherSlots.FirstOrDefault(x => x.Index == itemModel.Slot);

                if (slot == null)
                {
                    continue;
                }
                
                slot.Setup(itemModel);
            }
        }

        private void CheckSuccessfulTrade()
        {
            if (isLocalConfirm == false || isOtherConfirm == false)
            {
                return;
            }

            tradeResult = TradeResult.Successful;
            
            Hide().Forget();
        }
        
        protected void BeginObservablePositionHandle()
        {
            var view = gameplayStage.LocalGameplayData.CharacterView;
            var target = gameplayStage.GameplayDataDic[syncActor].CharacterView;
            
            Observable.EveryUpdate().Where(_ =>
            {
                if (view == null || target == null)
                {
                    return true;
                }
                
                return Vector3.Distance(view.transform.position, target.transform.position) > balance.Interactive.MaxTradeDistance;
            }).Subscribe(_ =>
            {
                OnCancelButtonClick();
            }).AddTo(CompositeDisposable);
        }
        
        protected override void OnTrySnap(ItemTransitionData itemTransitionData)
        {
            if (isLocalConfirm)
            {
                itemTransitionData.ItemView.ReturnPrevPosition();
                return;
            }
            
            base.OnTrySnap(itemTransitionData);
        }
        
        protected override void OnSlotHasBeenClicked(ItemTransitionData itemTransitionData)
        {
            if (isLocalConfirm)
            {
                itemTransitionData.ItemView.ReturnPrevPosition();
                return;
            }
            
            base.OnSlotHasBeenClicked(itemTransitionData);
        }
    }
}