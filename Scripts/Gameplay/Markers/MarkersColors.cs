using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Player.Markers
{
    [CreateAssetMenu(fileName = "MarkersColors", menuName = "SO/MarkersColors")]
    public class MarkersColors : ScriptableObject
    {
        [SerializeField] private List<MarkersColorsData> data;

        public Color Get(MarkerType type)
        {
            return data.FirstOrDefault(x => x.Type == type).Color;
        }
    }

    [Serializable]
    public class MarkersColorsData
    {
        public MarkerType Type;
        public Color Color;
    }
}