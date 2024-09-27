using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;
using UnityEngine;

namespace Gameplay.Player.LootBox
{
    public class PrisonerLootBoxView : PersonalLootBoxView
    {
        protected override void Start()
        {
            title.text = $"Prisoner LootBox id:{photonView.OwnerActorNr}";
            gameplayStage.GameplayDataDic[photonView.OwnerActorNr].LootBoxView = this;
        }
        
        public override bool CanInteract(RoleType roleType)
        {
            return canInteract.Contains(roleType) && photonView.IsMine || roleType == RoleType.Security;
        }
        
        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            if (!CanInteract(role))
            {
                return;
            }

            if (role == RoleType.Prisoner && view.photonView.OwnerActorNr == photonView.OwnerActorNr)
            {
                SuccessfulInteractive();
            }
            else if (role == RoleType.Security)
            {
                SuccessfulInteractive();
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
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.PrisonerLootBoxPopup, response.Data)).Forget();
                });
        }

        protected override void FailedInteractive()
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
        }
    }
}