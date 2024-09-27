namespace Services.Gameplay
{
    public enum ItemTransitionResult
    {
        None,
        Successfully,
        SlotOccupied,
        IsNotPossible,
        ItemDoesNotExist,
        InventoryDoesNotExist,
        SendRefreshInventory
    }
}