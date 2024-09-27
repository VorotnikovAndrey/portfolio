using System;
using Gameplay.Player.Minigames;

namespace Gameplay.Player.Quests
{
    [Serializable]
    public class QuestData
    {
        public MinigameType Type;
        public MinigameDifficulty Difficulty;
        public int TargetNetworkId;
    }
}