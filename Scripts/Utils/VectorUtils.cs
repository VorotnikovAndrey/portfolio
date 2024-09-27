using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace Defong.Utils
{
    public static class VectorUtils
    {
        public static bool AreEqual(this Vector3 expected, Vector3 actual, float error)
        {
            return FloatComparer.AreEqual(expected.x, actual.x, error)
                   && FloatComparer.AreEqual(expected.y, actual.y, error)
                   && FloatComparer.AreEqual(expected.z, actual.z, error);
        }

        public static bool AreEqual(this Vector2 expected, Vector2 actual, float error)
        {
            return FloatComparer.AreEqual(expected.x, actual.x, error)
                   && FloatComparer.AreEqual(expected.y, actual.y, error);
        }

        public static Vector2 ToVector2xz(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector3 ToVector3xz(this Vector2 vector2)
        {
            return new Vector3(vector2.x, 0f, vector2.y);
        }

        public static Vector3 ToVector3xz(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0f, vector3.z);
        }

        public static Vector3 GetCirclePosition2(Vector3 center, float radius, float ang)
        {
            return new Vector3
            {
                x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad),
                y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad),
                z = center.z
            };
        }

        public static Vector3 GetCirclePosition3(Vector3 center, float radius, float ang)
        {
            return new Vector3
            {
                x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad),
                y = center.y,
                z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad),
            };
        }

        public static Vector3 PointOnCircle2(Vector3 position, float angle, float distance)
        {
            return new Vector3
            (
                position.x + Mathf.Sin(angle) * distance,
                position.y + Mathf.Cos(angle) * distance,
                position.z
            );
        }

        public static Vector3 PointOnCircle3(Vector3 position, Vector3 target, float distance)
        {
            return position + (target - position).normalized * distance;
        }

        public static Vector3 Parabola3D(Vector3 start, Vector3 end, float height, float time)
        {
            float Func(float x) => -4 * height * x * x + 4 * height * x;

            Vector3 mid = Vector3.Lerp(start, end, time);

            return new Vector3(mid.x, Func(time) + Mathf.Lerp(start.y, end.y, time), mid.z);
        }

        public static Vector2 Parabola2D(Vector2 start, Vector2 end, float height, float time)
        {
            float Func(float x) => -4 * height * x * x + 4 * height * x;

            Vector2 mid = Vector2.Lerp(start, end, time);

            return new Vector2(mid.x, Func(time) + Mathf.Lerp(start.y, end.y, time));
        }

        public static Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        public static Vector3 GetCirclePosition3InBounds(Vector3 center, float radius, float ang, Bounds bounds)
        {
            return new Vector3
            {
                x = Mathf.Clamp(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad), bounds.min.x, bounds.max.x),
                y = center.y,
                z = Mathf.Clamp(center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad), bounds.min.z, bounds.max.z)
            };
        }
    }
}
