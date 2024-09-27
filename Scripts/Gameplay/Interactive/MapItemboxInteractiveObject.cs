using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayVibe
{
    public sealed class MapItemboxInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] private DropPreset dropPreset;
        [SerializeField] [Range(0, 8)] private int itemCount = 4;
        [SerializeField] private bool needItem;
        [ShowIf("needItem")] [SerializeField] private bool removeItemAfterUse;
        [ShowIf("needItem")] [SerializeField] private string needItemKey;

        public DropPreset DropPreset => dropPreset;
        public int ItemCount => itemCount;
        
        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            if (!CanInteract(role))
            {
                return;
            }

            if (needItem)
            {
                gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                    PhotonPeerEvents.TryUseItem,
                    new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    },
                    new GetItemByTypeNetworkData
                    {
                        Owner = view.PhotonView.OwnerActorNr,
                        InventoryType = InventoryType.Character,
                        ItemType = needItemKey,
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
            else
            {
                ShowPopup();
            }
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
                PhotonPeerEvents.GetMapItemBoxItemsRequest,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                networkKey,
                response =>
                {   
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.MapItemBoxPopup, response.Data)).Forget();
                });
        }
    }
}