namespace Gameplay.Inventory
{
    public class RecyclerInventory : AbstractInventory
    {
        public RecyclerInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}