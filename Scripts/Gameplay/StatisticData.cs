using System;
using System.Collections.Generic;

namespace Gameplay.Player
{
    [Serializable]
    public class StatisticData
    {
        public Dictionary<string, StatisticItem> StatisticItemData = new();
    }

    [Serializable]
    public class StatisticItem
    {
        public string ItemKey;
        
        // время смены суток - пресет - количество
        public Dictionary<int, Dictionary<string, int>> SuccessfulCounts = new();
    }
}