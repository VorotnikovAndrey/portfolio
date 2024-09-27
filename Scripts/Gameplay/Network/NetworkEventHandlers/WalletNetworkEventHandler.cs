using ExitGames.Client.Photon;
using Photon.Realtime;
using PlayVibe;
using Services;
using Services.Gameplay.Wallet;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class WalletNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private Balance balance;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.ModifyCurrency] = ReceiveModifyCurrency;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Игрок сообщает всем что значение его кошелька изменилось
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <param name="currencyType"></param>
        /// <param name="amount"></param>
        public void SendModifyCurrency(int actorNumber, CurrencyType currencyType, int amount)
        {
            var eventCode = PhotonPeerEvents.ModifyCurrency;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new WalletData
            {
                ActorNumber = actorNumber,
                CurrencyType = currencyType,
                Amount = amount
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Обновить значеник кошелька
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveModifyCurrency(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not WalletData data)
            {
                return;
            }

            gameplayStage.GameplayDataDic[data.ActorNumber].Wallet.Modify(data.CurrencyType, data.Amount);
        }
    }
}