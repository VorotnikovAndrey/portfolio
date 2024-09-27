using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Inventory;
using PlayVibe;

namespace Gameplay.Player
{
    public class MasterData
    {
        public Dictionary<int, MapItemBoxInventory> MapItemBoxesItems = new();
        public Dictionary<int, RecyclerInventory> RecyclerInventories = new();
        public List<int> ImReadyArray = new();
        public Dictionary<int, DropMasterData> DropData = new();

        public DropInventory GetDropInventory(int viewID)
        {
            if (DropData == null || !DropData.Any())
            {
                return null;
            }

            DropData.TryGetValue(viewID, out var data);

            return data?.Inventory;
        }

        [Serializable]
        public class DropMasterData
        {
            public DropInteractiveObject InteractiveObject;
            public DropInventory Inventory;
        }
    }
}