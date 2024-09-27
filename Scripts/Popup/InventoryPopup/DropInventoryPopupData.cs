using System;

namespace PlayVibe
{
    [Serializable]
    public class DropInventoryPopupData : InventoryPopupData
    {
        [NonSerialized] public DropInteractiveObject InteractiveObject;
        public int Capacity;
    }
}