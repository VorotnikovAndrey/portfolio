using System;
using System.Collections.Generic;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class CraftNetworkData
    {
        public int Owner;
        public string ItemKey;
        public List<int> ComponentsNetworkId;
    }
}