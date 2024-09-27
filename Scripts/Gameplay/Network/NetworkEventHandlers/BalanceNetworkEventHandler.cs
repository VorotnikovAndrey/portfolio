using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Inventory;
using Gameplay.Player.SpawnPoint;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using Services;
using Services.Gameplay.Wallet;
using Source;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class BalanceNetworkEventHandler : AbstractNetworkEventHandler
    {
        [Inject] private Balance balance;
        [Inject] private SpawnPointHandler spawnPointHandler;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.BalanceRoles] = ReceiveBalancedRoles;
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }

        /// <summary>
        /// Автобалансировка ролей
        /// </summary>
        public void SendMasterBalanceRoles()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            var groups = new Dictionary<RoleType, List<int>>
            {
                { RoleType.Prisoner, new List<int>() },
                { RoleType.Security, new List<int>() }
            };

            foreach (var element in gameplayStage.GameplayDataDic.Values)
            {
                if (element.RoleType is RoleType.None or RoleType.Random)
                {
                    element.RoleType = UnityEngine.Random.Range(0, 2) == 0 ? RoleType.Prisoner : RoleType.Security;
                }
                
                groups[element.RoleType].Add(element.ActorNumber);
            }
            
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Constants.Room.CustomProperties.AutoRoleBalanceEnabled, out var enableAdminPopup);

            if (enableAdminPopup is true)
            {
                if (gameplayStage.GameplayDataDic.Count > 1)
                {
                    var securityLimit = balance.RoleRules.Data.FirstOrDefault(x => x.NumberPlayers == PhotonNetwork.CurrentRoom.PlayerCount)?.SecurityLimit ?? 1;
    
                    while (groups[RoleType.Security].Count < securityLimit)
                    {
                        var actor = groups[RoleType.Prisoner].GetRandom();
                
                        groups[RoleType.Prisoner].Remove(actor);
                        groups[RoleType.Security].Add(actor);
                    }
            
                    while (groups[RoleType.Security].Count > securityLimit)
                    {
                        var actor = groups[RoleType.Security].GetRandom();
                
                        groups[RoleType.Security].Remove(actor);
                        groups[RoleType.Prisoner].Add(actor);
                    }
                }
            }

            var prisonerSpawnPointGroup = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.PrisonerRoom].Select(x => x.PersonalId).ToList();
            var securitySpawnPointGroup = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.SecurityRoom].Select(x => x.PersonalId).ToList();

            foreach (var group in groups)
            {
                foreach (var actorNumber in group.Value)
                {
                    var target = gameplayStage.GameplayDataDic[actorNumber];
                    
                    target.RoleType = group.Key;

                    var spawnPointGroup = target.RoleType == RoleType.Prisoner
                        ? prisonerSpawnPointGroup
                        : securitySpawnPointGroup;
                    
                    var characterSpawnPointIndex = spawnPointGroup.GetRandom();
                    spawnPointGroup.Remove(characterSpawnPointIndex);

                    target.CharacterSpawnPointIndex = characterSpawnPointIndex;
                }
            }
            
            // Подготовка к отправке данных
            
            var sendData = new Dictionary<int, AutoBalanceData.AutoBalanceDataElement>();

            foreach (var element in gameplayStage.GameplayDataDic)
            {
                sendData.Add(element.Key, new AutoBalanceData.AutoBalanceDataElement
                {
                    RoleType = element.Value.RoleType,
                    CharacterSpawnPointIndex = element.Value.CharacterSpawnPointIndex,
                    LootBoxSpawnPointIndex = element.Value.LootBoxSpawnPointIndex,
                });
            }
            
            var eventCode = PhotonPeerEvents.BalanceRoles;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            var data = new AutoBalanceData
            {
                Data = sendData
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);

            GameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendCreateCharacter();
        }

        /// <summary>
        /// Получить и назначить сбалансировнные роли от мастера
        /// </summary>
        private void ReceiveBalancedRoles(PhotonPeerData peerData)
        {
            if (peerData.CustomData is not AutoBalanceData data)
            {
                return;
            }

            foreach (var playerData in data.Data)
            {
                var gameplayData = gameplayStage.GameplayDataDic[playerData.Key];
                
                gameplayData.RoleType = playerData.Value.RoleType;
                gameplayData.CharacterSpawnPointIndex = playerData.Value.CharacterSpawnPointIndex;
                gameplayData.LootBoxSpawnPointIndex = playerData.Value.LootBoxSpawnPointIndex;
                
                gameplayData.Inventories.Add(InventoryType.Character, new CharacterInventory(
                    gameplayData.RoleType == RoleType.Prisoner
                        ? balance.Inventory.PrisonerEquipmentCapacity
                        : balance.Inventory.SecurityEquipmentCapacity,
                    InventoryType.Character,
                    InventoryOwnerType.Player,
                    playerData.Key));
                
                gameplayData.Inventories.Add(InventoryType.LootBox, new LootBoxInventory(
                    gameplayData.RoleType == RoleType.Prisoner
                        ? balance.Inventory.PrisonerLootBoxCapacity
                        : balance.Inventory.SecurityLootBoxCapacity,
                    InventoryType.LootBox,
                    InventoryOwnerType.Player,
                    playerData.Key));
                
                gameplayData.Inventories.Add(InventoryType.Trade, new TradeInventory(
                    4,
                    InventoryType.Trade,
                    InventoryOwnerType.Player,
                    playerData.Key));

                if (gameplayData.RoleType == RoleType.Prisoner)
                {
                    gameplayData.Inventories.Add(InventoryType.Secret, new SecretInventory(
                        1,
                        InventoryType.Secret,
                        InventoryOwnerType.Player,
                        playerData.Key));
                }
                else
                {
                    gameplayData.Inventories.Add(InventoryType.Seized, new SeizedInventory(
                        balance.Inventory.SecuritySeizedInventoryCapacity,
                        InventoryType.Seized,
                        InventoryOwnerType.Player,
                        playerData.Key));
                }
                
                gameplayData.Wallet = new Wallet(gameplayData.RoleType == RoleType.Prisoner
                    ? new Dictionary<CurrencyType, int>
                    {
                        { CurrencyType.Soft, balance.Wallet.PrisonerInitialAmountCurrency}
                    }
                    : new Dictionary<CurrencyType, int>
                    {
                        { CurrencyType.Soft, balance.Wallet.SecurityInitialAmountCurrency}
                    });
            }

            popupService.TryHidePopup(Constants.Popups.RolePopup).Forget();
        }
    }
}