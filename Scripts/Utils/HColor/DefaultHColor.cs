using UnityEngine;

namespace PlayVibe
{
    public sealed class DefaultHColor : MonoBehaviour, HColor
    {
        [SerializeField] private HColorData hColorData = new();
        
        public HColorData HColorData => hColorData;
    }
}