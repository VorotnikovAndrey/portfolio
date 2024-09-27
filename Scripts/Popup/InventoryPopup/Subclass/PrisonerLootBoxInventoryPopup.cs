using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using PlayVibe.RolePopup;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.Subclass
{
    public class PrisonerLootBoxInventoryPopup : PersonalLootBoxInventoryPopup
    {
        [SerializeField] private GameObject hidenSlotParent;
        [SerializeField] private InventorySlot secretSlot;
        [SerializeField] private Button confiscateButton;
        
        protected override bool DragAllowed => gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner;

        public InventorySlot SecretSlot => secretSlot;
        protected override InventoryType InventoryType => InventoryType.LootBox;
        
        protected override void SetupSlots()
        {
            base.SetupSlots();
            
            secretSlot.Initialize(popupData.OwnerId, InventoryType.Secret);
            secretSlot.SetSelectState(false);
            secretSlot.EmitClick.Subscribe(OnSlotHasBeenClicked).AddTo(CompositeDisposable);
            secretSlot.EmitTrySnap.Subscribe(OnTrySnap).AddTo(CompositeDisposable);
            secretSlot.gameObject.SetActive(true);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            confiscateButton.gameObject.SetActive(gameplayStage.LocalGameplayData.RoleType == RoleType.Security);
            confiscateButton.OnClickAsObservable().Subscribe(_ => OnConfiscateButtonClick()).AddTo(CompositeDisposable);
        }

        protected override void UpdateUpgradeState(bool updated = false)
        {
            var isMine = popupData.OwnerId == PhotonNetwork.LocalPlayer.ActorNumber;
            
            if (isMine)
            {
                var state = gameplayStage.GameplayDataDic[popupData.OwnerId].LootBoxUpgraded;
            
                upgradeButton.gameObject.SetActive(!state);
                hidenSlotParent.SetActive(state);
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
                hidenSlotParent.SetActive(false);
            }

            if (updated)
            {
                UpdateTitle();
            }
        }

        protected override async UniTask Refresh(CancellationToken token, IEnumerable<ItemModel> items)
        {
            await base.Refresh(token, items);

            confiscateButton.interactable = HasProhibitedItems();
        }
        
        private void OnConfiscateButtonClick()
        {
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendConfiscateInventory(new ConfiscateInventoryData
            {
                ActorNumber = popupData.OwnerId,
                InventoryType = popupData.InventoryType
            });
        }

        private bool HasProhibitedItems()
        {
            foreach (var item in popupData.Items)
            {
                if (itemsSettings.Data[item.ItemKey].Classification == ItemClassification.Prohibited)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        protected override void OnSlotHasBeenClicked(ItemTransitionData itemTransitionData)
        {
            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Security)
            {
                return;
            }
            
            var requestData = itemTransitionService.CreateClickTransitionRequestData(itemTransitionData);

            if (requestData == null)
            {
                return;
            }
            
            ApplyTransition(itemTransitionData, requestData);
        }
    }
}