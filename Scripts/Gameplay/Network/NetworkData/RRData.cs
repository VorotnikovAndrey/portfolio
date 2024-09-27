using System;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class RRData
    {
        public int RequestId;
        public RRType Type;
        public object Data;
    }
}