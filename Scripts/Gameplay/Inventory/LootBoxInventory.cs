namespace Gameplay.Inventory
{
    public class LootBoxInventory : AbstractInventory
    {
        public LootBoxInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}