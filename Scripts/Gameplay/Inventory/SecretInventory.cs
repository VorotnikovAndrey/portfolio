namespace Gameplay.Inventory
{
    public class SecretInventory : AbstractInventory
    {
        public SecretInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}