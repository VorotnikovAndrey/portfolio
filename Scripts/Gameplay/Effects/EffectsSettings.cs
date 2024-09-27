using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Player.Effects
{
    [CreateAssetMenu(fileName = "EffectsSettings", menuName = "SO/EffectsSettings")]
    public class EffectsSettings : ScriptableObject
    {
        [BoxGroup("Stamina Potion"), HideLabel]
        [SerializeField] private StaminaPotionEffectData staminaPotionData;
        [Space]
        [BoxGroup("Speed Potion"), HideLabel]
        [SerializeField] private SpeedPotionEffectData speedPotionData;
        [Space]
        [BoxGroup("Trap"), HideLabel]
        [SerializeField] private TrapEffectData trapData;
        
        public EffectData Get(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.StaminaPotion: return staminaPotionData;
                case EffectType.SpeedPotion: return speedPotionData;
                case EffectType.Trap: return trapData;
                default: return null;
            }
        }
    }
}