using Zenject;

namespace Gameplay
{
    public class ItemFactory : PlaceholderFactory<ItemModel>
    {
        private int networkItemCount;

        public ItemFactory()
        {
            networkItemCount = 0;
        }

        public ItemModel CreateModel(string itemKey, int slot = 0)
        {
            var model = new ItemModel
            {
                ItemKey = itemKey,
                NetworkId = networkItemCount,
                Slot = slot
            };

            networkItemCount++;

            return model;
        }
    }
}