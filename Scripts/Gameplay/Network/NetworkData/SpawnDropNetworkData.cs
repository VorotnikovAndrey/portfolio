using System;
using System.Collections.Generic;
using Services;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class SpawnDropNetworkData
    {
        public CustomVector3 Position;
        public List<ItemModel> Items = new();
        public int Floor;
    }
}