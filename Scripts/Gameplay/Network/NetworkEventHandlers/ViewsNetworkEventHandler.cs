using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Player;
using Gameplay.Player.Markers;
using Gameplay.Player.SpawnPoint;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;
using Services.ExtensionsClasses;
using Services.Gameplay.Delay;
using Services.Gameplay.TimeDay;
using Services.Gameplay.Wallet;
using Services.Gameplay.Warp;
using Source;
using UnityEngine;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class ViewsNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private SpawnPointHandler spawnPointHandler;
        [Inject] private WarpService warpService;
        [Inject] private DelayService delayService;
        [Inject] private Balance balance;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private TimeDayService timeDayService;
        [Inject] private ViewsHandler viewsHandler;
        [Inject] private FloorsHandler floorsHandler;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.CreateCharacter] = ReceiveCreateCharacter;
            events[PhotonPeerEvents.WarpToSpawnPoint] = ReceiveWarpTo;
            events[PhotonPeerEvents.AddMarker] = AddMarker;
            events[PhotonPeerEvents.RemoveMarker] = RemoveMarker;
            events[PhotonPeerEvents.Arrest] = Arrest;
            events[PhotonPeerEvents.SendArrestData] = ReceiveArrestData;
            events[PhotonPeerEvents.SystemArrest] = SystemArrest;
            events[PhotonPeerEvents.SpawnDrop] = SpawnDrop;
            events[PhotonPeerEvents.SpawnPhotonView] = SpawnPhotonView;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Создать персонажей для игроков
        /// </summary>
        public void SendCreateCharacter()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.CreateCharacter;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Сообщение от мастера с командой создания персонажей и прочей белеберды
        /// </summary>
        /// <param name="photonEvent"></param>
        private async void ReceiveCreateCharacter(PhotonPeerData peerData)
        {
            var localData = gameplayStage.LocalGameplayData;
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            var characterSpawnPoint = spawnPointHandler.GetRoomDic(localData.RoleType).FirstOrDefault(x => x.PersonalId == localData.CharacterSpawnPointIndex);
            var lootBoxSpawnPoint = characterSpawnPoint.TryGetConnection(SpawnPointConnectionType.LootBox);
            
            var characterView = PhotonNetwork.Instantiate(Constants.Resources.Gameplay.CharacterView, characterSpawnPoint.Position, Quaternion.identity);
            
            var lootBox = PhotonNetwork.Instantiate(
                role == RoleType.Prisoner ? Constants.Resources.Gameplay.PrisonerLootBoxView : Constants.Resources.Gameplay.SecurityLootBoxView,
                lootBoxSpawnPoint.Position,
                Quaternion.identity);

            var lootBoxInteractiveObject = lootBox.GetComponent<AbstractInteractiveObject>();
            var floorIndex = role == RoleType.Prisoner
                ? floorsHandler.PrisonerHomeFloor
                : floorsHandler.SecurityHomeFloor;
            
            lootBoxInteractiveObject.ChangeFloor(floorIndex);
            
            var cameraController = await objectPoolService.GetOrCreateView<LocationCameraController>(Constants.Views.LocationCameraController);
            var movement = characterView.GetComponent<CharacterMovement>();
            
            movement.SetCamera(cameraController);
            movement.WarpTo(characterSpawnPoint.Position);

            GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendAddMarker(localData.ActorNumber, new List<MarkerType>
            {
                role == RoleType.Prisoner ? MarkerType.Prisoner : MarkerType.Security
            });
            
            cameraController.MoveTo(characterView.transform.position);
            cameraController.FollowTo(characterView.transform);
            cameraController.gameObject.SetActive(true);

            gameplayStage.LocalGameplayData.LocationCamera = cameraController;
            
            ShowInfoPopup($"Your role: {gameplayStage.LocalGameplayData.RoleType.ToString()}", 1f);
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.ChatPopup, role)).Forget();
            popupService.ShowPopup(new PopupOptions(Constants.Popups.SpellsHudPopup)).Forget();
            
            var eventCode = PhotonPeerEvents.ImReady;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Телепортировать игрока в указанную точку
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <param name="pointType"></param>
        public void SendCharacterWarpTo(int actorNumber, WarpPointType pointType)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            SpawnPointType spawnPointType;

            if (pointType == WarpPointType.Solitary)
            {
                spawnPointType = SpawnPointType.Solitary;
            }
            else if (pointType == WarpPointType.SolitaryExit)
            {
                spawnPointType = SpawnPointType.SolitaryExit;
            }
            else if (pointType == WarpPointType.Home)
            {
                spawnPointType = gameplayStage.GameplayDataDic[actorNumber].RoleType == RoleType.Prisoner ? SpawnPointType.PrisonerRoom : SpawnPointType.SecurityRoom;
            }
            else
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.WarpToSpawnPoint;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new WarpData
            {
                ActorNumber = actorNumber,
                PointType = pointType,
                PersonalId = spawnPointHandler.SpawnPointsDictionary[spawnPointType].GetRandom().PersonalId
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок получат от мастера сообщения с командой телепортироваться в указанную точку
        /// </summary>
        /// <param name="peerData"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ReceiveWarpTo(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not WarpData data)
            {
                return;
            }

            switch (data.PointType)
            {
                case WarpPointType.Home:
                    warpService.WarpToHome(data.ActorNumber);
                    break;
                case WarpPointType.Solitary:
                    warpService.WarpToSolitary(data.ActorNumber, data.PersonalId);
                    break;
                case WarpPointType.SolitaryExit:
                    warpService.WarpToSolitaryExit(data.ActorNumber, data.PersonalId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Отправить всем сообщение о том что нужно добавить маркер конкретному игроку
        /// </summary>
        public void SendAddMarker(int actorNumber, List<MarkerType> array)
        {
            var eventCode = PhotonPeerEvents.AddMarker;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new MarkerNetworkData
            {
                ActorNumber = actorNumber,
                MarkerType = array
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Отправить всем сообщение о том что нужно добавить маркер конкретному игроку
        /// </summary>
        public void SendRemoveMarker(int actorNumber, List<MarkerType> array)
        {
            var eventCode = PhotonPeerEvents.RemoveMarker;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new MarkerNetworkData
            {
                ActorNumber = actorNumber,
                MarkerType = array
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Присвоить маркер игроку
        /// </summary>
        /// <param name="peerData"></param>
        private void AddMarker(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not MarkerNetworkData data)
            {
                return;
            }

            var actor = gameplayStage.GameplayDataDic[data.ActorNumber];

            if (actor.RoleType == RoleType.Security)
            {
                return;
            }
            
            var view = actor.CharacterView as CharacterView;

            foreach (var marker in data.MarkerType)
            {
                actor.Markers.Add(marker);
            }

            if (view != null && view.Marker != null)
            {
                view.Marker.UpdateColor(actor.Markers);
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (!data.MarkerType.Contains(MarkerType.Wanted))
            {
                return;
            }
            
            var id = $"{data.ActorNumber} RemoveMarker {MarkerType.Wanted}";
            var time = PhotonNetwork.Time + balance.Markers.WantedMarkLifeTime;
                
            delayService.Add(new DelayData
            {
                Id = id,
                Action = () =>
                {
                    SendRemoveMarker(data.ActorNumber, new List<MarkerType>
                    {
                        MarkerType.Wanted
                    });
                },
                Time = time
            });
        }
        
        private void RemoveMarker(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not MarkerNetworkData data)
            {
                return;
            }

            var actor = gameplayStage.GameplayDataDic[data.ActorNumber];
            var view = actor.CharacterView as CharacterView;

            if (view == null || view.Marker == null)
            {
                return;
            }
            
            foreach (var marker in data.MarkerType)
            {
                actor.Markers.Remove(marker);
            }
            
            view.Marker.UpdateColor(actor.Markers);
        }

        public void SendArrest(ArrestNetworkData data)
        {
            var eventCode = PhotonPeerEvents.Arrest;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        public void SendSystemArrest(int actorId)
        {
            var eventCode = PhotonPeerEvents.SystemArrest;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, actorId, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void Arrest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not ArrestNetworkData data)
            {
                return;
            }
            
            var casterActor = gameplayStage.GameplayDataDic[data.CasterId];
            var targetActor = gameplayStage.GameplayDataDic[data.TargetId];
            
            if (targetActor.Markers.Contains(MarkerType.Violator) ||
                targetActor.Markers.Contains(MarkerType.Night) ||
                targetActor.Markers.Contains(MarkerType.Wanted))
            {
                // Бесплатный арест
            }
            else
            {
                // Платный арест
                
                if (!casterActor.Wallet.Has(CurrencyType.Soft, balance.Interactive.TryArrestPrice))
                {
                    GameplayController.GetEventHandler<GameplayNetworkEventHandler>().SendMessage(string.Format(Constants.Messages.Info.NotEnoughCurrencyForArrest, balance.Interactive.TryArrestPrice), data.CasterId);
                
                    return;
                }
                
                GameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(data.CasterId, CurrencyType.Soft, -balance.Interactive.TryArrestPrice);

                if (!targetActor.Markers.Contains(MarkerType.Smuggler))
                {
                    GameplayController.GetEventHandler<GameplayNetworkEventHandler>().SendMessage(string.Format(Constants.Messages.Info.HasNoProhibitedItems, targetActor.Nickname), data.CasterId);
                    
                    return;
                }
            }
            
            var sortItems = new Dictionary<ItemClassification, List<ItemModel>>
            {
                { ItemClassification.Prohibited, new List<ItemModel>() },
                { ItemClassification.Permitted, new List<ItemModel>() }
            };
            
            var casterInventory = casterActor.Inventories[InventoryType.Seized];
            var targetInventory = targetActor.Inventories[InventoryType.Character];

            foreach (var item in targetInventory.Items)
            {
                sortItems[itemsSettings.Data[item.ItemKey].Classification].Add(item);
            }

            var dropArray = new List<ItemModel>();
            
            foreach (var itemModel in sortItems[ItemClassification.Prohibited])
            {
                if (casterInventory.HasPlace)
                {
                    casterInventory.Add(itemModel);
                }
                else
                {
                    itemModel.Slot = dropArray.Count;
                    dropArray.Add(itemModel);
                }
            }
            
            if (dropArray.Count > 0)
            {
                GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendSpawnDrop(new SpawnDropNetworkData
                {
                    Position = targetActor.CharacterView?.transform.position.ToCustomVector3() ?? Vector3.zero.ToCustomVector3(),
                    Items = dropArray,
                    Floor = targetActor.CharacterView?.FloorIndex ?? 0
                });
            }
            
            targetInventory.Clear();

            SendCharacterWarpTo(data.TargetId, WarpPointType.Solitary);
            
            GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendRemoveMarker(data.TargetId, new List<MarkerType>
            {
                MarkerType.Smuggler,
                MarkerType.Violator,
                MarkerType.Wanted
            });

            var arrestPopupNetworkData = new ArrestPopupNetworkData
            {
                EndTime = PhotonNetwork.Time + balance.Interactive.ArrestDuration
            };

            SendArrestData(data.TargetId, arrestPopupNetworkData);
            
            delayService.Add(new DelayData
            {
                Id = $"{data.TargetId} WarpTo {WarpPointType.SolitaryExit}",
                Action = () =>
                {
                    SendRemoveMarker(data.TargetId, new List<MarkerType>
                    {
                        MarkerType.Wanted, MarkerType.Violator, MarkerType.Smuggler
                    });
            
                    delayService.Remove($"{data.TargetId} RemoveMarker {MarkerType.Wanted}");
                    
                    if (timeDayService.CurrentState == TimeDayState.Day)
                    {
                        SendCharacterWarpTo(data.TargetId, WarpPointType.SolitaryExit);
                    }
                    else
                    {
                        SendCharacterWarpTo(data.TargetId, WarpPointType.Home);
                    }

                    popupService.TryHidePopup(Constants.Popups.ArrestPopup).Forget();
                },
                Time = arrestPopupNetworkData.EndTime
            });
            
            SendRefreshInventory(data.CasterId, new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = casterInventory.InventoryType,
                        OwnerId = data.CasterId,
                        Items = casterInventory.Items.ToList()
                    }
                }
            });

            SendRefreshInventory(data.TargetId, new RefreshInventoriesData
            {
                Data = new List<InventoryPopupData>
                {
                    new()
                    {
                        InventoryType = targetInventory.InventoryType,
                        OwnerId = data.TargetId,
                        Items = targetInventory.Items.ToList()
                    }
                }
            });
        }
        
        private void SystemArrest(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var actorId = (int)peerData.CustomData;

            if (gameplayStage.GameplayDataDic[actorId].RoleType == RoleType.Security)
            {
                return;
            }
            
            SendCharacterWarpTo(actorId, WarpPointType.Solitary);
            
            var arrestPopupNetworkData = new ArrestPopupNetworkData
            {
                EndTime = PhotonNetwork.Time + balance.Interactive.ArrestDuration
            };
            
            SendArrestData(actorId, arrestPopupNetworkData);
            
            delayService.Add(new DelayData
            {
                Id = $"{actorId} WarpTo {WarpPointType.SolitaryExit}",
                Action = () =>
                {
                    SendRemoveMarker(actorId, new List<MarkerType>
                    {
                        MarkerType.Wanted, MarkerType.Violator, MarkerType.Smuggler
                    });
            
                    delayService.Remove($"{actorId} RemoveMarker {MarkerType.Wanted}");
                    
                    if (timeDayService.CurrentState == TimeDayState.Day)
                    {
                        SendCharacterWarpTo(actorId, WarpPointType.SolitaryExit);
                    }
                    else
                    {
                        SendCharacterWarpTo(actorId, WarpPointType.Home);
                    }
                    
                    popupService.TryHidePopup(Constants.Popups.ArrestPopup).Forget();
                },
                Time = arrestPopupNetworkData.EndTime
            });
        }

        private void SendRefreshInventory(int actorNumber, RefreshInventoriesData data)
        {
            var eventCode = PhotonPeerEvents.RefreshInventories;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] {actorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// Заспавнить дроп
        /// </summary>
        public void SendSpawnDrop(SpawnDropNetworkData data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.SpawnDrop;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер получает сообщение о необходимости заспавнить дроп
        /// </summary>
        /// <param name="peerData"></param>
        private void SpawnDrop(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not SpawnDropNetworkData data)
            {
                return;
            }
            
            var instance = PhotonNetwork.Instantiate(Constants.Resources.Gameplay.DropInteractiveObject, data.Position.ToVector3(), Quaternion.identity);
            var dropInteractiveObject = instance.GetComponent<DropInteractiveObject>();
            var dropInventory = new DropInventory(data.Items.Count, InventoryType.Drop, InventoryOwnerType.System, dropInteractiveObject.PhotonView.ViewID);

            dropInteractiveObject.ChangeFloor(data.Floor);
            
            foreach (var itemModel in data.Items)
            {
                dropInventory.Add(itemModel);
            }
            
            gameplayStage.MasterData.DropData.Add(dropInteractiveObject.PhotonView.ViewID, new MasterData.DropMasterData
            {
                InteractiveObject = dropInteractiveObject,
                Inventory = dropInventory
            });
        }

        /// <summary>
        /// Удалить всю информацию о дропе
        /// </summary>
        /// <param name="viewID"></param>
        public void ReleaseDropInteractiveObject(int viewID)
        {
            gameplayStage.MasterData.DropData.TryGetValue(viewID, out var data);

            if (data == null)
            {
                return;
            }
            
            var instance = data.InteractiveObject;

            if (instance == null)
            {
                return;
            }
            
            PhotonNetwork.Destroy(instance.gameObject);

            gameplayStage.MasterData.DropData.Remove(viewID);
        }

        /// <summary>
        /// Отправить игроку информацию об аресте
        /// </summary>
        /// <param name="actorNumber"></param>
        /// <param name="data"></param>
        public void SendArrestData(int actorNumber, ArrestPopupNetworkData data)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.SendArrestData;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] {actorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок получает информацию о своем аресте
        /// </summary>
        /// <param name="obj"></param>
        private async void ReceiveArrestData(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not ArrestPopupNetworkData data)
            {
                return;
            }
            
            await popupService.TryHidePopup(Constants.Popups.ArrestPopup, true);
            popupService.ShowPopup(new PopupOptions(Constants.Popups.ArrestPopup, data)).Forget();
            
            eventAggregator.SendEvent(new ArrestLocalPrisoner());
        }

        /// <summary>
        /// Отправить сообщение игроку для спавна
        /// </summary>
        public void SendSpawnPhotonView(int actorNumber, CreateViewNetworkData data)
        {
            var eventCode = PhotonPeerEvents.SpawnPhotonView;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { actorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок получает приказ от мастера на создание фотон объекта
        /// </summary>
        /// <param name="peerData"></param>
        private void SpawnPhotonView(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not CreateViewNetworkData data)
            {
                return;
            }

            var view = PhotonNetwork.Instantiate(data.Name, data.Position.ToVector3(), Quaternion.identity);
            
            view.GetComponent<FloorChanger>()?.ChangeFloor(data.FloorIndex);
        }
    }
}