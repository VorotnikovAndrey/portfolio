using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Source
{
    public class MonoPool
    {
        private Transform _container;
        private Func<IMonoPoolObject> _warmFunc;

        public event Action<IMonoPoolObject> OnWarm;
        public event Action<IMonoPoolObject> OnRelease;

        private List<IMonoPoolObject> _spawnedClones = new List<IMonoPoolObject>();
        private List<IMonoPoolObject> _pooledClones = new List<IMonoPoolObject>();
        private Action<IMonoPoolObject> _overrideReleaseAction;

        public MonoPool(Transform container, Func<IMonoPoolObject> warmFunc, Action<IMonoPoolObject> overrideReleaseAction)
        {
            _container = container;
            _warmFunc = warmFunc;
            _overrideReleaseAction = overrideReleaseAction;
        }

        public MonoPool(Transform container, IMonoPoolObject prefab)
        {
            _container = container;
            _warmFunc = () => UnityEngine.Object.Instantiate(prefab as MonoBehaviour, container) as IMonoPoolObject;
        }

        public void Warm()
        {
            var clone = _warmFunc();
            _pooledClones.Add(clone);
            OnWarm?.Invoke(clone);
            clone.gameObject.SetActive(false);
        }

        public IMonoPoolObject Spawn(bool callOnSpawn)
        {
            if (_pooledClones.Count == 0)
                Warm();

            var cloneToSpawn = _pooledClones[0];
            _pooledClones.RemoveAt(0);
            _spawnedClones.Add(cloneToSpawn);

            cloneToSpawn.gameObject.SetActive(true);
            if (callOnSpawn)
                cloneToSpawn.OnSpawnFromPool();

            return cloneToSpawn;
        }

        public bool Release(IMonoPoolObject clone)
        {
            if (_pooledClones.Contains(clone))
            {
                //Debug.LogWarning($"Clone already in pool: {clone.gameObject.name}");
                return false;
            }
            _spawnedClones.Remove(clone);
            _pooledClones.Add(clone);

            clone.OnReturnToPool();

            if (_overrideReleaseAction != null)
                _overrideReleaseAction?.Invoke(clone);
            else
                clone.gameObject.SetActive(false);

            DOTween.Kill(clone);
            DOTween.Kill(clone.gameObject.transform);

            if (_container != null)
                clone.gameObject.transform.SetParent(_container, false);

            OnRelease?.Invoke(clone);

            return true;
        }

        public void ReleaseAll()
        {
            while (_spawnedClones.Count > 0)
                Release(_spawnedClones.First());
        }

        public int GetSpawnedCount()
        {
            return _spawnedClones.Count;
        }
    }
}
