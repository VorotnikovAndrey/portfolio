namespace Gameplay.Inventory
{
    public class DropInventory : AbstractInventory
    {
        public DropInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}