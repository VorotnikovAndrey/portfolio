using UnityEngine;

namespace Source
{
    public interface IMonoPoolObject
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}