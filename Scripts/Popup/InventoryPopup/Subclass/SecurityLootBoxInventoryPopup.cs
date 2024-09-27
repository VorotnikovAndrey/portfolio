using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Inventory;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using UniRx;

namespace PlayVibe.Subclass
{
    public class SecurityLootBoxInventoryPopup : PersonalLootBoxInventoryPopup
    {
        protected override bool DragAllowed => true;
        protected override InventoryType InventoryType => InventoryType.LootBox;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ConnectToCharacterInventoryPopup();
        }
        
        protected override void UpdateUpgradeState(bool updated = false)
        {
            var isMine = popupData.OwnerId == PhotonNetwork.LocalPlayer.ActorNumber;
            
            if (isMine)
            {
                var state = gameplayStage.GameplayDataDic[popupData.OwnerId].LootBoxUpgraded;
            
                upgradeButton.gameObject.SetActive(!state);
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
            }

            if (!updated)
            {
                return;
            }
            
            if (isMine)
            {
                UpdateTitle();
            }
            else
            {
                Hide(true).Forget();
            }
        }
        
        private void ConnectToCharacterInventoryPopup()
        {
            if (popupData.OwnerId != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            var popups = popupService.GetPopups<CharacterInventoryPopup>(Constants.Popups.Inventory.CharacterInventoryPopup);

            foreach (var popup in popups)
            {
                popup.SecuritySlot.Button.OnClickAsObservable().Subscribe(_ =>
                {
                    gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRandomSeizedItemToLootBox(popupData.OwnerId);
                }).AddTo(CompositeDisposable);
            }
        }
    }
}