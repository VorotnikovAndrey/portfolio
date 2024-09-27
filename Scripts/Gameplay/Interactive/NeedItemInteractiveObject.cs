using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using Services;
using UnityEngine;

namespace PlayVibe
{
    public abstract class NeedItemInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] protected string itemKey;
        [SerializeField] protected bool removeItemAfterUse = true;

        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            if (!CanInteract(role))
            {
                return;
            }
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.TryUseItem,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                new GetItemByTypeNetworkData
                {
                    Owner = gameplayStage.LocalGameplayData.ActorNumber,
                    InventoryType = InventoryType.Character,
                    ItemType = itemKey,
                    RemoveItem = removeItemAfterUse
                },
                response =>
                {
                    if (response.Data is true)
                    {
                        SuccessfulInteractive(view);
                    }
                    else
                    {
                        FailedInteractive(view);
                    }
                });
        }
        
        protected virtual void SuccessfulInteractive(CharacterView view)
        {
            Debug.Log($"SuccessfulInteractive".AddColorTag(Color.green));
        }

        protected virtual void FailedInteractive(CharacterView view)
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
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