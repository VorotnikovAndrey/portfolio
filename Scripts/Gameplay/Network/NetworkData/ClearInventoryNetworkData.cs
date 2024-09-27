using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class ClearInventoryNetworkData
    {
        public int Owner;
        public InventoryType InventoryType;
    }
}