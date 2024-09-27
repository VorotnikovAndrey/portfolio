using System;
using Gameplay;
using Gameplay.Inventory;

namespace Services.Gameplay
{
    [Serializable]
    public class ItemTransitionRequestData
    {
        public ItemModel ItemModel;
        public InventoryType FromInventoryType;
        public InventoryType ToInventoryType;
        public int FromSlot;
        public int ToSlot;
        public int FromNetworkId;
        public int ToNetworkId;
    }
}