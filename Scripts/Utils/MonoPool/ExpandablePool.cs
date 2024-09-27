using System;
using System.Collections.Generic;
using UnityEngine;

namespace Source
{
    public class ExpandablePool
    {
        public Transform Container { get; private set; }

        // Key is the source prefab (NOT THE CLONE!), Value is the pool for its clones
        private Dictionary<IMonoPoolObject, MonoPool> _poolLookup = new Dictionary<IMonoPoolObject, MonoPool>();

        // Key is the clone (NOT PREFAB!), Value is its pool
        private Dictionary<IMonoPoolObject, MonoPool> _cloneLookup = new Dictionary<IMonoPoolObject, MonoPool>();

        // TODO: clear when objects are destroyed in pools
        private Dictionary<IMonoPoolObject, List<Action<IMonoPoolObject>>> _onReleaseEvents = new Dictionary<IMonoPoolObject, List<Action<IMonoPoolObject>>>();

        private Action<IMonoPoolObject> _overrideReleaseAction;
        
        public event Action<IMonoPoolObject> OnAfterRelease; 

        public ExpandablePool(Transform container, Action<IMonoPoolObject> overrideReleaseAction = null)
        {
            Container = container;
            _overrideReleaseAction = overrideReleaseAction;
        }

        public void Warm(IMonoPoolObject prefab, int count = 1)
        {
            var pool = !_poolLookup.ContainsKey(prefab) ? CreatePoolForPrefab(prefab) : _poolLookup[prefab];
            for (int i = 0; i < count; i++)
                pool.Warm();
        }

        public T Spawn<T>(T prefab, Vector3 pos, Quaternion rotation) where T : IMonoPoolObject
        {
            var pool = !_poolLookup.ContainsKey(prefab) ? CreatePoolForPrefab(prefab) : _poolLookup[prefab];
            var clone = (T)pool.Spawn(false);

            clone.gameObject.transform.position = pos;
            clone.gameObject.transform.rotation = rotation;

            clone.OnSpawnFromPool();

            return clone;
        }

        public T Spawn<T>(T prefab) where T : IMonoPoolObject
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity);
        }

        public bool Release(IMonoPoolObject clone)
        {
            if (!_cloneLookup.ContainsKey(clone))
            {
                return false;
            }
            
            var b = _cloneLookup[clone].Release(clone);

            OnAfterRelease?.Invoke(clone);

            return b;
        }

        public void ReleaseAll()
        {
            foreach (var pair in _cloneLookup)
                pair.Value.ReleaseAll();
        }

        public void AddReleaseHandler(IMonoPoolObject obj, Action<IMonoPoolObject> action)
        {
            if (!_onReleaseEvents.ContainsKey(obj))
                _onReleaseEvents.Add(obj, new List<Action<IMonoPoolObject>>());
            _onReleaseEvents[obj].Add(action);
        }

        public int GetSpawnedCount(IMonoPoolObject prefab)
        {
            MonoPool pool;
            return _poolLookup.TryGetValue(prefab, out pool)
                ? pool.GetSpawnedCount()
                : 0;
        }

        private IMonoPoolObject InstantiateFromPrefab(IMonoPoolObject prefab)
        {
            var go = UnityEngine.Object.Instantiate(prefab.gameObject, Container, false);
            if (Container != null)
                go.transform.SetParent(Container, false);
            return go.GetComponent<IMonoPoolObject>();
        }

        private MonoPool CreatePoolForPrefab(IMonoPoolObject prefab)
        {
            var pool = new MonoPool(Container, () => InstantiateFromPrefab(prefab), _overrideReleaseAction);
            pool.OnWarm += o =>
            {
                _cloneLookup.Add(o, pool);
            };
            pool.OnRelease += o =>
            {
                if (!_onReleaseEvents.ContainsKey(o)) 
                    return;
                foreach (var action in _onReleaseEvents[o])
                    action(o);
                _onReleaseEvents.Remove(o);
            };
            _poolLookup.Add(prefab, pool);
            return pool;
        }
    }
}
