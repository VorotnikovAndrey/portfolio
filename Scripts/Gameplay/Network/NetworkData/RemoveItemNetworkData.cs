using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class RemoveItemNetworkData
    {
        public int Owner;
        public InventoryType InventoryType;
        public ItemModel ItemModel;
    }
}