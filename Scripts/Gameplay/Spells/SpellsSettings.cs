using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Player.Spells
{
    [CreateAssetMenu(fileName = "SpellsSettings", menuName = "SO/SpellsSettings")]
    public class SpellsSettings : ScriptableObject
    {
        [SerializeField] private List<SpellData> data;

        public List<SpellData> Data => data;

        public SpellData GetByType(SpellType type)
        {
            return data.FirstOrDefault(x => x.SpellType == type);
        }
    }
}