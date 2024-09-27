using System;
using System.Collections.Generic;
using System.Linq;
using PlayVibe;
using UniRx;
using UnityEngine;

namespace Gameplay.Inventory
{
    public abstract class AbstractInventory
    {
        protected readonly HashSet<ItemModel> items = new ();
        protected readonly InventoryOwnerType ownerType;
        protected readonly int ownerId;
        
        protected InventoryType inventoryType;
        protected int capacity;
        
        public InventoryType InventoryType => inventoryType;
        public InventoryOwnerType OwnerType => ownerType;
        public int OwnerId => ownerId;
        public int Capacity => capacity;
        public IEnumerable<ItemModel> Items => items;
        public bool HasPlace => items.Count < capacity; 
        
        public AbstractInventory(int capacity, InventoryType inventoryType, InventoryOwnerType ownerType, int ownerId)
        {
            this.inventoryType = inventoryType;
            this.capacity = capacity;
            this.ownerType = ownerType;
            this.ownerId = ownerId;
        }

        public void Add(ItemModel model)
        {
            if (!HasPlace)
            {
                return;
            }

            items.Add(model);
        }
        
        public void Add(IEnumerable<ItemModel> models)
        {
            foreach (var model in models)
            {
                Add(model);
            }
        }
        
        public void Remove(ItemModel model)
        {
            if (model == null)
            {
                return;
            }
            
            Remove(model.NetworkId);
        }
        
        public void Remove(int networkId)
        {
            if (!HasItem(networkId))
            {
                return;
            }

            items.RemoveWhere(x => x.NetworkId == networkId);
        }

        public bool HasItem(ItemModel model)
        {
            return items.Any(x => x.NetworkId == model.NetworkId);
        }
        
        public bool HasItem(int networkId)
        {
            return items.Any(x => x.NetworkId == networkId);
        }
        
        public ItemModel HasItemByItemKey(string key)
        {
            return items.FirstOrDefault(x => x.ItemKey == key);
        }

        public int GetFreeSlot()
        {
            var occupiedSlots = new HashSet<int>(Items.Select(item => item.Slot));

            for (var i = 0; i < capacity; i++)
            {
                if (!occupiedSlots.Contains(i))
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetFirstFreeSlot()
        {
            for (var i = 0; i < capacity; i++)
            {
                if (IsFreeSlot(i))
                {
                    return i;
                }
            }
            
            return -1;
        }

        public bool IsFreeSlot(int index)
        {
            return index >= 0 && Items.All(x => x.Slot != index);
        }

        public void Clear()
        {
            items.Clear();
        }
        
        public bool HasPlaceFor(int value)
        {
            return items.Count + value <= capacity; 
        }

        public ItemModel GetItemModelByType(string type)
        {
            return items.FirstOrDefault(x => x.ItemKey == type);
        }
    }
}