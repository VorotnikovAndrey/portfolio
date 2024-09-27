using System.Collections.Generic;
using Gameplay.Network.NetworkData;

namespace Gameplay.Player
{
    public class LevelData
    {
        public readonly Dictionary<int, RecyclersData> RecyclersData = new();
    }
}