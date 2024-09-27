using ExitGames.Client.Photon;
using Gameplay.Character;
using Gameplay.Network.NetworkData;
using Gameplay.Player.Effects;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Services;
using UnityEngine;

namespace Gameplay.Player.Zones.TrapZone
{
    public class TrapZone : AbstractZoneView
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material p;
        [SerializeField] private Material s;

        private void Start()
        {
            meshRenderer.material = gameplayStage.GameplayDataDic[PhotonView.OwnerActorNr].RoleType == RoleType.Prisoner
                ? p
                : s;
        }
        
        protected override void HandleTrigger(Collider other, bool isEntering)
        {
            var view = other.gameObject.GetComponent<CharacterView>();
            
            if (view == null)
            {
                return;
            }

            var actorId = view.PhotonView.OwnerActorNr;
            
            if (gameplayStage.GameplayDataDic[actorId].RoleType == gameplayStage.GameplayDataDic[PhotonView.OwnerActorNr].RoleType)
            {
                return;
            }

            var eventCode = PhotonPeerEvents.AddEffect;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            var data = new EffectNetworkData
            {
                Target = actorId,
                EffectType = EffectType.Trap
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
            
            NetworkDestroy();
        }
    }
}