using System;

namespace Services
{
    [Serializable]
    public enum PhotonPeerEvents
    {
        // Chat
        SendChatMessageEvent,
        
        // Gameplay
        LoadLevelEvent,
        GameplayControllerInitialized,
        StartGameplay,
        NextDayTime,
        PrisonerEscaped,
        WinBehavior,
        SendMessage,
        
        // Role
        SelectRole,
        ShowRolePopup,
        BalanceRoles,
        
        // Character
        CreateCharacter,
        WarpToSpawnPoint,
        AddMarker,
        RemoveMarker,
        Arrest,
        SendArrestData,
        ImReady,
        SystemArrest,
        
        // Spawn
        SpawnDrop,
        SpawnPhotonView,
        
        // Inventory
        TryUpgradeLootBox,
        PlayerUpgradedLootBox,
        GetMapItemBoxItemsRequest,
        GetRecyclersItemsRequest,
        ShowPersonalLootBoxRequest,
        TransitionItem,
        RefreshInventories,
        ConfiscateInventory,
        TryRunRecycler,
        RunRecycler,
        TrySendRandomSeizedItemToRecycler,
        ReactivateRecycler,
        TrySendRandomSeizedItemToLootBox,
        DropItem,
        RemoveItem,
        HasItem,
        TryUseItem,
        CreateItemFor,
        ClearInventory,
        UseConsumableItem,
        OfferTrade,
        InitializeTrade,
        ShowTradePopup,
        CancelTrade,
        InterruptTrade,
        ConfirmTrade,
        UpdateTradeItems,
        GetTradeItems,
        SuccessfulTrade,
        CheckPlaceForTrade,
        GetDropItems,
        
        // Wallet
        ModifyCurrency,
        
        // Craft
        TryCraftItem,
        
        // Minigames
        CompleteMinigame,
        ReceiveQuests,
        
        // Effects
        AddEffect,
        RemoveEffect,
        RemoveAllEffects,
        
        // Spells
        TryBuySpell,
        AddSpell,
        
        // Debug
        GetStatistic
    }
}