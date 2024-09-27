using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Gameplay.Events;
using Gameplay.Player;
using Gameplay.Player.Minigames;
using Gameplay.Player.Quests;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Services;
using Source;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class QuestsNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private MinigamesHandler minigamesHandler;
        [Inject] private MinigamesSettings minigamesSettings;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.ReceiveQuests] = ReceiveQuests;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Мастер генерирует задания для всех игроков и отсылает их им
        /// </summary>
        public void GenerateQuestsForPlayers()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            foreach (var playerData in gameplayStage.GameplayDataDic.Where(x => x.Value.RoleType == RoleType.Security))
            {
                var result = new QuestsNetworkData();
                
                var fastClassification = minigamesSettings.GetClassification(MinigameDifficulty.Fast);
                var mediumClassification = minigamesSettings.GetClassification(MinigameDifficulty.Medium);
                var longClassification = minigamesSettings.GetClassification(MinigameDifficulty.Long);

                apply(result, fastClassification, playerData.Value, 3);
                apply(result, mediumClassification, playerData.Value, 1);
                apply(result, longClassification, playerData.Value, 1);
                
                var eventCode = PhotonPeerEvents.ReceiveQuests;
                var raiseEventOptions = new RaiseEventOptions
                {
                    TargetActors = new[] { playerData.Key }
                };
    
                PhotonPeerService.RaiseUniversalEvent(eventCode, result, raiseEventOptions, SendOptions.SendReliable);
            }

            return;

            void apply(QuestsNetworkData result, IList<MinigamesSettings.MinigameSettingsData> classification, GameplayData gameplayData, int count)
            {
                var ignoreIds = new List<int>();
                
                for (var i = 0; i < count; i++)
                {
                    var settings = classification.GetRandom();
                    
                    if (settings == null)
                    {
                        continue;
                    }
                    
                    var minigame = minigamesHandler.GetRandomMinigameWithDifficulty(settings.Difficulty, gameplayData.RoleType, ignoreIds);

                    if (minigame == null)
                    {
                        continue;
                    }
                    
                    result.Data.Add(minigame.NetworkKey, new QuestData
                    {
                        Type = minigame.MinigameType,
                        Difficulty = settings.Difficulty,
                        TargetNetworkId = minigame.NetworkKey,
                    });
                    
                    ignoreIds.Add(minigame.NetworkKey);
                }
            }
        }

        /// <summary>
        /// Игрок получается список заданий от мастера
        /// </summary>
        /// <param name="peerData"></param>
        private void ReceiveQuests(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not QuestsNetworkData data)
            {
                return;
            }

            gameplayStage.LocalGameplayData.Quests = data.Data;
            
            eventAggregator.SendEvent(new QuestsUpdatedEvent());
        }
    }
}