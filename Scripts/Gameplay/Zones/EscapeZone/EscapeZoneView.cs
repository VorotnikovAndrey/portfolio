using Gameplay.Character;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using PlayVibe.RolePopup;
using UnityEngine;

namespace Gameplay.Player.Zones.EscapeZone
{
    public class EscapeZoneView : AbstractZoneView
    {
        [SerializeField] private EscapeType escapeType;
        
        protected override void HandleTrigger(Collider other, bool isEntering)
        {
            if (!isEntering)
            {
                return;
            }
            
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

            var eventHandler = gameplayController.GetEventHandler<GameplayNetworkEventHandler>();

            eventHandler.SendPrisonerEscape(actorId, escapeType);
        }
    }
}