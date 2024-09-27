using System;

namespace Gameplay
{
    [Serializable]
    public class DropChanceData
    {
        public string ItemKey;
        public float Chance = 100f;
        public int FromDay;
        public int ToDay = 10;
    }
}