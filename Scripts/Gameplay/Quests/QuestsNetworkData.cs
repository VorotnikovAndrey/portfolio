using System;
using System.Collections.Generic;

namespace Gameplay.Player.Quests
{
    [Serializable]
    public class QuestsNetworkData
    {
        public Dictionary<int, QuestData> Data = new();
    }
}