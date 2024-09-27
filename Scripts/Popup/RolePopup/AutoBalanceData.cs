using System;
using System.Collections.Generic;

namespace PlayVibe.RolePopup
{
    [Serializable]
    public class AutoBalanceData
    {
        public Dictionary<int, AutoBalanceDataElement> Data;
        
        [Serializable]
        public class AutoBalanceDataElement
        {
            public RoleType RoleType;
            public int CharacterSpawnPointIndex;
            public int LootBoxSpawnPointIndex;
        }
    }
}