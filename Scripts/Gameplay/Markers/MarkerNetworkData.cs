using System;
using System.Collections.Generic;

namespace Gameplay.Player.Markers
{
    [Serializable]
    public class MarkerNetworkData
    {
        public int ActorNumber;
        public List<MarkerType> MarkerType = new();
    }
}