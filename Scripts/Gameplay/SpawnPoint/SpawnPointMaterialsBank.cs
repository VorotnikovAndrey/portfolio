using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Player.SpawnPoint
{
    [CreateAssetMenu(fileName = "SpawnPointMaterialsBank", menuName = "SO/SpawnPointMaterialsBank")]
    public class SpawnPointMaterialsBank : ScriptableObject
    {
        [SerializeField] private List<SpawnPointMaterialData> data;

        public Material GetMaterial(SpawnPointType type)
        {
            return data.FirstOrDefault(x => x.Type == type)?.Material;
        }
        
        [Serializable]
        public class SpawnPointMaterialData
        {
            public SpawnPointType Type;
            public Material Material;
        }
    }
}