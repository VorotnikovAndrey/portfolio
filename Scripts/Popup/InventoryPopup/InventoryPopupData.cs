using System;
using System.Collections.Generic;
using Gameplay;
using Gameplay.Inventory;

namespace PlayVibe
{
    [Serializable]
    public class InventoryPopupData
    {
        public InventoryType InventoryType;
        public int OwnerId;
        public List<ItemModel> Items;
    }
}