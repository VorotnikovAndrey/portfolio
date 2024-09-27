using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;
using UnityEngine;

namespace Gameplay.Player.LootBox
{
    public class SecurityLootBoxView : PersonalLootBoxView
    {
        protected override void Start()
        {
            title.text = $"Prisoner LootBox id:{photonView.OwnerActorNr}";
            gameplayStage.GameplayDataDic[photonView.OwnerActorNr].LootBoxView = this;
        }
        
        public override bool CanInteract(RoleType roleType)
        {
            return canInteract.Contains(roleType) && (photonView.IsMine || roleType == RoleType.Prisoner);
        }
        
        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;

            if (!CanInteract(role))
            {
                return;
            }

            if (role == RoleType.Security && view.photonView.OwnerActorNr == photonView.OwnerActorNr)
            {
                SuccessfulInteractive();
            }
            else if (role == RoleType.Prisoner)
            {
                if (gameplayStage.GameplayDataDic[photonView.OwnerActorNr].LootBoxUpgraded)
                {
                    gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                        PhotonPeerEvents.HasItem,
                        new RaiseEventOptions
                        {
                            Receivers = ReceiverGroup.MasterClient
                        },
                        new GetItemByTypeNetworkData
                        {
                            Owner = gameplayStage.LocalGameplayData.ActorNumber,
                            InventoryType = InventoryType.Character,
                            ItemType = "Lockpick"
                        },
                        response =>
                        {
                            if (response.Data is ItemModel itemModel)
                            {
                                gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRemoveItem(
                                    gameplayStage.LocalGameplayData.ActorNumber,
                                    InventoryType.Character,
                                    itemModel);
                                
                                SuccessfulInteractive();
                            }
                            else
                            {
                                FailedInteractive();
                            }
                        },
                        FailedInteractive);
                }
                else
                {
                    SuccessfulInteractive();
                }
            }
            else
            {
                FailedInteractive();
            }
        }
        
        protected override void SuccessfulInteractive()
        {
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.ShowPersonalLootBoxRequest,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                photonView.OwnerActorNr,
                response =>
                {   
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.SecurityLootBoxPopup, response.Data)).Forget();
                });
        }

        protected override void FailedInteractive()
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
        }
    }
}