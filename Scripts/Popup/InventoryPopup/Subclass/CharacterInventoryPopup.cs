using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Inventory;
using Gameplay.Network.NetworkEventHandlers;
using PlayVibe.RolePopup;
using PlayVibe.Subclass.CharacterInventoryPopupElements;
using Services.Gameplay;
using Services.Gameplay.Craft;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe.Subclass
{
    public class CharacterInventoryPopup : InventoryPopup
    {
        [SerializeField] private SecuritySlot securitySlot;
        [SerializeField] private RectTransform canCraftBar;
        [SerializeField] private GameObject trashCanSlot;

        [Inject] private CraftBank craftBank;
        
        private CancellationTokenSource canCraftTokenSource;
        private readonly List<CanCraftButton> canCraftButtons = new();
        
        protected override bool DragAllowed => true;
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.Character;
        public SecuritySlot SecuritySlot => securitySlot;

        protected override UniTask OnShow(object data = null)
        {
            base.OnShow(data);
            
            trashCanSlot.SetActive(gameplayStage.GameplayDataDic[popupData.OwnerId].RoleType == RoleType.Prisoner);
            
            return UniTask.CompletedTask;
        }
        
        protected override UniTask OnHide()
        {
            base.OnHide();
            
            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Security)
            {
                securitySlot.Deinitialize();
            }

            ClearCanCraftBar();
            
            return UniTask.CompletedTask;
        }
        
        protected override void OnInitialized()
        {
            Capacity = gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner
                ? balance.Inventory.PrisonerEquipmentCapacity
                : balance.Inventory.SecurityEquipmentCapacity;
            
            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Security)
            {
                securitySlot.gameObject.SetActive(true);
                securitySlot.Initialize(popupData.OwnerId);
            }
            else
            {
                securitySlot.gameObject.SetActive(false);
            }
        }
        
        protected override void Subscribes()
        {
            base.Subscribes();
            
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Alpha1) ||
                            Input.GetKeyDown(KeyCode.Alpha2) ||
                            Input.GetKeyDown(KeyCode.Alpha3) ||
                            Input.GetKeyDown(KeyCode.Alpha4) ||
                            Input.GetKeyDown(KeyCode.Alpha5) ||
                            Input.GetKeyDown(KeyCode.Alpha6) ||
                            Input.GetKeyDown(KeyCode.Alpha7) ||
                            Input.GetKeyDown(KeyCode.Alpha8) ||
                            Input.GetKeyDown(KeyCode.Alpha9) ||
                            Input.GetKeyDown(KeyCode.Alpha0))
                .Subscribe(_ => CheckAndProcessInput())
                .AddTo(CompositeDisposable);
        }
        
        protected override void OnSlotHasBeenClicked(ItemTransitionData itemTransitionData)
        {
            if (OnUseSlot(itemTransitionData.FromSlot.Index))
            {
                return;
            }
            
            base.OnSlotHasBeenClicked(itemTransitionData);
        }

        private bool OnUseSlot(int index)
        {
            if (index == -1)
            {
                return false;
            }

            var data = gameplayStage.LocalGameplayData;
            var view = data.CharacterView;

            if (view == null || view.IsBusy)
            {
                return false;
            }

            var slot = slots.FirstOrDefault(x => x.Index == index);

            if (slot == null || slot.IsEmpty)
            {
                return false;
            }

            var itemModel = slot.CurrentItem.ItemModel;

            itemsSettings.Data.TryGetValue(itemModel.ItemKey, out var itemData);

            if (itemData == null)
            {
                return false;
            }

            if (!itemData.IsConsumable)
            {
                return false;
            }

            if (!itemData.AvailableFor.Contains(data.RoleType))
            {
                return false;
            }

            slot.DropItem();

            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendUseConsumableItem(itemModel);

            return true;
        }

        protected override IEnumerable<ItemModel> GetItemsFromSource()
        {
            return popupData.Items;
        }

        protected override void OnRefreshed()
        {
            base.OnRefreshed();

            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
            {
                canCraftTokenSource?.Dispose();
                canCraftTokenSource = new CancellationTokenSource();
            
                RefreshCanCraftBar(canCraftTokenSource.Token).Forget();
            }
        }

        private void CheckAndProcessInput()
        {
            int number;

            if (Input.GetKeyDown(KeyCode.Alpha1)) number = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) number = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) number = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) number = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) number = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) number = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha7)) number = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha8)) number = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha9)) number = 8;
            else if (Input.GetKeyDown(KeyCode.Alpha0)) number = 9;
            else return;

            if (number > Capacity - 1)
            {
                return;
            }

            OnUseSlot(number);

            OnRefreshed();
        }

        private async UniTask RefreshCanCraftBar(CancellationToken token)
        {
            ClearCanCraftBar();

            var itemsDictionary = PopupData.Items
                .Select(x => x.ItemKey)
                .Where(key => !string.IsNullOrEmpty(key))
                .GroupBy(key => key)
                .ToDictionary(g => g.Key, g => g.Count());
            
            foreach (var craftPair in craftBank.Data)
            {
                if (canCraftButtons.Any(x => x.Key == craftPair.ItemKey))
                {
                    continue;
                }

                var allElementsExist = craftPair.ComponentsKeys
                    .Where(key => !string.IsNullOrEmpty(key))
                    .GroupBy(key => key)
                    .All(g => itemsDictionary.TryGetValue(g.Key, out var count) && count >= g.Count());
                
                if (!allElementsExist)
                {
                    continue;
                }
                
                var view = await objectPoolService.GetOrCreateView<CanCraftButton>(Constants.Views.CanCraftButton, canCraftBar, true);
                
                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(view);
                    return;
                }
                
                diContainer.Inject(view);
                canCraftButtons.Add(view);
                
                view.Setup(craftPair.ItemKey);
            }
        }

        private void ClearCanCraftBar()
        {
            foreach (var button in canCraftButtons)
            {
                objectPoolService.ReturnToPool(button);
            }
            
            canCraftButtons.Clear();
        }
        
        protected override bool AdditiveCheckApplyTransition(ItemTransitionData itemTransitionData, ItemTransitionRequestData requestTransitionData)
        {
            var baseResult = base.AdditiveCheckApplyTransition(itemTransitionData, requestTransitionData);
            
            if (requestTransitionData.ToInventoryType == InventoryType.Seized)
            {
                return baseResult && SecuritySlot.HasPlace;
            }
            
            return baseResult;
        }
    }
}