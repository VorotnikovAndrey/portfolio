namespace Gameplay.Inventory
{
    public class MapItemBoxInventory : AbstractInventory
    {
        public MapItemBoxInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}