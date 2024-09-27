using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class GetItemByTypeNetworkData
    {
        public int Owner;
        public InventoryType InventoryType;
        public string ItemType;
        public bool RemoveItem = true;
    }
}