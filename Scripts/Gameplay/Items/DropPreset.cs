using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "DropPreset", menuName = "SO/DropPreset")]
    public class DropPreset : ScriptableObject
    {
        public ItemsSettings ItemsSettings;
        
        [Space]
        [SerializeField] private List<DropChanceData> data;

        public List<DropChanceData> Data => data;
        
        public List<string> GetRandomItems(int count, int currentDay)
        {
            if (count <= 0)
            {
                Debug.LogWarning("Count must be greater than zero.");
                return new List<string>();
            }

            var selectedItems = new List<string>();
    
            var filteredData = data
                .Where(d => d.Chance > 0 && currentDay >= d.FromDay && currentDay < d.ToDay)
                .ToList();

            if (!filteredData.Any())
            {
                return new List<string>();
            }

            var totalChance = filteredData.Sum(d => d.Chance);

            for (var i = 0; i < count; i++)
            {
                var randomValue = Random.Range(0f, totalChance);
                var cumulativeChance = 0f;

                foreach (var item in filteredData)
                {
                    cumulativeChance += item.Chance;

                    if (randomValue <= cumulativeChance)
                    {
                        selectedItems.Add(item.ItemKey);
                        
                        break;
                    }
                }
            }

            return selectedItems;
        }
    }
}