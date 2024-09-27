using System;

namespace Services.Gameplay.Warp
{
    [Serializable]
    public class WarpData
    {
        public int ActorNumber;
        public WarpPointType PointType;
        public int PersonalId;
    }
}