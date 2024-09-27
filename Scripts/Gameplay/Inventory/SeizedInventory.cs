namespace Gameplay.Inventory
{
    public class SeizedInventory : AbstractInventory
    {
        public SeizedInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}