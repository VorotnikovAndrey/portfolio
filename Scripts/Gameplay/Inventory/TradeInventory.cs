namespace Gameplay.Inventory
{
    public class TradeInventory : AbstractInventory
    {
        public TradeInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}