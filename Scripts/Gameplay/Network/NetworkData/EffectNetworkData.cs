using System;
using Gameplay.Player.Effects;
using UnityEngine.Serialization;

namespace Gameplay.Network.NetworkData
{
    [Serializable]
    public class EffectNetworkData
    {
        public EffectType EffectType;
        [FormerlySerializedAs("Owner")] public int Target;
    }
}