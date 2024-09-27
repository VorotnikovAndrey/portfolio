using UnityEngine;

namespace Source
{
    public static class VectorExtensions
    {
        public static Vector3 ClampTop(this Vector3 v, float maxLength)
        {
            return Vector3.ClampMagnitude(v, maxLength);
        }

        public static Vector3 ClampBottom(this Vector3 v, float minLength)
        {
            var m = v.magnitude;
            return v.normalized * m.ClampBottom(minLength);
        }

        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.x = x;
            return v;
        }

        public static Vector3 SetY(this Vector3 v, float y)
        {
            v.y = y;
            return v;
        }

        public static Vector3 SetZ(this Vector3 v, float z)
        {
            v.z = z;
            return v;
        }
    }
}
