using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.WarpZone;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Services;
using Services.Gameplay.Warp;
using UnityEngine;
using Zenject;

namespace PlayVibe.Elements
{
    public class DynamiteWallInteractiveObject : NeedItemInteractiveObject
    {
        [SerializeField] private string actionAnimationKey;
        [SerializeField] private float duration;
        [SerializeField] private Transform dinamiteSpawnPoint;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private List<WarpZone> warpZones;

        [Inject] private WarpService warpService;
        
        public bool Activated { get; private set; }
        
        protected override void Subscribes()
        {
            eventAggregator.Add<NextDayEvent>(OnNextDayEvent);
            eventAggregator.Add<DynamiteExplosionEvent>(OnDynamiteExplosionEvent);
        }

        protected override void UnSubscribes()
        {
            eventAggregator.Remove<NextDayEvent>(OnNextDayEvent);
            eventAggregator.Remove<DynamiteExplosionEvent>(OnDynamiteExplosionEvent);
        }

        private void OnDynamiteExplosionEvent(DynamiteExplosionEvent sender)
        {
            SetActivateState(true);
        }

        private void OnNextDayEvent(NextDayEvent sender)
        {
            Activated = false;
            
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            warpZones.ForEach(x => x.SetActiveState(true));
        }
        
        public override void TryInteractive(CharacterView view)
        {
            if (Activated)
            {
                FailedInteractive(view);
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
                    RemoveItem = false
                },
                response =>
                {
                    if (response.Data is false)
                    {
                        ShowInfoPopup($"Required item: {itemKey}");
                        return;
                    }
                    
                    if (Activated)
                    {
                        return;
                    }
                    
                    view.ActionBar.Show(new CharacterActionData
                    {
                        AnimationKey = actionAnimationKey,
                        Position = view.Center.position,
                        Duration = duration,
                        Action = () =>
                        {
                            if (view == null)
                            {
                                return;
                            }
                    
                            if (Activated)
                            {
                                return;
                            }

                            PhotonNetwork.Instantiate(Constants.Resources.Gameplay.DynamiteInteractiveObject, dinamiteSpawnPoint.position, Quaternion.identity);

                            base.TryInteractive(view);
                        }
                    });
                });
        }
        
        public void SetActivateState(bool value)
        {
            photonView.RPC("SetActivateStateRPC", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void SetActivateStateRPC(bool value)
        {
            if (Activated)
            {
                return;
            }
            
            Activated = value;

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            warpZones.ForEach(x => x.SetActiveState(false));
            
            var eventHandler = gameplayController.GetEventHandler<GameplayNetworkEventHandler>();
            var center = boxCollider.transform.TransformPoint(boxCollider.center);
            var halfExtents = boxCollider.size * 0.5f;
            var colliders = Physics.OverlapBox(center, halfExtents, boxCollider.transform.rotation);
            var characterViews = colliders
                .Select(collider => collider.GetComponent<CharacterView>())
                .Where(characterView => characterView != null)
                .ToList();

            foreach (var characterView in characterViews)
            {
                var actorId = characterView.PhotonView.Owner.ActorNumber;
            
                if (gameplayStage.GameplayDataDic[actorId].RoleType != RoleType.Prisoner)
                {
                    warpService.WarpToHome(actorId);
                    
                    continue;
                }

                gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                    PhotonPeerEvents.HasItem,
                    new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    },
                    new GetItemByTypeNetworkData
                    {
                        Owner = actorId,
                        InventoryType = InventoryType.Character,
                        ItemType = "Wetsuit",
                        RemoveItem = false
                    },
                    response =>
                    {
                        if (response.Data is not ItemModel itemModel)
                        {
                            Arrest(actorId);
                            return;
                        }

                        if (itemModel.ItemKey != "Wetsuit")
                        {
                            Arrest(actorId);
                            return;
                        }
                        
                        eventHandler.SendPrisonerEscape(actorId, EscapeType.Dynamite);
                    });
            }
        }

        private void Arrest(int actorId)
        {
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendClearInventory(new ClearInventoryNetworkData
            {
                Owner = actorId,
                InventoryType = InventoryType.Character
            });
            
            gameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendSystemArrest(actorId);
        }
    }
}