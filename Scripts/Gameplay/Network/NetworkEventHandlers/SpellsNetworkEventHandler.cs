using System.Linq;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Gameplay.Player.Spells;
using Photon.Pun;
using Photon.Realtime;
using Services;
using Services.Gameplay.Wallet;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class SpellsNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private SpellsSettings spellsSettings;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.TryBuySpell] = ReceiveTryBuySpell;
            events[PhotonPeerEvents.AddSpell] = ReceiveAddSpell;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Игрок отправлят запрос мастеру на покупку спела
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <param name="spellType"></param>
        public void SendTryBuySpell(SpellType spellType)
        {
            var eventCode = PhotonPeerEvents.TryBuySpell;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, spellType, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void ReceiveTryBuySpell(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not SpellType spellType)
            {
                return;
            }

            var actorData = gameplayStage.GameplayDataDic[peerData.Sender];
            var spellData = spellsSettings.GetByType(spellType);

            if (!spellData.AvailableFor.Contains(actorData.RoleType))
            {
                return;
            }

            if (actorData.SpellHandlers.Any(x => x.SpellType == spellType))
            {
                return;
            }
            
            if (!actorData.Wallet.Has(CurrencyType.Soft, spellData.Price))
            {
                return;
            }
            
            GameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(actorData.ActorNumber, CurrencyType.Soft, -spellData.Price);
            
            var eventCode = PhotonPeerEvents.AddSpell;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new AddSpellNetworkData
            {
                ActorNumber = actorData.ActorNumber,
                SpellType = spellType
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void ReceiveAddSpell(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not AddSpellNetworkData data)
            {
                return;
            }
            
            gameplayStage.GameplayDataDic[data.ActorNumber].SpellHandlers.Add(new SpellHandler(spellsSettings.GetByType(data.SpellType))); 
            
            eventAggregator.SendEvent(new AddSpellEvent
            {
                Data = data
            });
        }
    }
}