namespace Gameplay.Inventory
{
    public class CharacterInventory : AbstractInventory
    {
        public CharacterInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId) : base(capacity, inventoryType, ownerType, ownerId)
        {
        }
    }
}