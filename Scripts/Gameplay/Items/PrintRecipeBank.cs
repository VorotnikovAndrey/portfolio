using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Items
{
    [CreateAssetMenu(fileName = "PrintRecipeBank", menuName = "SO/PrintRecipeBank")]
    public class PrintRecipeBank : ScriptableObject
    {
        [SerializeField] private ItemsSettings itemsSettings; 
        [Space]
        [SerializeField] private List<PrintRecipePair> data;

        public ItemsSettings ItemsSettings => itemsSettings;
        public List<PrintRecipePair> Data => data;
        
        [Serializable]
        public class PrintRecipePair
        {
            public string ItemKey;
            public List<string> ComponentsKeys;
        }
    }
}