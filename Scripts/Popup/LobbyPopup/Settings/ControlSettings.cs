using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayVibe
{
    [Serializable]
    public class ControlSettings
    {
        public Dictionary<ControlType, KeyCode> Data = new()
        {
            { ControlType.Interact, KeyCode.F },
            { ControlType.Affect, KeyCode.E },
            { ControlType.Map, KeyCode.M },
            { ControlType.CraftOrQuests, KeyCode.Tab },
            { ControlType.SpellShop, KeyCode.B },
            { ControlType.Spell1, KeyCode.Q },
            { ControlType.Spell2, KeyCode.R },
            { ControlType.Spell3, KeyCode.T },
            { ControlType.Spell4, KeyCode.G },
        };
    }
}