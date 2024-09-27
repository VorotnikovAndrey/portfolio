using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using Services;
using UnityEngine;

namespace PlayVibe
{
    public class DropInteractiveObject : AbstractInteractiveObject
    {
        public override int NetworkKey => photonView.ViewID;
        
        public override void TryInteractive(CharacterView view)
        {
            transform.localEulerAngles = new Vector3(0, Random.Range(0, 180), 0);
            
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            if (!CanInteract(role))
            {
                FailedInteractive(view);
                return;
            }

            SuccessfulInteractive(view);
        }
        
        private void SuccessfulInteractive(CharacterView view)
        {
            ShowPopup();
        }

        private void FailedInteractive(CharacterView view)
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
        }
        
        private void ShowPopup()
        {
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.GetDropItems,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                NetworkKey,
                response =>
                {
                    if (response.Data is not DropInventoryPopupData data)
                    {
                        return;
                    }

                    data.InteractiveObject = this;
                    
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.DropItemBoxPopup, data)).Forget();
                });
        }
    }
}