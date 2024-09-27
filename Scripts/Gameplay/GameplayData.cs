using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Player.Markers;
using Gameplay.Player.Quests;
using Gameplay.Player.Spells;
using PlayVibe;
using PlayVibe.RolePopup;
using Services.Gameplay.Wallet;

namespace Gameplay.Player
{
    public class GameplayData
    {
        public string Nickname;
        public GameplayReadyType ReadyStatus = GameplayReadyType.Loading;
        public int ActorNumber;
        public RoleType RoleType;
        public HashSet<MarkerType> Markers = new();
        public AbstractCharacterView CharacterView;
        public AbstractInteractiveObject LootBoxView;
        public int CharacterSpawnPointIndex;
        public int LootBoxSpawnPointIndex;
        public LocationCameraController LocationCamera;
        public List<SpellHandler> SpellHandlers = new();
        
        #region OnlyMaster
        
        public Wallet Wallet;
        public bool SelectRoleReady;
        public bool LootBoxUpgraded;
        public Dictionary<InventoryType, AbstractInventory> Inventories = new();
        public Dictionary<int, QuestData> Quests = new();
        public bool Escaped;

        #endregion
    }
}