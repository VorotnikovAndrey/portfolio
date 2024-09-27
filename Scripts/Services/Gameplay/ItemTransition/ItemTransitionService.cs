using System;
using System.Linq;
using Gameplay;
using Gameplay.Inventory;
using PlayVibe;
using PlayVibe.RolePopup;
using PlayVibe.Subclass;
using Zenject;

namespace Services.Gameplay
{
    public class ItemTransitionService
    {
        [Inject] private GameplayStage gameplayStage;
        [Inject] private PopupService popupService;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private ObjectPoolService objectPoolService;
        
        public ItemTransitionResult FakeTransition(ItemTransitionData data)
        {
            if (itemsSettings.Data[data.ItemView.ItemModel.ItemKey].Key == "SoftCoin")
            {
                objectPoolService.ReturnToPool(data.ItemView);
                
                return ItemTransitionResult.Successfully;
            }
            
            if (data.ToSlot.InventoryType is InventoryType.MapItemBox or InventoryType.Drop)
            {
                return ItemTransitionResult.IsNotPossible;
            }

            if (data.ToSlot == data.FromSlot && data.ToSlot.InventoryType == data.FromSlot.InventoryType)
            {
                return ItemTransitionResult.IsNotPossible;
            }

            if (data.FromSlot.InventoryType == InventoryType.MapItemBox && data.ToSlot.CurrentItem != null)
            {
                return ItemTransitionResult.IsNotPossible;
            }

            if (data.ToSlot.InventoryType == InventoryType.Recycler)
            {
                gameplayStage.LevelData.RecyclersData.TryGetValue(data.ToSlot.OwnerId, out var recyclerData);
                
                if (recyclerData != null)
                {
                    if (recyclerData.Enable == false)
                    {
                        return ItemTransitionResult.IsNotPossible;
                    }
                }
            }

            if (data.ToSlot.InventoryType == InventoryType.LootBox && gameplayStage.LocalGameplayData.ActorNumber != data.ToSlot.OwnerId)
            {
                return ItemTransitionResult.IsNotPossible;
            }
            
            if (data.ToSlot.InventoryType is InventoryType.Seized)
            {
                objectPoolService.ReturnToPool(data.ItemView);
                
                return ItemTransitionResult.Successfully;
            }

            if (data.ToSlot.CurrentItem != null &&
                data.FromSlot.InventoryType != InventoryType.MapItemBox &&
                data.ToSlot.InventoryType != InventoryType.MapItemBox)
            {
                var itemViewFrom = data.FromSlot.CurrentItem;
                var itemViewTo = data.ToSlot.CurrentItem;
                
                data.FromSlot.DropItem(DropReason.ChangeSlot);
                data.ToSlot.DropItem(DropReason.ChangeSlot);
                data.FromSlot.TrySnap(itemViewTo);
                data.ToSlot.TrySnap(itemViewFrom);
            }
            else
            {
                data.FromSlot.DropItem(DropReason.ChangeSlot);
                data.ToSlot.TrySnap(data.ItemView);
            }

            return ItemTransitionResult.Successfully;
        }
        
        public ItemTransitionResult MasterTransition(ItemTransitionRequestData transition, out AbstractInventory fromInventory, out AbstractInventory toInventory)
        {
            MasterTransitionFindInventories(transition, out fromInventory, out toInventory);

            if (fromInventory == null || toInventory == null)
            {
                return ItemTransitionResult.InventoryDoesNotExist;
            }

            if (fromInventory.InventoryType == InventoryType.Seized || toInventory.InventoryType == InventoryType.Seized)
            {
                return MasterTransitionDefault(transition, fromInventory, toInventory);
            }

            return toInventory.IsFreeSlot(transition.ToSlot) ?
                MasterTransitionDefault(transition, fromInventory, toInventory) :
                MasterTransitionSwap(transition, fromInventory, toInventory);
        }

        private void MasterTransitionFindInventories(ItemTransitionRequestData transition, out AbstractInventory fromInventory, out AbstractInventory toInventory)
        {
            fromInventory = null;
            toInventory = null;

            fromInventory = transition.FromInventoryType switch
            {
                InventoryType.Character => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Character],
                InventoryType.LootBox => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.LootBox],
                InventoryType.Secret => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Secret],
                InventoryType.Seized => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Seized],
                InventoryType.MapItemBox => gameplayStage.MasterData.MapItemBoxesItems[transition.FromNetworkId],
                InventoryType.Recycler => gameplayStage.MasterData.RecyclerInventories[transition.FromNetworkId],
                InventoryType.Trade => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Trade],
                InventoryType.Drop => gameplayStage.MasterData.GetDropInventory(transition.FromNetworkId),
                _ => fromInventory
            };
            
            toInventory = transition.ToInventoryType switch
            {
                InventoryType.Character => gameplayStage.GameplayDataDic[transition.ToNetworkId].Inventories[InventoryType.Character],
                InventoryType.LootBox => gameplayStage.GameplayDataDic[transition.ToNetworkId].Inventories[InventoryType.LootBox],
                InventoryType.Secret => gameplayStage.GameplayDataDic[transition.ToNetworkId] .Inventories[InventoryType.Secret],
                InventoryType.Seized => gameplayStage.GameplayDataDic[transition.ToNetworkId] .Inventories[InventoryType.Seized],
                InventoryType.MapItemBox => gameplayStage.MasterData.MapItemBoxesItems[transition.ToNetworkId],
                InventoryType.Recycler => gameplayStage.MasterData.RecyclerInventories[transition.ToNetworkId],
                InventoryType.Trade => gameplayStage.GameplayDataDic[transition.ToNetworkId].Inventories[InventoryType.Trade],
                InventoryType.Drop => gameplayStage.MasterData.GetDropInventory(transition.ToNetworkId),
                _ => toInventory
            };
        }

        private ItemTransitionResult MasterTransitionDefault(ItemTransitionRequestData transition, AbstractInventory fromInventory, AbstractInventory toInventory)
        {
            switch (transition.FromInventoryType)
            {
                case InventoryType.Character:
                case InventoryType.LootBox:
                case InventoryType.MapItemBox:
                case InventoryType.Secret:
                case InventoryType.Recycler:
                case InventoryType.Trade:
                case InventoryType.Drop:
                {
                    var itemModel = fromInventory.Items.FirstOrDefault(x => x.Slot == transition.FromSlot);

                    if (itemModel == null)
                    {
                        return ItemTransitionResult.ItemDoesNotExist;
                    }

                    break;
                }
                case InventoryType.Seized:
                {
                    var itemModel = fromInventory.Items.FirstOrDefault(x => x.NetworkId == transition.ItemModel.NetworkId);

                    if (itemModel == null)
                    {
                        return ItemTransitionResult.ItemDoesNotExist;
                    }
                    
                    transition.ToSlot = toInventory.GetFreeSlot();
                    
                    if (transition.ToSlot == -1)
                    {
                        return ItemTransitionResult.IsNotPossible;
                    }
                    
                    break;
                }
                default: return ItemTransitionResult.IsNotPossible;
            }

            switch (transition.ToInventoryType)
            {
                case InventoryType.Character:
                case InventoryType.LootBox:
                case InventoryType.Secret:
                case InventoryType.Recycler:
                case InventoryType.Trade:
                {
                    if (!toInventory.IsFreeSlot(transition.ToSlot))
                    {
                        return ItemTransitionResult.SlotOccupied;
                    }

                    break;
                }
                case InventoryType.Seized:
                {
                    if (!toInventory.HasPlace)
                    {
                        return ItemTransitionResult.SendRefreshInventory;
                    }
                    
                    break;
                }
                case InventoryType.MapItemBox:
                case InventoryType.Drop:
                
                default: return ItemTransitionResult.IsNotPossible;
            }
            
            fromInventory.Remove(transition.ItemModel);
            transition.ItemModel.Slot = transition.ToSlot;
            toInventory.Add(transition.ItemModel);
            
            return ItemTransitionResult.Successfully;
        }
        
        private ItemTransitionResult MasterTransitionSwap(ItemTransitionRequestData transition, AbstractInventory fromInventory, AbstractInventory toInventory)
        {
            if (fromInventory.InventoryType == InventoryType.MapItemBox ||
                toInventory.InventoryType == InventoryType.MapItemBox)
            {
                return ItemTransitionResult.IsNotPossible;
            }
            
            ItemModel fromItemModel;
            ItemModel toItemModel;

            switch (transition.FromInventoryType)
            {
                case InventoryType.Character:
                case InventoryType.LootBox:
                case InventoryType.MapItemBox:
                case InventoryType.Secret:
                case InventoryType.Recycler:
                case InventoryType.Trade:
                case InventoryType.Drop:
                {
                    fromItemModel = fromInventory.Items.FirstOrDefault(x => x.Slot == transition.FromSlot);

                    if (fromItemModel == null)
                    {
                        return ItemTransitionResult.ItemDoesNotExist;
                    }

                    break;
                }
                default: return ItemTransitionResult.IsNotPossible;
            }

            switch (transition.ToInventoryType)
            {
                case InventoryType.Character:
                case InventoryType.LootBox:
                case InventoryType.Secret:
                case InventoryType.Recycler:
                case InventoryType.Trade:
                case InventoryType.Drop:
                {
                    toItemModel = toInventory.Items.FirstOrDefault(x => x.Slot == transition.ToSlot);

                    if (toItemModel == null)
                    {
                        return ItemTransitionResult.ItemDoesNotExist;
                    }

                    break;
                }
                case InventoryType.MapItemBox:
                default: return ItemTransitionResult.IsNotPossible;
            }

            if (toInventory == fromInventory)
            {
                (fromItemModel.Slot, toItemModel.Slot) = (toItemModel.Slot, fromItemModel.Slot);
            }
            else
            {
                fromInventory.Remove(fromItemModel);
                toInventory.Remove(toItemModel);
                fromInventory.Add(toItemModel);
                toInventory.Add(fromItemModel);
                
                (fromItemModel.Slot, toItemModel.Slot) = (toItemModel.Slot, fromItemModel.Slot);
            }
            
            return ItemTransitionResult.Successfully;
        }

        public ItemTransitionRequestData CreateClickTransitionRequestData(ItemTransitionData data)
        {
            var requestData = new ItemTransitionRequestData
            {
                ItemModel = data.ItemView.ItemModel,
                FromInventoryType = data.FromSlot.InventoryType,
                FromSlot = data.FromSlot.Index,
                FromNetworkId = data.FromSlot.OwnerId
            };

            switch (data.FromSlot.InventoryType)
            {
                case InventoryType.Character:
                {
                    ItemTransitionRequestData result = null;
                    InventoryPopup popup = null;
                    
                    popup = getPopup(InventoryType.LootBox);
                    result = tryPlace(popup);

                    if (result != null)
                    {
                        return result;
                    }

                    if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner && gameplayStage.LocalGameplayData.LootBoxUpgraded)
                    {
                        popup = getPopup(InventoryType.Secret);
                        result = tryPlace(popup, true);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                    
                    popup = getPopup(InventoryType.Recycler);
                    result = tryPlace(popup);
                    
                    if (result != null)
                    {
                        return result;
                    }
                    
                    popup = getPopup(InventoryType.Trade);
                    result = tryPlace(popup);
                    
                    if (result != null)
                    {
                        return result;
                    }
                    
                    popup = getPopup(InventoryType.Drop);
                    result = tryPlace(popup);
                    
                    if (result != null)
                    {
                        return result;
                    }
                    
                    return null;
                }
                default:
                {
                    var popup = getPopup(InventoryType.Character);
                    
                    return tryPlace(popup);
                }
            }

            ItemTransitionRequestData tryPlace(InventoryPopup popup, bool isSecret = false)
            {
                if (popup == null)
                {
                    return null;
                }

                var popupData = popup.PopupData;
                
                if (itemsSettings.Data[data.ItemView.ItemModel.ItemKey].Key == "SoftCoin")
                {
                    requestData.ToInventoryType = InventoryType.Character;
                    requestData.ToNetworkId = popupData.OwnerId;
                    
                    return requestData;
                }
                
                requestData.ToInventoryType = isSecret ? InventoryType.Secret : popupData.InventoryType;
                requestData.ToNetworkId = popupData.OwnerId;

                if (isSecret)
                {
                    var secret = (popup as PrisonerLootBoxInventoryPopup).SecretSlot;

                    requestData.ToSlot = secret.Index;
                    data.ToSlot = secret;
                }
                else
                {
                    requestData.ToSlot = popup.GetFirstFreeSlot()?.Index ?? -1;
                    data.ToSlot = popup.GetSlot(requestData.ToSlot);
                }

                if (requestData.ToSlot == -1 || data.ToSlot.CurrentItem != null)
                {
                    return null;
                }

                return data.ToSlot == null ? null : requestData;
            }

            InventoryPopup getPopup(InventoryType inventoryType)
            {
                InventoryPopup popup = null;
                    
                switch (inventoryType)
                {
                    case InventoryType.Character:
                        popup = popupService.GetPopups<CharacterInventoryPopup>(Constants.Popups.Inventory.CharacterInventoryPopup)?.FirstOrDefault();
                        break;
                    case InventoryType.LootBox:
                        if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
                        {
                            popup = popupService.GetPopups<PrisonerLootBoxInventoryPopup>(Constants.Popups.Inventory.PrisonerLootBoxPopup)?.FirstOrDefault();
                        }
                        else
                        {
                            popup = popupService.GetPopups<SecurityLootBoxInventoryPopup>(Constants.Popups.Inventory.SecurityLootBoxPopup)?.FirstOrDefault();
                        }
                        break;
                    case InventoryType.Secret:
                        if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
                        {
                            popup = popupService.GetPopups<PrisonerLootBoxInventoryPopup>(Constants.Popups.Inventory.PrisonerLootBoxPopup)?.FirstOrDefault();
                        }
                        break;
                    case InventoryType.Seized:
                        break;
                    case InventoryType.Recycler:
                        popup = popupService.GetPopups<RecyclerInventoryPopup>(Constants.Popups.Inventory.RecyclerInventoryPopup)?.FirstOrDefault();
                        break;
                    case InventoryType.Trade:
                        popup = popupService.GetPopups<TradePopup>(Constants.Popups.Inventory.TradePopup)?.FirstOrDefault();
                        break;
                    case InventoryType.MapItemBox:
                    case InventoryType.Drop:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inventoryType), inventoryType, null);
                }

                return popup;
            }
        }
        
        public void FindInventory(ItemTransitionRequestData transition, out AbstractInventory inventory)
        {
            inventory = null;

            inventory = transition.FromInventoryType switch
            {
                InventoryType.Character => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Character],
                InventoryType.LootBox => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.LootBox],
                InventoryType.Secret => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Secret],
                InventoryType.Seized => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Seized],
                InventoryType.MapItemBox => gameplayStage.MasterData.MapItemBoxesItems[transition.FromNetworkId],
                InventoryType.Recycler => gameplayStage.MasterData.RecyclerInventories[transition.FromNetworkId],
                InventoryType.Trade => gameplayStage.GameplayDataDic[transition.FromNetworkId].Inventories[InventoryType.Trade],
                InventoryType.Drop => gameplayStage.MasterData.GetDropInventory(transition.FromNetworkId),
                _ => inventory
            };
        }
    }
}
