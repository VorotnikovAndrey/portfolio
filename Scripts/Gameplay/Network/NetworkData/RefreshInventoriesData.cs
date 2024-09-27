using System;
using System.Collections.Generic;
using PlayVibe;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class RefreshInventoriesData
    {
        public List<InventoryPopupData> Data = new();
    }
}