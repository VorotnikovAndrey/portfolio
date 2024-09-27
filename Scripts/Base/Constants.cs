namespace PlayVibe
{
    public static class Constants
    {
        public static class Global
        {
            public const uint SteamAppId = 480;
            public const int MaxNicknameLength = 15;
        }
        
        public static class Colors
        {
            public const string FriendInGameColor = "#7aeb34";
            public const string FriendOnline = "#7aeb34";
            public const string FriendInAnotherGameColor = "#34a4eb";
        }
        
        public static class Room
        {
            public static class CustomProperties
            {
                public const string Password = "password";
                public const string Owner = "owner";
                public const string Region = "region";
                public const string EnableAdminPopup = "enableAdminPopup";
                public const string AutoRoleBalanceEnabled  = "autoRoleBalanceEnabled";
                public const string SelectedLocation  = "selectedLocation";
            }
        }
        
        public static class PlayerPrefs
        {
            public static class User
            {
                public const string ControlSettingsKey = nameof(ControlSettingsKey);
                public const string PrevRoomName = nameof(PrevRoomName);
                public const string PreRegion = nameof(PreRegion);
            }
        }
        
        public static class Stages
        {
            public const string Main = nameof(Main);
            public const string Gameplay = nameof(Gameplay);
        }

        public static class Formats
        {
            public const string ReactivateRecycler = "ReactivateRecycler_{0}";
        }
        
        public static class Popups
        {
            public static class Inventory
            {
                public const string CharacterInventoryPopup = nameof(CharacterInventoryPopup);
                public const string MapItemBoxPopup = nameof(MapItemBoxPopup);
                public const string PrisonerLootBoxPopup = nameof(PrisonerLootBoxPopup);
                public const string SecurityLootBoxPopup = nameof(SecurityLootBoxPopup);
                public const string RecyclerInventoryPopup = nameof(RecyclerInventoryPopup);
                public const string TradePopup = nameof(TradePopup);
                public const string DropItemBoxPopup = nameof(DropItemBoxPopup);
            }

            public static class Minigames
            {
                public const string MinigameDefaultPopup = nameof(MinigameDefaultPopup);
            }
            
            public static class Actions
            {
                public const string ActionDefaultPopup = nameof(ActionDefaultPopup);
            }
            
            public const string MainPopup = nameof(MainPopup);
            public const string ConnectionPopup = nameof(ConnectionPopup);
            public const string LobbyPopup = nameof(LobbyPopup);
            public const string InfoPopup = nameof(InfoPopup);
            public const string PasswordPopup = nameof(PasswordPopup);
            public const string NetworkLoadingPopup = nameof(NetworkLoadingPopup);
            public const string ChatPopup = nameof(ChatPopup);
            public const string NicknamePopup = nameof(NicknamePopup);
            public const string GameplayReadyPopup = nameof(GameplayReadyPopup);
            public const string RolePopup = nameof(RolePopup);
            public const string RegionPopup = nameof(RegionPopup);
            public const string TabPopup = nameof(TabPopup);
            public const string CurrenciesPopup = nameof(CurrenciesPopup);
            public const string SelfCraftPopup = nameof(SelfCraftPopup);
            public const string GameplayHudPopup = nameof(GameplayHudPopup);
            public const string GameplaySettingsPopup = nameof(GameplaySettingsPopup);
            public const string QuestsPopup = nameof(QuestsPopup);
            public const string TimeDayPopup = nameof(TimeDayPopup);
            public const string QuestsIndicatorPopup = nameof(QuestsIndicatorPopup);
            public const string MapPopup = nameof(MapPopup);
            public const string FriendsPopup = nameof(FriendsPopup);
            public const string WinPopup = nameof(WinPopup);
            public const string SpectatorPopup = nameof(SpectatorPopup);
            public const string AdminPopup = nameof(AdminPopup);
            public const string StaminaPopup = nameof(StaminaPopup);
            public const string EffectsPopup = nameof(EffectsPopup);
            public const string ConfirmPopup = nameof(ConfirmPopup);
            public const string TradeConfirmPopup = nameof(TradeConfirmPopup);
            public const string TradeWaitPopup = nameof(TradeWaitPopup);
            public const string AnyKeyPressPopup = nameof(AnyKeyPressPopup);
            public const string ItemsStatisticPopup = nameof(ItemsStatisticPopup);
            public const string ArrestPopup = nameof(ArrestPopup);
            public const string SpellShopPopup = nameof(SpellShopPopup);
            public const string SpellsHudPopup = nameof(SpellsHudPopup);
        }

        public static class Views
        {
            public const string LobbyRoomUserContainer = nameof(LobbyRoomUserContainer);
            public const string RoomContainer = nameof(RoomContainer);
            public const string ChatMessageContainer = nameof(ChatMessageContainer);
            public const string GameplayReadyContainer = nameof(GameplayReadyContainer);
            public const string LocationCameraController = nameof(LocationCameraController);
            public const string ItemView = nameof(ItemView);
            public const string SelfCraftContainer = nameof(SelfCraftContainer);
            public const string CanCraftButton = nameof(CanCraftButton);
            public const string QuestContainer = nameof(QuestContainer);
            public const string QuestsIndicatorView = nameof(QuestsIndicatorView);
            public const string FriendContainer = nameof(FriendContainer);
            public const string EffectContainer = nameof(EffectContainer);
            public const string ItemStatisticContainer = nameof(ItemStatisticContainer);
            public const string ItemStatisticDayButton = nameof(ItemStatisticDayButton);
            public const string ItemStatisticPresetContainer = nameof(ItemStatisticPresetContainer);
            public const string OtherInventoriesButton = nameof(OtherInventoriesButton);
            public const string SpellContainer = nameof(SpellContainer);
            public const string SpellShopContainer = nameof(SpellShopContainer);
        }
        
        public static class Scenes
        {
            public const string Main = nameof(Main);
            public const string Lobby = nameof(Lobby);
        }
        
        public static class Resources
        {
            public static class Gameplay
            {
                public const string CharacterView = "Characters/CharacterView";
                public const string PrisonerLootBoxView = "LootBoxes/PrisonerLootBoxView";
                public const string SecurityLootBoxView = "LootBoxes/SecurityLootBoxView";
                public const string DropInteractiveObject = "SpawnObjects/DropInteractiveObject";
                public const string DynamiteInteractiveObject = "SpawnObjects/DynamiteInteractiveObject";
                public const string TrapInteractiveObject = "SpawnObjects/TrapInteractiveObject";
            } 
            
            public static class VFX
            {
                public const string DynamiteExplosionVFX = "NetworkVFX/DinamiteExplosionVFX";
                public const string TeleportVFX = "NetworkVFX/TeleportVFX";
            }
        }

        public static class Messages
        {
            public static class Trade
            {
                public const string StartTrade = "Player {0} offers you a trade.";
                public const string CancelTrade = "Player {0} has canceled the trade.";
                public const string DontHasPlace = "Player {0} dont has place for the trade.";
                public const string IsBusy = "Player {0} is busy.";
                public const string WaitConfirmTimeOut = "Player {0} did not respond to the request.";
                public const string WaitResponse = "Waiting for trade acceptance from Player {0}.";
            }
            
            public static class Info
            {
                public const string NotEnoughCurrencyForArrest = "Not enough currency to inspect the prisoner. Price {0}";
                public const string InsufficientInventorySpace = "Insufficient inventory space for arrest and item confiscation";
                public const string HasNoProhibitedItems = "The player {0} has no prohibited items detected";
                public const string RestroomIsNotAvailable = "The restroom is available only at night";
            }
        }
        
        public static class Keys
        {
            public static class Busy
            {
                public const string WaitConfirm = nameof(WaitConfirm);
                public const string InTradeConfirm = nameof(InTradeConfirm);
                public const string InTrade = nameof(InTrade);
                public const string InInteractiveInventoryPopup = nameof(InInteractiveInventoryPopup);
                public const string InMapPopup = nameof(InMapPopup);
                public const string InSelfCraftPopup = nameof(InSelfCraftPopup);
            }
        }
    }
}

