using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class TrashNetworkData
    {
        public ItemModel ItemModel;
        public InventoryType FromInventoryType;
    }
}