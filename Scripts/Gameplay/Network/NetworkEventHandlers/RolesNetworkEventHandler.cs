using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class RolesNetworkEventHandler : AbstractNetworkEventHandler
    {
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.SelectRole] = ReceiveSelectRole;
            events[PhotonPeerEvents.ShowRolePopup] = ReceiveShowRolePopup;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Отправить мастеру результат своего выбора роли
        /// </summary>
        /// <param name="roleType"></param>
        /// <param name="actorNumber"></param>
        public void SendSelectRole(RoleType roleType, bool readyStatus, int actorNumber)
        {
            var eventCode = PhotonPeerEvents.SelectRole;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new RoleData
            {
                RoleType = roleType,
                ActorNumber = actorNumber,
                ReadyStatus = readyStatus
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Игроки получают информацию о выборе роли игрока
        /// </summary>
        /// <param name="photonEvent"></param>
        private void ReceiveSelectRole(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not RoleData data)
            {
                return;
            }

            var actorData = gameplayStage.GameplayDataDic[data.ActorNumber];
            actorData.RoleType = data.RoleType;
            actorData.SelectRoleReady = data.ReadyStatus;
            
            eventAggregator.SendEvent(new PlayerSelectRoleEvent
            {
                Data = data
            });
            
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (gameplayStage.GameplayDataDic.All(x => x.Value.SelectRoleReady))
            {
                GameplayController.GetEventHandler<BalanceNetworkEventHandler>().SendMasterBalanceRoles();
            }
        }
        
        /// <summary>
        /// Мастер запрашивает роли у игроков
        /// </summary>
        private void ReceiveShowRolePopup(PhotonPeerData peerData)
        {
            if (peerData.CustomData is RolePopupSettings data)
            {
                popupService.ShowPopup(new PopupOptions(Constants.Popups.RolePopup, data)).Forget();
            }
        }
    }
}