using System.Collections.Generic;
using PlayVibe.RolePopup;
using UnityEngine;

namespace Gameplay.Player.SpawnPoint
{
    public class SpawnPointHandler : MonoBehaviour
    {
        [SerializeField] private List<SpawnPoint> spawnPoints;

        public Dictionary<SpawnPointType, List<SpawnPoint>> SpawnPointsDictionary { get; } = new();

        private void Start()
        {
            UpdateSpawnPointsDictionary();
        }
        
        public void SetSpawnPoints(List<SpawnPoint> newSpawnPoints)
        {
            spawnPoints = newSpawnPoints;

            UpdateSpawnPointsDictionary();
        }

        public IEnumerable<SpawnPoint> GetRoomDic(RoleType roleType)
        {
            return roleType == RoleType.Prisoner
                ? SpawnPointsDictionary[SpawnPointType.PrisonerRoom]
                : SpawnPointsDictionary[SpawnPointType.SecurityRoom];
        }
        
        public IEnumerable<SpawnPoint> GetLootBoxDic(RoleType roleType)
        {
            return roleType == RoleType.Prisoner
                ? SpawnPointsDictionary[SpawnPointType.PrisonerLootBox]
                : SpawnPointsDictionary[SpawnPointType.SecurityLootBox];
        }
        
        private void UpdateSpawnPointsDictionary()
        {
            SpawnPointsDictionary.Clear();

            foreach (var spawnPoint in spawnPoints)
            {
                if (!SpawnPointsDictionary.ContainsKey(spawnPoint.SpawnPointType))
                {
                    SpawnPointsDictionary[spawnPoint.SpawnPointType] = new List<SpawnPoint>();
                }

                SpawnPointsDictionary[spawnPoint.SpawnPointType].Add(spawnPoint);
            }
        }
    }
}