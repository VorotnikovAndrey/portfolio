using Gameplay;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Services;
using UnityEngine;

namespace PlayVibe.Elements
{
    public class SubmarineInteractiveGasolineCanisterObject : NeedItemInteractiveObject
    {
        [SerializeField] private float duration = 5f;
        [SerializeField] private float securityClearFuelTankDuration = 10f;
        [SerializeField] private SubmarineInteractiveObject submarineInteractiveObject;

        public void Start()
        {
            submarineInteractiveObject.ClearFuelTank();
        }
        
        public override void TryInteractive(CharacterView view)
        {
            if (gameplayStage.GameplayDataDic[view.PhotonView.Owner.ActorNumber].RoleType == RoleType.Security)
            {
                if (submarineInteractiveObject.FuelTank > 0)
                {
                    view.ActionBar.Show(new CharacterActionData
                    {
                        Position = view.Center.position,
                        Duration = securityClearFuelTankDuration,
                        Action = () =>
                        {
                            if (view == null)
                            {
                                return;
                            }
                    
                            submarineInteractiveObject.ClearFuelTank();
                        }
                    });
                }
                
                return;
            }
            
            if (submarineInteractiveObject.FuelTank == 3)
            {
                FailedInteractive(view);
                return;
            }
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.HasItem,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                new GetItemByTypeNetworkData
                {
                    Owner = view.PhotonView.OwnerActorNr,
                    InventoryType = InventoryType.Character,
                    ItemType = itemKey,
                    RemoveItem = false
                },
                response =>
                {
                    if (response.Data is ItemModel itemModel)
                    {
                        if (itemModel.ItemKey == itemKey)
                        {
                            if (submarineInteractiveObject.FuelTank == 3)
                            {
                                return;
                            }
                            
                            view.ActionBar.Show(new CharacterActionData
                            {
                                Position = view.Center.position,
                                Duration = duration,
                                Action = () =>
                                {
                                    if (view == null)
                                    {
                                        return;
                                    }
                                    
                                    if (submarineInteractiveObject.FuelTank == 3)
                                    {
                                        return;
                                    }
                                    
                                    base.TryInteractive(view);
                                }
                            });
                        }
                    }
                    else
                    {
                        FailedInteractive(view);
                    }
                });
        }
        
        protected override void SuccessfulInteractive(CharacterView view)
        {
            base.SuccessfulInteractive(view);

            submarineInteractiveObject.IncrementFuelTank();
        }
    }
}