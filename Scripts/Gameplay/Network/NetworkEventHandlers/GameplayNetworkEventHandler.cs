using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network.NetworkData;
using Gameplay.Player.Markers;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using PlayVibe.WinPopup;
using Services;
using Services.Gameplay.TimeDay;
using Services.Gameplay.Warp;
using Utils;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class GameplayNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private WarpService warpService;
        [Inject] private TimeDayService timeDayService;
        [Inject] private Balance balance;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.ImReady] = ReceiveImReady;
            events[PhotonPeerEvents.StartGameplay] = ReceiveStartGameplay;
            events[PhotonPeerEvents.NextDayTime] = ReceiveNextDayTime;
            events[PhotonPeerEvents.PrisonerEscaped] = ReceivePrisonerEscaped;
            events[PhotonPeerEvents.WinBehavior] = ReceiveWinBehavior;
            events[PhotonPeerEvents.SendMessage] = ReceiveSendMessage;
            events[PhotonPeerEvents.GameplayControllerInitialized] = ReceiveGameplayControllerInitialized;
        }

        protected override void OnSubscribes()
        {
            eventAggregator.Add<EndDayEvent>(OnEndDayEvent);
        }

        protected override void OnUnSubscribes()
        {
            eventAggregator.Remove<EndDayEvent>(OnEndDayEvent);
        }
        
        private void OnEndDayEvent(EndDayEvent sender)
        {
            ApplyNextDayTime();
        }
        
        /// <summary>
        /// Мастер получает сообщение от игрока что тот готов к игре
        /// </summary>
        private void ReceiveImReady(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var actorNumber = peerData.Sender;
            
            gameplayStage.MasterData.ImReadyArray.Add(actorNumber);

            if (gameplayStage.MasterData.ImReadyArray.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                SendStartGameplay();
            }
        }

        /// <summary>
        /// Мастер запускает игровой цикл
        /// </summary>
        private void SendStartGameplay()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.StartGameplay;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, null, raiseEventOptions, SendOptions.SendReliable);
            
            ApplyNextDayTime();
        }

        /// <summary>
        /// Игроки получают сообщение о том что игровой цикл начался
        /// </summary>
        private void ReceiveStartGameplay(PhotonPeerData peerData)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.QuestsIndicatorPopup, null, PopupGroup.Hud)).Forget();
            popupService.ShowPopup(new PopupOptions(Constants.Popups.EffectsPopup, null, PopupGroup.Hud)).Forget();
            popupService.ShowPopup(new PopupOptions(Constants.Popups.GameplayHudPopup, null, PopupGroup.Hud)).Forget();
            popupService.ShowPopup(new PopupOptions(Constants.Popups.CurrenciesPopup, null, PopupGroup.Hud)).Forget();
            popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.CharacterInventoryPopup, new InventoryPopupData
            {
                InventoryType = InventoryType.Character,
                OwnerId = PhotonNetwork.LocalPlayer.ActorNumber,
                Items = new List<ItemModel>()
            }, PopupGroup.Hud)).Forget();
            
            warpService.WarpToHome(gameplayStage.LocalGameplayData.ActorNumber);
        }

        /// <summary>
        /// Мастер запускает следующее время суток
        /// </summary>
        public void ApplyNextDayTime()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var timeDayState = timeDayService.CurrentState == TimeDayState.Day ? TimeDayState.Night : TimeDayState.Day;
            var endTime = PhotonNetwork.Time + balance.TimeDay.GetTime(timeDayState);
            
            var data = new StartGameplayNetworkData
            {
                Day = timeDayState == TimeDayState.Day ? gameplayStage.CurrentDay + 1 : gameplayStage.CurrentDay,
                EndTime = endTime,
                TimeDayState = timeDayState
            };
            
            var eventCode = PhotonPeerEvents.NextDayTime;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void ReceiveNextDayTime(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not StartGameplayNetworkData data)
            {
                return;
            }
            
            if (gameplayStage.CurrentDay != data.Day)
            {
                gameplayStage.OverrideDay(data.Day);
                
                eventAggregator.SendEvent(new NextDayEvent
                {
                    Day = data.Day
                });
            }

            gameplayStage.IncrementationTimeOfDayChangeCounter();

            if (CheckWinBehavior())
            {
                return;
            }
            
            var prisoners = gameplayStage.GameplayDataDic.Where(x => x.Value.RoleType == RoleType.Prisoner);
            var viewsNetworkEventHandler = GameplayController.GetEventHandler<ViewsNetworkEventHandler>();
            
            if (data.TimeDayState == TimeDayState.Day)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    foreach (var element in prisoners)
                    {
                        viewsNetworkEventHandler.SendRemoveMarker(element.Key, new List<MarkerType>
                        {
                            MarkerType.Night
                        });
                    }
                }
                
                GameplayController.GetEventHandler<QuestsNetworkEventHandler>().GenerateQuestsForPlayers();
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    foreach (var element in prisoners)
                    {
                        viewsNetworkEventHandler.SendAddMarker(element.Key, new List<MarkerType>
                        {
                            MarkerType.Night
                        });
                    }
                }
            }
            
            GameplayController.GetEventHandler<ItemsNetworkEventHandler>().RefreshMapItemBoxes();
            
            timeDayService.Run(data.EndTime, data.TimeDayState);
            
            eventAggregator.SendEvent(new TimeOfDayChangeCounterUpdatedEvent
            {
                Index = gameplayStage.TimeOfDayChangeCounter
            });
        }
        
        public void SendPrisonerEscape(int actorId, EscapeType escapeType)
        {
            var eventCode = PhotonPeerEvents.PrisonerEscaped;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, new EscapeNetworkData
            {
                ActorNumber = actorId,
                EscapeType = escapeType
                
            }, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Мастер получает сообщение о том что преступник сбержал
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceivePrisonerEscaped(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not EscapeNetworkData data)
            {
                return;
            }
            
            var actorData = gameplayStage.GameplayDataDic[data.ActorNumber];

            if (actorData.Escaped)
            {
                return;
            }
            
            actorData.Escaped = true;

            if (data.ActorNumber == gameplayStage.LocalGameplayData.ActorNumber)
            {
                popupService.TryHidePopup(Constants.Popups.Inventory.CharacterInventoryPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.QuestsPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.QuestsIndicatorPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.MapPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.GameplayHudPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.SelfCraftPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.CurrenciesPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.AdminPopup).Forget();
                popupService.TryHidePopup(Constants.Popups.StaminaPopup).Forget();
            
                popupService.ShowPopup(new PopupOptions(Constants.Popups.SpectatorPopup)).Forget();
            }
            
            eventAggregator.SendEvent(new PrisonerEscapedEvent
            {
                ActorNumber = peerData.Sender
            });

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            actorData.CharacterView?.NetworkDestroy();
            actorData.LootBoxView?.NetworkDestroy();

            CheckWinBehavior();
        }

        /// <summary>
        /// Проверка мастером на победу
        /// </summary>
        private bool CheckWinBehavior()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            if (gameplayStage.CurrentDay > balance.Main.RoundMax)
            {
                var eventCode = PhotonPeerEvents.WinBehavior;
                var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
                PhotonPeerService.RaiseUniversalEvent(eventCode, RoleType.Security, raiseEventOptions, SendOptions.SendReliable);

                return true;
            }
            else
            {
                var array = gameplayStage.GameplayDataDic.Values.Where(x => x.RoleType == RoleType.Prisoner).ToList();

                if (array.Count == 0)
                {
                    return false;
                }
                
                if (!array.All(x => x.Escaped))
                {
                    return false;
                }
                
                var eventCode = PhotonPeerEvents.WinBehavior;
                var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
                PhotonPeerService.RaiseUniversalEvent(eventCode, RoleType.Prisoner, raiseEventOptions, SendOptions.SendReliable);
                
                return true;
            }
        }

        /// <summary>
        /// Сообщение от мастера о том что наступила победа
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveWinBehavior(PhotonPeerData peerData)
        {
            if (gameplayStage.LocalGameplayData.CharacterView != null)
            {
                (gameplayStage.LocalGameplayData.CharacterView as CharacterView)?.NetworkDestroy();
            }
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.WinPopup, new WinPopupData
            {
                WinRole = (RoleType)peerData.CustomData
            }, PopupGroup.Overlay)).Forget();
        }

        /// <summary>
        /// Отправить сообщение игроку
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actorNumber"></param>
        public void SendMessage(string message, int actorNumber)
        {
            var eventCode = PhotonPeerEvents.SendMessage;
            var raiseEventOptions = new RaiseEventOptions { TargetActors = new[] { actorNumber } };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, message, raiseEventOptions, SendOptions.SendReliable);
        }
        
        /// <summary>
        /// Игрок получает сообщение о чем-то и выводит его в InfoPopup
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveSendMessage(PhotonPeerData peerData)
        {
            ShowInfoPopup(peerData.CustomData.ToString());
        }
        
        /// <summary>
        /// Конкретный игрок загрузил уровень
        /// </summary>
        /// <param name="photonEvent"></param>
        private void ReceiveGameplayControllerInitialized(PhotonPeerData peerData)
        {
            gameplayStage.GameplayDataDic[peerData.Sender].ReadyStatus = GameplayReadyType.Ready;
            
            eventAggregator.SendEvent(new UpdateReadyPopupEvent());

            if (gameplayStage.GameplayDataDic.Any(x => x.Value.ReadyStatus != GameplayReadyType.Ready))
            {
                return;
            }
            
            popupService.TryHidePopup(Constants.Popups.GameplayReadyPopup).Forget();

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.ShowRolePopup;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new RolePopupSettings
            {
                Time = PhotonNetwork.Time + balance.RoleRules.PrepareTime
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}