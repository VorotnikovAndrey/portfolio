using Gameplay;
using Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class SecuritySlot : MonoBehaviour, ISlot
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI amountText;

        [Inject] private EventAggregator eventAggregator;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;

        public Button Button => button;
        public int Index => 0;
        public int OwnerId { get; private set; }
        public InventoryType InventoryType => InventoryType.Seized;
        public RectTransform SlotRect => null;
        public ItemView CurrentItem { get; private set; }
        public bool HasPlace { get; set; } = true;
        
        public void SetSelectState(bool value)
        {
            
        }

        public bool TrySnap(ItemView item)
        {
            return true;
        }

        public bool DropItem(DropReason reason = DropReason.Default)
        {
            return false;
        }

        public void Initialize(int ownerId)
        {
            OwnerId = ownerId;
            
            eventAggregator.Add<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
            
            UpdateText(0);
        }

        public void Deinitialize()
        {
            eventAggregator.Remove<UpdateInventoryItemsEvent>(OnUpdateInventoryItemsEvent);
        }

        private void OnUpdateInventoryItemsEvent(UpdateInventoryItemsEvent sender)
        {
            if (sender.PopupData.InventoryType != InventoryType.Seized || sender.PopupData.OwnerId != OwnerId)
            {
                return;
            }
            
            UpdateText(sender.PopupData.Items.Count);

            HasPlace = sender.PopupData.Items.Count < balance.Inventory.SecuritySeizedInventoryCapacity;
        }

        private void UpdateText(int amount)
        {
            amountText.text = $"{amount}/{balance.Inventory.SecuritySeizedInventoryCapacity}";
        }
    }
}