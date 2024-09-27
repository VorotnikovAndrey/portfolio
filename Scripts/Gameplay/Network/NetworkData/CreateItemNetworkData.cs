using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class CreateItemNetworkData
    {
        public string ItemKey;
        public InventoryType InventoryType;
        public int Owner;
    }
}