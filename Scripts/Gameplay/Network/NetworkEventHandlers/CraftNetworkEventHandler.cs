using System.Linq;
using ExitGames.Client.Photon;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using Services;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class CraftNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private ItemFactory itemFactory;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.TryCraftItem] = TryCraftItem;
        }

        protected override void OnSubscribes()
        {

        }

        protected override void OnUnSubscribes()
        {

        }

        /// <summary>
        /// Игрок просит мастера скрафтить предмет
        /// </summary>
        /// <param name="data"></param>
        public void SendTryCraftItem(CraftNetworkData data)
        {
            var eventCode = PhotonPeerEvents.TryCraftItem;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер получает просьбу от игрока на крафт предмета
        /// </summary>
        /// <param name="peerData"></param>
        private void TryCraftItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not CraftNetworkData data)
            {
                return;
            }

            var inventory = gameplayStage.GameplayDataDic[data.Owner].Inventories[InventoryType.Character];

            var allElementsExist = data.ComponentsNetworkId.All(item => inventory.HasItem(item));

            if (!allElementsExist)
            {
                return;
            }

            foreach (var itemKey in data.ComponentsNetworkId)
            {
                inventory.Remove(itemKey);
            }
      
            inventory.Add(itemFactory.CreateModel(data.ItemKey, inventory.GetFirstFreeSlot()));
            
            var refreshData = new RefreshInventoriesData();
            
            refreshData.Data.Add(new InventoryPopupData
            {
                InventoryType = InventoryType.Character,
                OwnerId = data.Owner,
                Items = inventory.Items.ToList()
            });
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshData, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}