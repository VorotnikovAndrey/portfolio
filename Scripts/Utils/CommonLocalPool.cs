using System;
using UnityEngine;

namespace Source
{
    public class CommonLocalPool : MonoSingleton<CommonLocalPool>
    {
        private Transform _root;

        private ExpandablePool _pool;

        protected override void Init()
        {
            base.Init();

            _root = transform;
            _pool = new ExpandablePool(_root);
        }

        public void Warm(IMonoPoolObject prefab, int count = 1)
        {
            _pool.Warm(prefab, count);
        }

        public T Spawn<T>(T prefab, Vector3 pos, Quaternion rotation) where T : IMonoPoolObject
        {
            return _pool.Spawn(prefab, pos, rotation);
        }

        public bool Release(IMonoPoolObject clone)
        {
            return _pool.Release(clone);
        }

        public void ReleaseAll()
        {
            _pool.ReleaseAll();
        }

        public void AddReleaseHandler(IMonoPoolObject obj, Action<IMonoPoolObject> action)
        {
            _pool.AddReleaseHandler(obj, action);
        }
    }
}