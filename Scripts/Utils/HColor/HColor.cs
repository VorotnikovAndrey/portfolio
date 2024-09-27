using System;
using UnityEngine;

namespace PlayVibe
{
    public interface HColor
    {
        public HColorData HColorData { get; }
    }

    [Serializable]
    public class HColorData
    {
        public static readonly Color DefaultTextColor = ColorUtility.HexToColor("#DE3163");
        public static readonly Color DefaultBackgroundColor = ColorUtility.HexToColor("#383838");

        public Color TextColor = DefaultTextColor;
        public Color BackgroundColor = DefaultBackgroundColor;
        public FontStyle FontStyle = FontStyle.Bold;
    }
}
