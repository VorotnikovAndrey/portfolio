using System;
using Services.Gameplay.TimeDay;
using UnityEngine.Serialization;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class StartGameplayNetworkData
    {
        public int Day;
        public double EndTime;
        public TimeDayState TimeDayState;
    }
}