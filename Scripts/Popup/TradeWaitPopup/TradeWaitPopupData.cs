using System;
using UnityEngine.Serialization;

namespace PlayVibe.TradeWaitPopup
{
    [Serializable]
    public class TradeWaitPopupData
    {
        public string Message;
        public int ActorNumber;
        public double StartTime;
        public double EndTime;
    }
}