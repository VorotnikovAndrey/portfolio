using System;
using UnityEngine;

namespace Gameplay.Character
{
    [Serializable]
    public class CharacterActionData
    {
        public string AnimationKey;
        public float Duration;
        public Vector3 Position;
        public Action Action;
    }
}