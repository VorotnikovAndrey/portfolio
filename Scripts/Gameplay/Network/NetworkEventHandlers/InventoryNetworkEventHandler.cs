using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Items;
using Gameplay.Network.NetworkData;
using Gameplay.Player.Effects;
using Gameplay.Player.Markers;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using PlayVibe.Subclass;
using Services;
using Services.Gameplay;
using Services.Gameplay.Delay;
using Services.Gameplay.Wallet;
using Source;
using UnityEngine;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class InventoryNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private ItemFactory itemFactory;
        [Inject] private ViewsHandler viewsHandler;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private ItemTransitionService itemTransitionService;
        [Inject] private Balance balance;
        [Inject] private DelayService delayService;
        [Inject] private EffectsSettings effectsSettings;
        [Inject] private UseItemBehaviorHandler useItemBehaviorHandler;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.GetMapItemBoxItemsRequest] = GetMapItemBoxItemsRequest;
            events[PhotonPeerEvents.ShowPersonalLootBoxRequest] = ShowPersonalLootBoxRequest;
            events[PhotonPeerEvents.TransitionItem] = TransitionItemRequest;
            events[PhotonPeerEvents.RefreshInventories] = RefreshInventories;
            events[PhotonPeerEvents.ConfiscateInventory] = ConfiscateInventory;
            events[PhotonPeerEvents.GetRecyclersItemsRequest] = GetRecyclersItemsRequest;
            events[PhotonPeerEvents.TryRunRecycler] = TryRunRecycler;
            events[PhotonPeerEvents.RunRecycler] = RunRecycler;
            events[PhotonPeerEvents.TrySendRandomSeizedItemToRecycler] = TrySendRandomSeizedItemToRecycler;
            events[PhotonPeerEvents.ReactivateRecycler] = ReactivateRecycler;
            events[PhotonPeerEvents.TrySendRandomSeizedItemToLootBox] = TrySendRandomSeizedItemToLootBox;
            events[PhotonPeerEvents.DropItem] = ReceiveDropItem;
            events[PhotonPeerEvents.RemoveItem] = ReceiveRemoveItem;
            events[PhotonPeerEvents.HasItem] = ReceiveHasItem;
            events[PhotonPeerEvents.TryUseItem] = ReceiveTryUseItem;
            events[PhotonPeerEvents.ClearInventory] = ReceiveClearInventory;
            events[PhotonPeerEvents.UseConsumableItem] = ReceiveUseConsumableItem;
            events[PhotonPeerEvents.OfferTrade] = ReceiveOfferTrade;
            events[PhotonPeerEvents.InitializeTrade] = ReceiveInitializeTrade;
            events[PhotonPeerEvents.ShowTradePopup] = ReceiveShowTradePopup;
            events[PhotonPeerEvents.CancelTrade] = ReceiveCancelTrade;
            events[PhotonPeerEvents.InterruptTrade] = ReceiveInterruptTrade;
            events[PhotonPeerEvents.ConfirmTrade] = ReceiveConfirmTrade;
            events[PhotonPeerEvents.UpdateTradeItems] = ReceiveUpdateTradeItems;
            events[PhotonPeerEvents.GetTradeItems] = ReceiveGetTradeItems;
            events[PhotonPeerEvents.SuccessfulTrade] = RequestSuccessfulTrade;
            events[PhotonPeerEvents.CheckPlaceForTrade] = RequestCheckPlaceForTrade;
            events[PhotonPeerEvents.GetDropItems] = RequestGetDropItems;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }
        
        /// <summary>
        /// Мастер получает запрос на получение списка предметов из утилизатора
        /// </summary>
        /// <param name="peerData"></param>
        private void GetRecyclersItemsRequest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            var networkId = (int)requestData.Data;

            gameplayStage.MasterData.RecyclerInventories.TryGetValue(networkId, out var inventory);
            
            var data = new InventoryPopupData
            {
                OwnerId = networkId,
                InventoryType = InventoryType.Recycler,
                Items = inventory.Items.ToList()
            };

            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = data
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }

        /// <summary>
        /// Мастер получает запрос на отправку списка предметов из дропа
        /// </summary>
        /// <param name="peerData"></param>
        private void RequestGetDropItems(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            var viewID = (int)requestData.Data;
            var inventory = gameplayStage.MasterData.GetDropInventory(viewID);

            if (inventory == null)
            {
                sendResponse(null);
                GameplayController.GetEventHandler<GameplayNetworkEventHandler>().SendMessage("Drop inventory is null!", requestData.RequestId);
                
                return;
            }
            
            sendResponse(new DropInventoryPopupData
            {
                OwnerId = viewID,
                InventoryType = inventory.InventoryType,
                Items = inventory.Items.ToList(),
                Capacity = inventory.Capacity
            });
            
            return;

            void sendResponse(DropInventoryPopupData inventoryPopupData)
            {
                PhotonPeerService.RaiseUniversalEvent(
                    peerData.Code,
                    new RRData
                    {
                        RequestId = requestData.RequestId,
                        Type = RRType.Response,
                        Data = inventoryPopupData
                    },
                    new RaiseEventOptions
                    {
                        TargetActors = new[] { peerData.Sender }
                    },
                    SendOptions.SendReliable);
            }
        }

        /// <summary>
        /// Запрос мастеру для получения информации о MapItemBox
        /// </summary>
        /// <param name="photonEvent"></param>
        private void GetMapItemBoxItemsRequest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }

            var networkId = (int)requestData.Data;

            gameplayStage.MasterData.MapItemBoxesItems.TryGetValue(networkId, out var inventory);

            var data = new InventoryPopupData
            {
                OwnerId = networkId,
                InventoryType = InventoryType.MapItemBox,
                Items = inventory.Items.ToList()
            };

            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = data
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Запрос к мастеру для получения информации о персональном хранилище
        /// </summary>
        /// <param name="photonEvent"></param>
        private void ShowPersonalLootBoxRequest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }

            var playerActorNumber = (int)requestData.Data;
            var items = gameplayStage.GameplayDataDic[playerActorNumber].Inventories[InventoryType.LootBox].Items;

            var data = new InventoryPopupData
            {
                OwnerId = playerActorNumber,
                InventoryType = InventoryType.LootBox,
                Items = items.ToList()
            };

            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = data
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }
        
        private void ReceiveTryUseItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            if (requestData.Data is not GetItemByTypeNetworkData data)
            {
                return;
            }

            var inventory = gameplayStage.GameplayDataDic[data.Owner].Inventories[data.InventoryType];
            var itemModel = inventory.HasItemByItemKey(data.ItemType);
            var result = itemModel != null;

            if (result && data.RemoveItem)
            {
                result = RemoveItem(peerData.Sender, new RemoveItemNetworkData
                {
                    InventoryType = data.InventoryType,
                    Owner = peerData.Sender,
                    ItemModel = itemModel
                });
            }
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = result
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }

        private void ReceiveHasItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            if (requestData.Data is not GetItemByTypeNetworkData data)
            {
                return;
            }
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = gameplayStage.GameplayDataDic[data.Owner].Inventories[data.InventoryType].GetItemModelByType(data.ItemType)
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер проверяет, возможно ли совершить то действие которое хочет сделать игрок с предметом
        /// </summary>
        /// <param name="obj"></param>
        private void TransitionItemRequest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }
            
            if (requestData.Data is not ItemTransitionRequestData transition)
            {
                return;
            }

            if (TryPickUpMoney(peerData.Sender, transition))
            {
                return;
            }

            var result = itemTransitionService.MasterTransition(transition, out var fromInventory, out var toInventory);
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = result
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);

            if (result != ItemTransitionResult.Successfully && result != ItemTransitionResult.SendRefreshInventory)
            {
                return;
            }
            
            var data = new RefreshInventoriesData();
            
            data.Data.Add(new InventoryPopupData
            {
                InventoryType = transition.FromInventoryType,
                OwnerId = transition.FromNetworkId,
                Items = fromInventory?.Items.ToList()
            });

            if (toInventory != fromInventory)
            {
                data.Data.Add(new InventoryPopupData
                {
                    InventoryType = transition.ToInventoryType,
                    OwnerId = transition.ToNetworkId,
                    Items = toInventory?.Items.ToList()
                });
            }
                
            PhotonPeerService.RaiseUniversalEvent(PhotonPeerEvents.RefreshInventories,
                data,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                },
                SendOptions.SendReliable);
            
            // проверка на удаление DropInteractiveObject

            if (transition.FromInventoryType == InventoryType.Drop && !fromInventory.Items.Any())
            {
                GameplayController.GetEventHandler<ViewsNetworkEventHandler>().ReleaseDropInteractiveObject(transition.FromNetworkId);
            }
            
            // Проверка для марки

            var actorId = -1;
            
            if (transition.FromInventoryType == InventoryType.Character)
            {
                actorId = transition.FromNetworkId;
            }
            else if (transition.ToInventoryType == InventoryType.Character)
            {
                actorId = transition.ToNetworkId;
            }
            else
            {
                return;
            }
            
            var actor = gameplayStage.GameplayDataDic[actorId];

            if (actor.RoleType != RoleType.Prisoner)
            {
                return;
            }

            UpdateMarkers(actorId);
        }

        /// <summary>
        /// Обработка подбора игровой валюты
        /// </summary>
        private bool TryPickUpMoney(int actorNumber, ItemTransitionRequestData transition)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }
            
            if (itemsSettings.Data[transition.ItemModel.ItemKey].Key != "SoftCoin")
            {
                return false;
            }

            itemTransitionService.FindInventory(transition, out var inventory);

            if (inventory == null)
            {
                return false;
            }
            
            inventory.Remove(transition.ItemModel);

            var data = new RefreshInventoriesData();
            
            data.Data.Add(new InventoryPopupData
            {
                InventoryType = transition.FromInventoryType,
                OwnerId = transition.FromNetworkId,
                Items = inventory.Items.ToList()
            });
            
            PhotonPeerService.RaiseUniversalEvent(PhotonPeerEvents.RefreshInventories,
                data,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                },
                SendOptions.SendReliable);
            
            GameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(actorNumber, CurrencyType.Soft, 50);
            
            return true;
        }
        
        /// <summary>
        /// Игроки получают информацию о том что какие-то инвентари изменились
        /// </summary>
        /// <param name="peerData"></param>
        private void RefreshInventories(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not RefreshInventoriesData data)
            {
                return;
            }

            foreach (var element in data.Data)
            {
                eventAggregator.SendEvent(new UpdateInventoryItemsEvent
                {
                    PopupData = element
                });
            }
        }

        /// <summary>
        /// Конфисковать все предметы из инвентаря игрока
        /// </summary>
        /// <param name="popupDataOwnerId"></param>
        /// <param name="popupDataInventoryType"></param>
        public void SendConfiscateInventory(ConfiscateInventoryData data)
        {
            var eventCode = PhotonPeerEvents.ConfiscateInventory;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер получается запрос на очистку инвентаря у конкретного игрока
        /// </summary>
        /// <param name="peerData"></param>
        private void ConfiscateInventory(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not ConfiscateInventoryData data)
            {
                return;
            }
            
            var toInventory = gameplayStage.GameplayDataDic[peerData.Sender].Inventories[InventoryType.Seized];

            if (!toInventory.HasPlace)
            {
                return;
            }
            
            var fromInventory = gameplayStage.GameplayDataDic[data.ActorNumber].Inventories[data.InventoryType];
            
            foreach (var item in fromInventory.Items.ToList())
            {
                if (item == null)
                {
                    continue;
                }

                var itemSettings = itemsSettings.Data[item.ItemKey];

                if (itemSettings.Classification == ItemClassification.Permitted)
                {
                    continue;
                }
                
                fromInventory.Remove(item);

                if (toInventory.HasPlace)
                {
                    toInventory.Add(item);
                }
                else
                {
                    break;
                }
            }
            
            var refreshData = new RefreshInventoriesData();
            
            refreshData.Data.Add(new InventoryPopupData
            {
                InventoryType = fromInventory.InventoryType,
                OwnerId = data.ActorNumber,
                Items = fromInventory.Items.ToList()
            });
            
            refreshData.Data.Add(new InventoryPopupData
            {
                InventoryType = toInventory.InventoryType,
                OwnerId = toInventory.OwnerId,
                Items = toInventory.Items.ToList()
            });
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshData, raiseEventOptions, SendOptions.SendReliable);
        }

        public void SendTryRunRecycler(int networkId)
        {
            var eventCode = PhotonPeerEvents.TryRunRecycler;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, networkId, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Запрос к мастеру для запуска утилизатора
        /// </summary>
        /// <param name="obj"></param>
        private void TryRunRecycler(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var networkId = (int)peerData.CustomData;
            
            gameplayStage.LevelData.RecyclersData.TryGetValue(networkId, out var recyclerData);

            if (recyclerData != null)
            {
                if (!recyclerData.Enable)
                {
                    return;
                }
            }

            var time = PhotonNetwork.Time + balance.Inventory.RecyclerRunDuration;

            var data = new RecyclersData
            {
                OwnerId = networkId,
                Time = time,
                Enable = false
            };
            
            var eventCode = PhotonPeerEvents.RunRecycler;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
            
            delayService.Add(new DelayData
            {
                Id = string.Format(Constants.Formats.ReactivateRecycler, networkId),
                Time = time,
                Action = () =>
                {
                    gameplayStage.MasterData.RecyclerInventories[networkId].Clear();
                    
                    PhotonPeerService.RaiseUniversalEvent(PhotonPeerEvents.ReactivateRecycler,
                        networkId,
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable);
                }
            });
        }
        
        /// <summary>
        /// Все игрока получают информацию о том что утилизатор запустился
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RunRecycler(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not RecyclersData data)
            {
                return;
            }

            gameplayStage.LevelData.RecyclersData[data.OwnerId] = data;
            
            eventAggregator.SendEvent(new RecyclerDataUpdatedEvent
            {
                Data = data
            });
        }

        /// <summary>
        /// Игрок хочет переместить в утилизатор случайный изъятый предмет
        /// </summary>
        public void SendRandomSeizedItemToRecycler(int ownerId)
        {
            gameplayStage.LevelData.RecyclersData.TryGetValue(ownerId, out var recyclerData);

            if (recyclerData != null)
            {
                if (!recyclerData.Enable)
                {
                    return;
                }
            }
            
            var eventCode = PhotonPeerEvents.TrySendRandomSeizedItemToRecycler;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, ownerId, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер забирает один предмет из инвентаря охранника и помещает в утилизатор
        /// </summary>
        /// <param name="peerData"></param>
        private void TrySendRandomSeizedItemToRecycler(PhotonPeerData peerData)
        {
            var actorNumber = peerData.Sender;

            var actor = gameplayStage.GameplayDataDic[actorNumber];

            if (actor.RoleType != RoleType.Security)
            {
                return;
            }
            
            var ownerId = (int)peerData.CustomData;
            
            gameplayStage.LevelData.RecyclersData.TryGetValue(ownerId, out var recyclerData);

            if (recyclerData != null)
            {
                if (!recyclerData.Enable)
                {
                    return;
                }
            }
            
            var inventory = actor.Inventories[InventoryType.Seized];

            if (!inventory.Items.Any())
            {
                return;
            }
            
            var item = inventory.Items.ToList().GetRandom();

            var transition = new ItemTransitionRequestData
            {
                ItemModel = item,
                FromInventoryType = InventoryType.Seized,
                FromNetworkId = actor.ActorNumber,
                ToInventoryType = InventoryType.Recycler,
                ToNetworkId = ownerId
            };
            
            var result = itemTransitionService.MasterTransition(transition, out var fromInventory, out var toInventory);

            if (result != ItemTransitionResult.Successfully)
            {
                return;
            }
            
            var refreshData = new RefreshInventoriesData();
            
            refreshData.Data.Add(new InventoryPopupData
            {
                InventoryType = fromInventory.InventoryType,
                OwnerId = fromInventory.OwnerId,
                Items = fromInventory.Items.ToList()
            });
            
            refreshData.Data.Add(new InventoryPopupData
            {
                InventoryType = toInventory.InventoryType,
                OwnerId = toInventory.OwnerId,
                Items = toInventory.Items.ToList()
            });
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshData, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void ReactivateRecycler(PhotonPeerData peerData)
        {
            gameplayStage.LevelData.RecyclersData.TryGetValue((int)peerData.CustomData, out var recyclerData);

            if (recyclerData != null)
            {
                recyclerData.Enable = true;
            }
            
            popupService.GetPopups<RecyclerInventoryPopup>(Constants.Popups.Inventory.RecyclerInventoryPopup).ForEach(x => x.Reactivate());
        }
        
        /// <summary>
        /// Игрок хочет переместить в личный контейнер случайный изъятый предмет
        /// </summary>
        public void SendRandomSeizedItemToLootBox(int ownerId)
        {
            var eventCode = PhotonPeerEvents.TrySendRandomSeizedItemToLootBox;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, ownerId, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер проводит транзакцию предмета
        /// </summary>
        private void TrySendRandomSeizedItemToLootBox(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var ownerId = (int)peerData.CustomData;
            var inventorySeized = gameplayStage.GameplayDataDic[ownerId].Inventories[InventoryType.Seized];
            var inventoryLootBox = gameplayStage.GameplayDataDic[ownerId].Inventories[InventoryType.LootBox];

            if (!inventorySeized.Items.Any() || !inventoryLootBox.HasPlace)
            {
                return;
            }

            var item = inventorySeized.Items.FirstOrDefault();
            inventorySeized.Remove(item);
            item.Slot = inventoryLootBox.GetFreeSlot();
            inventoryLootBox.Add(item);

            var data = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = inventorySeized.InventoryType,
                        OwnerId = ownerId,
                        Items = inventorySeized.Items.ToList()
                    },
                    new()
                    {
                        InventoryType = inventoryLootBox.InventoryType,
                        OwnerId = ownerId,
                        Items = inventoryLootBox.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { ownerId }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Сообщить мастеру что предмет уничтожен
        /// </summary>
        /// <param name="data"></param>
        public void SendDropItem(TrashNetworkData data)
        {
            var eventCode = PhotonPeerEvents.DropItem;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Мастер получает сообщение о том что игрок уничтожил предмет и обрабатывает эту информацию
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveDropItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not TrashNetworkData trashNetworkData)
            {
                return;
            }

            var inventory = gameplayStage.GameplayDataDic[peerData.Sender].Inventories[trashNetworkData.FromInventoryType];
            
            inventory.Remove(trashNetworkData.ItemModel.NetworkId);
            
            var data = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = trashNetworkData.FromInventoryType,
                        OwnerId = peerData.Sender,
                        Items = inventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { peerData.Sender }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
            
            UpdateMarkers(peerData.Sender);
        }

        /// <summary>
        /// Удалить предмет у игрока
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <param name="lockPickModel"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SendRemoveItem(int actorNumber, InventoryType inventoryType, ItemModel model)
        {
            var eventCode = PhotonPeerEvents.RemoveItem;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, new RemoveItemNetworkData
            {
                Owner = actorNumber,
                InventoryType = inventoryType,
                ItemModel = model
            }, raiseEventOptions, SendOptions.SendReliable);
        }

        private void ReceiveRemoveItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not RemoveItemNetworkData data)
            {
                return;
            }

            RemoveItem(peerData.Sender, data);
        }

        private bool RemoveItem(int senderId, RemoveItemNetworkData data)
        {
            var inventory = gameplayStage.GameplayDataDic[data.Owner].Inventories[data.InventoryType];

            if (!inventory.HasItem(data.ItemModel.NetworkId))
            {
                return false;
            }
            
            inventory.Remove(data.ItemModel.NetworkId);
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = inventory.InventoryType,
                        OwnerId = data.Owner,
                        Items = inventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { senderId }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshInventoriesData, raiseEventOptions, SendOptions.SendReliable);
            
            UpdateMarkers(data.Owner);
            
            return true;
        }

        public void SendClearInventory(ClearInventoryNetworkData data)
        {
            var eventCode = PhotonPeerEvents.ClearInventory;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void ReceiveClearInventory(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not ClearInventoryNetworkData data)
            {
                return;
            }
            
            var inventory = gameplayStage.GameplayDataDic[data.Owner].Inventories[data.InventoryType];

            if (inventory == null)
            {
                return;
            }
            
            inventory.Clear();
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = inventory.InventoryType,
                        OwnerId = data.Owner,
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

            UpdateMarkers(data.Owner);
        }

        public void SendUseConsumableItem(ItemModel itemModel)
        {
            var eventCode = PhotonPeerEvents.UseConsumableItem;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, itemModel, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер получает сообщение о том что игрок использовал предмет
        /// </summary>
        /// <param name="obj"></param>
        private void ReceiveUseConsumableItem(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not ItemModel itemModel)
            {
                return;
            }
            
            var inventory = gameplayStage.GameplayDataDic[peerData.Sender].Inventories[InventoryType.Character];

            if (inventory == null)
            {
                return;
            }

            if (!inventory.HasItem(itemModel))
            {
                return;
            }
            
            inventory.Remove(itemModel);
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = inventory.InventoryType,
                        OwnerId = peerData.Sender,
                        Items = inventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { peerData.Sender }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshInventoriesData, raiseEventOptions, SendOptions.SendReliable);
            
            itemsSettings.Data.TryGetValue(itemModel.ItemKey, out var itemData);

            if (itemData is not { IsConsumable: true })
            {
                return;
            }

            if (itemData.Effects.Count > 0)
            {
                eventCode = PhotonPeerEvents.AddEffect;
                raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };

                foreach (var effectType in itemData.Effects)
                {
                    PhotonPeerService.RaiseUniversalEvent(eventCode, new EffectNetworkData
                    {
                        Target = peerData.Sender,
                        EffectType = effectType
                    }, raiseEventOptions, SendOptions.SendReliable);
                }
            }

            useItemBehaviorHandler.UseItem(itemData, peerData.Sender);
        }
        
        /// <summary>
        /// Какой-то игрок предлагает трейд
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveOfferTrade(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }

            if (gameplayStage.LocalGameplayData.CharacterView.IsBusy)
            {
                sendResult(false);
                return;
            }

            popupService.ShowPopup(new PopupOptions(Constants.Popups.TradeConfirmPopup, new TradeConfirmPopupData
            {
                ActorNumber = gameplayStage.GameplayDataDic[peerData.Sender].ActorNumber,
                StartTime = PhotonNetwork.Time,
                EndTime = PhotonNetwork.Time + balance.RequestTimeout.Trade,
                Message = string.Format(Constants.Messages.Trade.StartTrade, PhotonNetwork.NickName),
                Action = sendResult
            })).Forget();
            
            return;

            void sendResult(bool result)
            {
                PhotonPeerService.RaiseUniversalEvent(
                    peerData.Code,
                    new RRData
                    {
                        RequestId = requestData.RequestId,
                        Type = RRType.Response,
                        Data = result
                    },
                    new RaiseEventOptions
                    {
                        TargetActors = new[] { peerData.Sender }
                    },
                    SendOptions.SendReliable);

                if (result)
                {
                    SendInitializeTrade(peerData.Sender);
                }
            }
        }
        
        /// <summary>
        /// Отправить запрос на инициализацию сделки
        /// </summary>
        /// <param name="itemModel"></param>
        private void SendInitializeTrade(int target)
        {
            var eventCode = PhotonPeerEvents.InitializeTrade;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, target, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер инициализирует сделку между игроками
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveInitializeTrade(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var firstActorNumber = peerData.Sender;
            var secondActorNumber = (int)peerData.CustomData;
            var firstInventory = gameplayStage.GameplayDataDic[firstActorNumber].Inventories[InventoryType.Trade];
            var secondInventory = gameplayStage.GameplayDataDic[secondActorNumber].Inventories[InventoryType.Trade];
            var firstPopupData = new InventoryPopupData
            {
                OwnerId = firstActorNumber,
                InventoryType = InventoryType.Trade,
                Items = firstInventory.Items.ToList()
            };
            var secondPopupData = new InventoryPopupData
            {
                OwnerId = secondActorNumber,
                InventoryType = InventoryType.Trade,
                Items = secondInventory.Items.ToList()
            };

            sendData(firstActorNumber, new TradeInitializeNetworkData
            {
                PopupData = firstPopupData,
                TargetActorNumber = secondActorNumber
                
            });
            sendData(secondActorNumber, new TradeInitializeNetworkData
            {
                PopupData = secondPopupData,
                TargetActorNumber = firstActorNumber
            });
            
            return;

            void sendData(int actorNumber, TradeInitializeNetworkData data)
            {
                var eventCode = PhotonPeerEvents.ShowTradePopup;
                var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { actorNumber } };
            
                PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
            }
        }
        
        /// <summary>
        /// Для игроков открывается окно сделки
        /// </summary>
        /// <param name="obj"></param>
        private async void ReceiveShowTradePopup(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not TradeInitializeNetworkData tradeInitializeNetwork)
            {
                return;
            }
            
            var popup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.TradePopup, tradeInitializeNetwork.PopupData)) as TradePopup;

            if (popup != null)
            {
                popup.Sync(tradeInitializeNetwork.TargetActorNumber);
            }
        }

        /// <summary>
        /// Отправить сообщение мастеру о том что сделка отменена
        /// </summary>
        public void SendCancelTrade()
        {
            var eventCode = PhotonPeerEvents.CancelTrade;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Обмен был отменен, вернуть предметы
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveCancelTrade(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var actorNumber = peerData.Sender;
            var characterInventory = gameplayStage.GameplayDataDic[actorNumber].Inventories[InventoryType.Character];
            var tradeInventory = gameplayStage.GameplayDataDic[actorNumber].Inventories[InventoryType.Trade];

            foreach (var item in tradeInventory.Items)
            {
                item.Slot = characterInventory.GetFirstFreeSlot();
                characterInventory.Add(item);
            }
            
            tradeInventory.Clear();
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = characterInventory.InventoryType,
                        OwnerId = actorNumber,
                        Items = characterInventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { actorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshInventoriesData, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Отправить сообщение игроку об отмене сделки
        /// </summary>
        /// <param name="targetActorNumber"></param>
        public void SendInterruptTrade(int targetActorNumber)
        {
            var eventCode = PhotonPeerEvents.InterruptTrade;
            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { targetActorNumber } };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок получает сообщение что собеседник прервал обмен
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveInterruptTrade(PhotonPeerData peerData)
        {
            popupService.GetPopups<TradePopup>(Constants.Popups.Inventory.TradePopup).ForEach(x => x.TryInterrupt(peerData.Sender));
        }

        /// <summary>
        /// Отправить сообщение мастеру для проверки наличия места для подтверждения сделки и отправить подтверждение в случае если место есть.
        /// </summary>
        /// <param name="targetActorNumber"></param>
        public async UniTask<bool> SendTryConfirmTrade(int targetActorNumber, CancellationToken token)
        {
            var source = new UniTaskCompletionSource();
            var result = false;
            
            SendRequest(
                PhotonPeerEvents.CheckPlaceForTrade,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                targetActorNumber,
                response =>
                {
                    result = (bool)response.Data;
                    
                    if (result)
                    {
                        var eventCode = PhotonPeerEvents.ConfirmTrade;
                        var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { targetActorNumber } };
            
                        PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
                    }
                    
                    source.TrySetResult();
                });
            
            await source.Task.AttachExternalCancellation(token);
            
            return result;
        }

        /// <summary>
        /// Игрок подтвердил передачу предметов
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveConfirmTrade(PhotonPeerData peerData)
        {
            popupService.GetPopups<TradePopup>(Constants.Popups.Inventory.TradePopup).ForEach(x => x.SyncConfirm(peerData.Sender));
        }
        
        /// <summary>
        /// Игрок получает обновление инвентаря 
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveUpdateTradeItems(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not List<ItemModel> data)
            {
                return;
            }
            
            popupService.GetPopups<TradePopup>(Constants.Popups.Inventory.TradePopup).ForEach(x => x.TryUpdateItems(peerData.Sender, data));
        }

        /// <summary>
        /// Запросить у мастера информацию о содержимом инвентаря Trade
        /// </summary>
        public void RequestGetAndSendMyTradeItems(int targetActorNumber)
        {
            SendRequest(
                PhotonPeerEvents.GetTradeItems,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                gameplayStage.LocalGameplayData.ActorNumber,
                response =>
                {
                    if (response.Data is not List<ItemModel> data)
                    {
                        return;
                    }
                    
                    var eventCode = PhotonPeerEvents.UpdateTradeItems;
                    var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { targetActorNumber } };
            
                    PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
                });
        }

        /// <summary>
        /// За
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveGetTradeItems(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }

            var targetActor = (int)requestData.Data;
            var inventory = gameplayStage.GameplayDataDic[targetActor].Inventories[InventoryType.Trade];
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = inventory.Items.ToList()
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { peerData.Sender }
                },
                SendOptions.SendReliable);
        }

        /// <summary>
        /// Отправить сообщение мастеру что трейд удачно завершен
        /// </summary>
        public void SendSuccessfulTrade(int targetActorNumber)
        {
            var eventCode = PhotonPeerEvents.SuccessfulTrade;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, targetActorNumber, raiseEventOptions, SendOptions.SendReliable);
        }

        private void RequestSuccessfulTrade(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var senderActorNumber = peerData.Sender;
            var targetActorNumber = (int)peerData.CustomData;
            var targetTradeInventory = gameplayStage.GameplayDataDic[targetActorNumber].Inventories[InventoryType.Trade];
            var senderCharacterInventory = gameplayStage.GameplayDataDic[senderActorNumber].Inventories[InventoryType.Character];

            if (!senderCharacterInventory.HasPlaceFor(targetTradeInventory.Items.Count()))
            {
                Debug.LogError($"Error: RequestSuccessfulTrade dont has place".AddColorTag(Color.red));
                return;
            }

            foreach (var itemModel in targetTradeInventory.Items)
            {
                itemModel.Slot = senderCharacterInventory.GetFirstFreeSlot();
                senderCharacterInventory.Add(itemModel);
            }
            
            targetTradeInventory.Clear();
            
            var refreshInventoriesData = new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = InventoryType.Character,
                        OwnerId = senderActorNumber,
                        Items = senderCharacterInventory.Items.ToList()
                    }
                }
            };
            
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { senderActorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, refreshInventoriesData, raiseEventOptions, SendOptions.SendReliable);
            
            UpdateMarkers(senderActorNumber);
        }
        
        /// <summary>
        /// Мастер проверяет, хватает ли у игроков места для обмена
        /// </summary>
        /// <param name="peerData"></param>
        private void RequestCheckPlaceForTrade(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not RRData requestData)
            {
                return;
            }

            var senderActorNumber = peerData.Sender;
            var senderTradeInventory = gameplayStage.GameplayDataDic[senderActorNumber].Inventories[InventoryType.Trade];
            var targetActorNumber = (int)requestData.Data;
            var targetCharacterInventory = gameplayStage.GameplayDataDic[targetActorNumber].Inventories[InventoryType.Character];
            var result = targetCharacterInventory.HasPlaceFor(senderTradeInventory.Items.Count());
            
            PhotonPeerService.RaiseUniversalEvent(
                peerData.Code,
                new RRData
                {
                    RequestId = requestData.RequestId,
                    Type = RRType.Response,
                    Data = result
                },
                new RaiseEventOptions
                {
                    TargetActors = new[] { senderActorNumber }
                },
                SendOptions.SendReliable);
        }

        public void UpdateMarkers(int actorNumber)
        {
            var characterItems = gameplayStage.GameplayDataDic[actorNumber].Inventories[InventoryType.Character].Items.ToList();
            var tradeItems = gameplayStage.GameplayDataDic[actorNumber].Inventories[InventoryType.Trade].Items.ToList();
            var combinedItems = characterItems.Concat(tradeItems).ToList();
            
            if (combinedItems.Any(item => itemsSettings.Data[item.ItemKey].Classification == ItemClassification.Prohibited))
            {
                GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendAddMarker(actorNumber, new List<MarkerType>
                {
                    MarkerType.Smuggler
                });

                return;
            }
                
            GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendRemoveMarker(actorNumber, new List<MarkerType>
            {
                MarkerType.Smuggler
            });
        }
    }
}