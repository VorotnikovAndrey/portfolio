using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Player.Effects
{
    [Serializable]
    public abstract class EffectData
    {
        public float Duration;
        public Color BackgroundColor = Color.white;
        [PreviewField] public Sprite Icon;
    }
}