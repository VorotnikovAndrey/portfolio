using DG.Tweening;
using UnityEngine;

namespace Source
{
    public static class FloatExtensions
    {
        public static float ClampToDegrees(this float f)
        {
            f = f % 360;
            if (f < 0)
                f += 360;
            return f;
        }

        public static float Clamp(this float f, float min, float max)
        {
            return Mathf.Clamp(f, min, max);
        }

        public static float ClampBottom(this float f, float minimum)
        {
            return f < minimum ? minimum : f;
        }

        public static float ClampTop(this float f, float maximum)
        {
            return f > maximum ? maximum : f;
        }

        public static float Clamp01(this float f)
        {
            return Mathf.Clamp01(f);
        }


        public static Tweener DoBlendable(this float from, float to, float duration, TweenCallback<float> deltaHandler)
        {
            var blendableValue = from;
            var tween = DOVirtual.Float(from, to, duration, value =>
            {
                var delta = value - blendableValue;
                blendableValue = value;
                deltaHandler(delta);
            });
            return tween;
        }
        
        public static float RoundTo(this float f, float round)
        {
            f *= 1 / round;
            f = Mathf.Round(f);
            f /= 1 / round;
            return f;
        }
    }
}
