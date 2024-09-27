using System;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Services.Gameplay.Craft
{
    [CreateAssetMenu(fileName = "CraftBank", menuName = "SO/CraftBank")]
    public class CraftBank : ScriptableObject
    {
        [SerializeField] private ItemsSettings itemsSettings; 
        [Space]
        [SerializeField] private List<CraftBankPair> data;

        public ItemsSettings ItemsSettings => itemsSettings;
        public List<CraftBankPair> Data => data;
        
        [Serializable]
        public class CraftBankPair
        {
            public string ItemKey;
            public List<string> ComponentsKeys;
        }
    }
}