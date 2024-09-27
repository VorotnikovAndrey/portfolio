using System;
using Gameplay.Inventory;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class ConfiscateInventoryData
    {
        public int ActorNumber;
        public InventoryType InventoryType;
    }
}