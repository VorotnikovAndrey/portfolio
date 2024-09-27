using System;
using System.Collections.Generic;
using PlayVibe.RolePopup;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Player.Spells
{
    [Serializable]
    public class SpellData
    {
        public SpellType SpellType;
        public List<RoleType> AvailableFor = new();
        [PreviewField]
        public Sprite Icon;
        public bool Enable = true;
        public float Cooldown = 10;
        public int Price = 200;
    }
}