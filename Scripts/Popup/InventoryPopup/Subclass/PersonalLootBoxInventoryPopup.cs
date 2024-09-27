using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using PlayVibe.RolePopup;
using Services.Gameplay.Wallet;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.Subclass
{
    public abstract class PersonalLootBoxInventoryPopup : InteractiveInventoryPopup
    {
        [SerializeField] protected Button upgradeButton;
        [SerializeField] protected TextMeshProUGUI upgradePriceText;
        
        protected override AbstractInteractiveObject InteractiveObject { get; set; }
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.LootBox;
        
        protected override void Subscribes()
        {
            base.Subscribes();

            gameplayStage.GameplayDataDic[popupData.OwnerId].Wallet.HasChanged.Subscribe(_ => UpdatePriceText()).AddTo(CompositeDisposable);
            upgradeButton.OnClickAsObservable().Subscribe(_ => OnUpgradeButtonClick()).AddTo(CompositeDisposable);
            
            eventAggregator.Add<PersonalLootBoxUpgradedEvent>(OnPersonalLootBoxUpgradedEvent);
        }

        protected override void UnSubscribes()
        {
            base.UnSubscribes();
            
            eventAggregator.Remove<PersonalLootBoxUpgradedEvent>(OnPersonalLootBoxUpgradedEvent);
        }
        
        protected override void OnInitialized()
        {
            var owner = gameplayStage.GameplayDataDic[popupData.OwnerId];
            InteractiveObject = owner.LootBoxView;
            Capacity = owner.RoleType == RoleType.Prisoner
                ? balance.Inventory.PrisonerLootBoxCapacity
                : balance.Inventory.SecurityLootBoxCapacity;
            
            UpdatePriceText();
            UpdateUpgradeState();
        }

        protected override void UpdateTitle()
        {
            var isMine = InteractiveObject.photonView.OwnerActorNr == PhotonNetwork.LocalPlayer.ActorNumber;

            if (isMine)
            {
                var upgradeState = gameplayStage.LocalGameplayData.LootBoxUpgraded ? "upgraded " : string.Empty;
                title.text = $"{gameplayStage.GameplayDataDic[InteractiveObject.photonView.OwnerActorNr].RoleType} LootBox {upgradeState}[id:{popupData.OwnerId}]";
            }
            else
            {
                title.text = $"{gameplayStage.GameplayDataDic[InteractiveObject.photonView.OwnerActorNr].RoleType} LootBox [id:{popupData.OwnerId}]";
            }
        }
        
        protected abstract void UpdateUpgradeState(bool updated = false);
        
        protected void OnUpgradeButtonClick()
        {
            var data = gameplayStage.GameplayDataDic[popupData.OwnerId];
            var price = data.RoleType == RoleType.Prisoner
                ? balance.Wallet.PrisonerUpgradeLootBoxPrice
                : balance.Wallet.SecurityUpgradeLootBoxPrice;
            
            gameplayController.GetEventHandler<UpgradeNetworkEventHandler>().SendTryUpgradeItemBox(new WalletData
            {
                ActorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
                Amount = price,
                CurrencyType = CurrencyType.Soft
            });
        }
        
        protected void OnPersonalLootBoxUpgradedEvent(PersonalLootBoxUpgradedEvent sender)
        {
            if (popupData.OwnerId != sender.Data.ActorNumber)
            {
                return;
            }
            
            UpdateUpgradeState(true);
        }
        
        protected void UpdatePriceText()
        {
            var data = gameplayStage.GameplayDataDic[popupData.OwnerId];
            var price = data.RoleType == RoleType.Prisoner
                ? balance.Wallet.PrisonerUpgradeLootBoxPrice
                : balance.Wallet.SecurityUpgradeLootBoxPrice;
            var hasCurrency = data.Wallet.Has(CurrencyType.Soft, price);

            upgradeButton.interactable = hasCurrency;
            upgradePriceText.text = price.ToString();
        }
    }
}