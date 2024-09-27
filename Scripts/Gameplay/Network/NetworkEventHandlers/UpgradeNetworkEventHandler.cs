using System;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Gameplay.Network.NetworkData;
using Photon.Pun;
using Photon.Realtime;
using Services;
using Services.Gameplay.Wallet;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class UpgradeNetworkEventHandler : AbstractNetworkEventHandler
    {
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.TryUpgradeLootBox] = TryUpgradeLootBox;
            events[PhotonPeerEvents.PlayerUpgradedLootBox] = PlayerUpgradedLootBox;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }
        
        /// <summary>
        /// Отправить запрос на попытку апгрейда личного контейнера
        /// </summary>
        public void SendTryUpgradeItemBox(WalletData data)
        {
            var eventCode = PhotonPeerEvents.TryUpgradeLootBox;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок запрашивает у мастера апгрейд своего личного хранилища
        /// </summary>
        /// <param name="photonEvent"></param>
        private void TryUpgradeLootBox(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not WalletData walletData)
            {
                return;
            }

            var actorData = gameplayStage.GameplayDataDic[walletData.ActorNumber];

            if (actorData.LootBoxUpgraded)
            {
                return;
            }

            if (!actorData.Wallet.Has(walletData.CurrencyType, walletData.ActorNumber))
            {
                return;
            }
            
            GameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(actorData.ActorNumber, CurrencyType.Soft, -walletData.Amount);
            
            var eventCode = PhotonPeerEvents.PlayerUpgradedLootBox;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new UpgradeLootBoxData
            {
                ActorNumber = walletData.ActorNumber,
                State = true
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер возвращает результат запроса улучшения личного хранилища
        /// </summary>
        /// <param name="photonEvent"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PlayerUpgradedLootBox(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not UpgradeLootBoxData data)
            {
                return;
            }
            
            gameplayStage.GameplayDataDic[data.ActorNumber].LootBoxUpgraded = data.State;
                
            eventAggregator.SendEvent(new PersonalLootBoxUpgradedEvent
            {
                Data = data
            });
        }
    }
}