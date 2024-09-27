using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Player.Minigames
{
    [CreateAssetMenu(fileName = "MinigamesSettings", menuName = "SO/MinigamesSettings")]
    public class MinigamesSettings : ScriptableObject
    {
        [SerializeField] private List<QuestColor> QuestColors;
        [Space]
        [SerializeField] private List<MinigameClassification> Classification;
        [Space]
        [SerializeField] private List<MinigameSettingsData> Data;
        
        [Serializable]
        public class MinigameSettingsData
        {
            public MinigameType Type;
            public MinigameDifficulty Difficulty;
            [SerializeField] private List<RewardChance> rewards;
            [Space]
            public float FakeDuration = 5f;
            
            public int GetRandomReward()
            {
                var totalChance = 0f;
                
                foreach (var reward in rewards)
                {
                    totalChance += reward.Chance;
                }

                var randomValue = UnityEngine.Random.Range(0f, totalChance);
                var cumulativeChance = 0f;
                
                foreach (var reward in rewards)
                {
                    cumulativeChance += reward.Chance;
                    if (randomValue < cumulativeChance)
                    {
                        return reward.Amount;
                    }
                }

                return 0;
            }
        }
        
        [Serializable]
        public class QuestColor
        {
            public MinigameDifficulty Difficulty;
            public Color Color;
        }
        
        [Serializable]
        public class MinigameClassification
        {
            public MinigameDifficulty Difficulty;
            public List<MinigameType> Types;
        }

        [Serializable]
        public class RewardChance
        {
            public int Amount;
            public float Chance;
        }

        public Color GetQuestColor(MinigameDifficulty difficulty)
        {
            return QuestColors.FirstOrDefault(x => x.Difficulty == difficulty).Color;
        }

        public MinigameSettingsData GetSettings(MinigameType type)
        {
            return Data.FirstOrDefault(x => x.Type == type);
        }

        public List<MinigameSettingsData> GetClassification(MinigameDifficulty difficulty)
        {
            return Data.Where(x => x.Difficulty == difficulty).ToList();
        }
    }
}