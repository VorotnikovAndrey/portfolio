using Gameplay.Inventory;
using PlayVibe;
using UnityEngine;

namespace Gameplay
{
    public interface ISlot
    {
        public int Index { get; }
        public int OwnerId { get; }
        public InventoryType InventoryType { get; }
        RectTransform SlotRect { get; }
        ItemView CurrentItem { get; }

        void SetSelectState(bool value);
        bool TrySnap(ItemView item);
        bool DropItem(DropReason reason = DropReason.Default);
    }
}