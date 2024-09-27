using System;
using Gameplay.Inventory;
using UnityEngine.EventSystems;

namespace Gameplay
{
    [Serializable]
    public class ItemDragData
    {
        public PointerEventData PointerEventData;
        public ItemView ItemView;
    }
}