using Cysharp.Threading.Tasks;
using Gameplay.Inventory;
using Gameplay.Network;
using Zenject;

namespace PlayVibe.Subclass
{
    public class DropItemBoxInventoryPopup : InteractiveInventoryPopup
    {
        [Inject] private ViewsHandler viewsHandler;

        private DropInventoryPopupData dropInventoryPopupData;
        
        protected override AbstractInteractiveObject InteractiveObject { get; set; }
        protected override bool DragAllowed => InteractiveObject.CanInteract(gameplayStage.LocalGameplayData.RoleType);
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.Drop;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not DropInventoryPopupData castData)
            {
                Hide().Forget();
                
                return UniTask.CompletedTask;
            }

            dropInventoryPopupData = castData;

            base.OnShow(new InventoryPopupData
            {
                InventoryType = dropInventoryPopupData.InventoryType,
                Items = dropInventoryPopupData.Items,
                OwnerId = dropInventoryPopupData.OwnerId,
            });
            
            return UniTask.CompletedTask;
        }
        
        protected override void OnInitialized()
        {
            InteractiveObject = dropInventoryPopupData.InteractiveObject;
            Capacity = dropInventoryPopupData.Capacity;
        }
        
        protected override void UpdateTitle()
        {
            title.text = $"DropItemBox [id:{popupData.OwnerId}]";
        }
    }
}