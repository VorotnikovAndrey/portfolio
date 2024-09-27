using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Source
{
    public static class ObjectPoolExtensions
    {
        public static T Spawn<T>(this T prefab, Vector3 pos, Quaternion rotation) where T : MonoBehaviour, IMonoPoolObject
        {
            if (CommonLocalPool.Instance != null && CommonLocalPool.AwakeDone)
                return CommonLocalPool.Instance.Spawn(prefab, pos, rotation);

            return Object.Instantiate(prefab, pos, rotation);
        }
        public static T Spawn<T>(this T prefab, Vector3 pos, Vector3 rotation) where T : MonoBehaviour, IMonoPoolObject
        {
            return Spawn(prefab, pos, Quaternion.Euler(rotation));
        }
        public static T Spawn<T>(this T prefab, Vector3 pos) where T : MonoBehaviour, IMonoPoolObject
        {
            return Spawn(prefab, pos, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab) where T : MonoBehaviour, IMonoPoolObject
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        public static void Release(this IMonoPoolObject obj)
        {
            if (CommonLocalPool.Instance != null && CommonLocalPool.AwakeDone)
                CommonLocalPool.Instance.Release(obj);
            if (!Application.isPlaying)
                Object.DestroyImmediate(obj.gameObject);
        }

        public static void AddOnReleaseHandler(this IMonoPoolObject obj, Action<IMonoPoolObject> action)
        {
            if (!CommonLocalPool.HasInstance)
                return;
            CommonLocalPool.Instance.AddReleaseHandler(obj, action);
        }
    }
}
