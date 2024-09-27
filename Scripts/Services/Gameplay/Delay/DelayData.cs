using System;
using UnityEngine.Serialization;

namespace Services.Gameplay.Delay
{
    [Serializable]
    public class DelayData
    {
        public string Id;
        public double Time;
        public Action Action;
    }
}