using ExitGames.Client.Photon;
using Gameplay.Player.Minigames;
using Photon.Pun;
using Photon.Realtime;
using Services;
using Services.Gameplay.Wallet;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class MinigamesNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private MinigamesSettings minigamesSettings;
        [Inject] private MinigamesHandler minigamesHandler;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.CompleteMinigame] = CompleteMinigame;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Игрок отправляет мастеру сообщение о том что прошел мини игру
        /// </summary>
        /// <param name="data"></param>
        public void SendCompleteMinigame(MinigameNetworkData data)
        {
            var eventCode = PhotonPeerEvents.CompleteMinigame;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Мастер получил сообщение о том что игрок прошел мини игру
        /// </summary>
        /// <param name="peerData"></param>
        private void CompleteMinigame(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not MinigameNetworkData data)
            {
                return;
            }

            var settings = minigamesSettings.GetSettings(data.Type);
            var walletNetworkEventHandler = GameplayController.GetEventHandler<WalletNetworkEventHandler>();
            
            walletNetworkEventHandler.SendModifyCurrency(data.Owner, CurrencyType.Soft, settings.GetRandomReward());
        }
    }
}