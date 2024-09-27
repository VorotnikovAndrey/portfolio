using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Character;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.Markers;
using Gameplay.Player.Zones;
using PlayVibe.RolePopup;
using UnityEngine;

namespace Gameplay.Player.TriggerZone
{
    public class TriggerZoneView : AbstractZoneView
    {
        [SerializeField] private List<BoxCollider> boxColliders;

        private void OnValidate()
        {
            boxColliders = GetComponents<BoxCollider>().ToList();
        }

        protected override void HandleTrigger(Collider other, bool isEntering)
        {
            var view = other.gameObject.GetComponent<CharacterView>();
            
            if (view == null || !view.PhotonView.IsMine)
            {
                return;
            }

            var actorId = view.PhotonView.Owner.ActorNumber;
            
            if (gameplayStage.GameplayDataDic[actorId].RoleType != RoleType.Prisoner)
            {
                return;
            }

            var eventHandler = gameplayController.GetEventHandler<ViewsNetworkEventHandler>();
            
            if (isEntering)
            {
                eventHandler.SendAddMarker(actorId, new List<MarkerType>
                {
                    MarkerType.Violator
                });
            }
            else
            {
                var isStillInside = boxColliders.Any(collider => collider.bounds.Contains(view.transform.position));

                if (!isStillInside)
                {
                    eventHandler.SendRemoveMarker(actorId, new List<MarkerType>
                    {
                        MarkerType.Violator
                    });
                }
            }
        }
    }
}