using System;
using Services;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class CreateViewNetworkData
    {
        public string Name;
        public CustomVector3 Position;
        public int FloorIndex;
    }
}