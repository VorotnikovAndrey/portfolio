using System.Collections.Generic;
using System.Linq;
using Gameplay.Events;
using PlayVibe;
using PlayVibe.RolePopup;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.Player.Minigames
{
    public class MinigamesHandler : MonoBehaviour
    {
        [SerializeField] private List<MinigameInteractiveObject> minigameInteractiveObjects;

        [Inject] private EventAggregator eventAggregator;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private MinigamesSettings minigamesSettings;
        
        public Dictionary<int, MinigameInteractiveObject> Data { get; } = new();

        private void Awake()
        {
            foreach (var element in minigameInteractiveObjects)
            {
                Data.Add(element.NetworkKey, element);
            }

            Subscribes();
        }

        private void OnDestroy()
        {
            UnSubscribes();
        }

        protected void Subscribes()
        {
            eventAggregator.Add<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<QuestsUpdatedEvent>(OnQuestsUpdatedEvent);
        }

        private void OnQuestsUpdatedEvent(QuestsUpdatedEvent sender)
        {
            foreach (var element in Data)
            {
                element.Value.SetState(gameplayStage.LocalGameplayData.Quests.ContainsKey(element.Key));
            }
        }

        public void SetArray(List<MinigameInteractiveObject> array)
        {
            minigameInteractiveObjects = array;
        }

        public MinigameInteractiveObject GetRandomMinigameWithDifficulty(MinigameDifficulty difficulty, RoleType role, List<int> ignore = null)
        {
            var filteredMinigames = minigameInteractiveObjects
                .Where(x => minigamesSettings.GetSettings(x.MinigameType).Difficulty == difficulty &&
                            x.CanInteract(role) && (ignore == null || !ignore.Contains(x.NetworkKey))).ToList();

            if (filteredMinigames.Count == 0)
            {
                Debug.Log("No minigames found for the given type.".AddColorTag(Color.red));

                return null;
            }

            var randomIndex = Random.Range(0, filteredMinigames.Count);

            return filteredMinigames[randomIndex];
        }

        public MinigameInteractiveObject GetMinigameWithNetworkId(int networkId)
        {
            return Data[networkId];
        }
    }
}