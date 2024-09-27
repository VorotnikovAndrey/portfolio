using System;
using UnityEngine.Serialization;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class RecyclersData
    {
        public int OwnerId;
        public double Time;
        public bool Enable;
    }
}