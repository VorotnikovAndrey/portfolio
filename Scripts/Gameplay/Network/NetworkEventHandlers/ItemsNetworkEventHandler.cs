using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Player;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;
using UnityEngine;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class ItemsNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private ItemFactory itemFactory;
        [Inject] private ViewsHandler viewsHandler;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private Balance balance;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.CreateItemFor] = ReceiveCreateItemFor;
            events[PhotonPeerEvents.GetStatistic] = ReceiveGetStatistic;
            
            InitializeRecyclers();
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }
        
        /// <summary>
        /// Обновляется предметы во всех MapItemBoxes
        /// </summary>
        public void RefreshMapItemBoxes()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var statistic = gameplayStage.StatisticData;
            
            gameplayStage.MasterData.MapItemBoxesItems.Clear();

            foreach (var mapItemBox in viewsHandler.MapItemBoxes)
            {
                if (mapItemBox== null)
                {
                    continue;
                }

                var items = new List<ItemModel>();
                var dropPreset = mapItemBox.DropPreset;
                var itemCount = mapItemBox.ItemCount;
                var currentDay = gameplayStage.CurrentDay;
                var timeOfDayChangeCounter = gameplayStage.TimeOfDayChangeCounter;

                if (dropPreset == null)
                {
                    Debug.Log("DropPreset is null".AddColorTag(Color.red));
                    continue;
                }

                var keys = dropPreset.GetRandomItems(itemCount, currentDay);
                var amountPrisoners = gameplayStage.GameplayDataDic.Count(x => x.Value.RoleType == RoleType.Prisoner);
                var failedChance = balance.Drop.FailedChange[amountPrisoners];

                keys = keys.Where((key, i) => Random.Range(0f, 100f) >= failedChance).ToList();

                if (keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        var data = itemsSettings.Data[key];
                        var model = itemFactory.CreateModel(data.Key, items.Count);
                        items.Add(model);
                    }
                }

                var inventory = new MapItemBoxInventory(
                    mapItemBox.ItemCount,
                    InventoryType.MapItemBox,
                    InventoryOwnerType.System,
                    mapItemBox.NetworkKey);
                
                inventory.Add(items);
                
                gameplayStage.MasterData.MapItemBoxesItems.Add(mapItemBox.NetworkKey, inventory);

                foreach (var itemModel in items)
                {
                    if (!statistic.StatisticItemData.ContainsKey(itemModel.ItemKey))
                    {
                        statistic.StatisticItemData.Add(itemModel.ItemKey, new StatisticItem());
                    }

                    var statisticItem = statistic.StatisticItemData[itemModel.ItemKey];

                    statisticItem.ItemKey = itemModel.ItemKey;

                    if (!statisticItem.SuccessfulCounts.ContainsKey(timeOfDayChangeCounter))
                    {
                        statisticItem.SuccessfulCounts.Add(timeOfDayChangeCounter, new Dictionary<string, int>());
                    }
                    
                    var dic = statisticItem.SuccessfulCounts[timeOfDayChangeCounter];

                    dic.TryAdd(dropPreset.name, 0);
                    dic[dropPreset.name]++;
                }
            }
        }

        /// <summary>
        /// Инициализация инвентарей для утилизаторов
        /// </summary>
        private void InitializeRecyclers()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            foreach (var element in viewsHandler.Recyclers)
            {
                gameplayStage.MasterData.RecyclerInventories.Add(element.NetworkKey,
                    new RecyclerInventory(balance.Inventory.RecyclerCapacity, InventoryType.Recycler,
                        InventoryOwnerType.System, element.NetworkKey));
            }
        }
        
        private void ReceiveCreateItemFor(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not CreateItemNetworkData data)
            {
                return;
            }
            
            var inventory = gameplayStage.GameplayDataDic[peerData.Sender].Inventories[data.InventoryType];

            if (!inventory.HasPlace)
            {
                return;
            }

            var itemModel = itemFactory.CreateModel(data.ItemKey, inventory.GetFirstFreeSlot());
            
            inventory.Add(itemModel);
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = InventoryType.Character,
                        OwnerId = peerData.Sender,
                        Items = inventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { data.Owner }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshInventoriesData, raiseEventOptions, SendOptions.SendReliable);

            GameplayController.GetEventHandler<InventoryNetworkEventHandler>().UpdateMarkers(peerData.Sender);
        }
        
        /// <summary>
        /// Игрок просит у мастера актуальную статистику
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveGetStatistic(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = JsonConvert.SerializeObject(gameplayStage.StatisticData)
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }
    }
}