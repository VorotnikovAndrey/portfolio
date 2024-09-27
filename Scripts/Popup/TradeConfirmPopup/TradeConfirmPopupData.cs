using System;

namespace PlayVibe
{
    [Serializable]
    public class TradeConfirmPopupData : ConfirmPopupData
    {
        public int ActorNumber;
        public double StartTime;
        public double EndTime;
    }
}