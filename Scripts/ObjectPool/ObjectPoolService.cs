using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Zenject;

namespace PlayVibe
{
    public class ObjectPoolEntry
    {
        private readonly PoolView prefab;
        private readonly List<PoolView> objectList = new();

        public ObjectPoolEntry(PoolView prefab)
        {
            this.prefab = prefab;
        }
        
        public async UniTask<PoolView> GetNextObject(Transform parent)
        {
            if (objectList.Count == 0)
            {
                return await InstantiateItem(parent);
            }

            var item = objectList[0];
            objectList.Remove(item);

            if (item != null)
            {
                item.transform.SetParent(parent, false);
            }

            return item;
        }

        public void ReturnToPool(PoolView view)
        {
            if (objectList.Contains(view))
            {
                return;
            }
            
            objectList.Add(view);

            view.gameObject.SetActive(false);
            view.transform.SetParent(ObjectPoolService.GlobalParent);
            view.OnReturnToPool();
        }

        public void ReleaseView(PoolView view)
        {
            if (objectList.Contains(view))
            {
                objectList.Remove(view);
            }
            
            view.gameObject.SetActive(false);
            Addressables.ReleaseInstance(view.gameObject);
        }

        public void ReleaseAll()
        {
            foreach (var element in objectList)
            {
                ReleaseView(element);
            }
            
            objectList.Clear();
        }

        public async UniTask<PoolView> InstantiateItem(Transform parent, bool toPool = false)
        {
            if (prefab == null)
            {
                return null;
            }

            var handle = Addressables.InstantiateAsync(prefab.gameObject.name, new Vector3(9999f, 9999f, 9999f), Quaternion.identity, parent);
            await handle.Task;
            
            if (handle.Result == null)
            {
                Debug.LogError("Failed to instantiate object from Addressables.");
                return null;
            }
           
            var view = handle.Result.GetComponent<PoolView>();
            view.gameObject.SetActive(false);
            view.transform.localPosition = Vector3.zero;
            view.PoolId = prefab.gameObject.name;
            
            if (toPool)
            {
                objectList.Add(view);
            }

            return view;
        }

        public List<PoolView> SelectCategory(PoolCategory category)
        {
            return objectList.Where(x => x.PoolCategory == category).ToList();
        }
    }

    public sealed class ObjectPoolService
    {
        private readonly Dictionary<string, ObjectPoolEntry> objectPoolDictionary = new();
        
        public static Transform GlobalParent { get; private set; }

        public ObjectPoolService()
        {
            GlobalParent = new GameObject("ObjectPoolHandler").transform;
            var color = GlobalParent.gameObject.AddComponent<DefaultHColor>();
            color.HColorData.TextColor = ColorUtility.HexToColor("#eb4034");
            Object.DontDestroyOnLoad(GlobalParent);
        }

        public async UniTask<T> GetOrCreateView<T>(AssetReference assetReference, Transform parent = null, bool activeSelf = false) where T : PoolView
        {
            var prefab = await Addressables.LoadAssetAsync<GameObject>(assetReference);
            return await GetOrCreateView<T>(prefab.gameObject.name, parent, activeSelf);
        }
        
        public async UniTask<T> GetOrCreateView<T>(string assetName, Transform parent = null, bool activeSelf = false, bool resetPosAndRotation = true) where T : PoolView
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }
            
            ObjectPoolEntry entry;
            
            var prefab = await Addressables.LoadAssetAsync<GameObject>(assetName);
            var prefabName = prefab.gameObject.name;

            if (!objectPoolDictionary.TryGetValue(prefabName, out var value))
            {
                entry = new ObjectPoolEntry(prefab.GetComponent<PoolView>());
                objectPoolDictionary[prefabName] = entry;
            }
            else
            {
                entry = value;
            }

            var item = await entry.GetNextObject(parent ? parent : GlobalParent);

            if (item == null)
            {
                return null;
            }

            var component = item.GetComponent<T>();
            component.gameObject.SetActive(activeSelf);

            if (!resetPosAndRotation)
            {
                return component != null ? component : null;
            }
            
            var transformComponent = component.transform;

            transformComponent.localScale = Vector3.one;
            transformComponent.localPosition = Vector3.zero;

            return component != null ? component : null;
        }
        
        public void ReturnToPool<T>(T view) where T : PoolView
        {
            if (view == null)
            {
                return;
            }
            
            ReturnToPoolInternal(view);
        }

        private void ReturnToPoolInternal<T>(T view) where T : PoolView
        {
            if (objectPoolDictionary.TryGetValue(view.PoolId, out var value))
            {
                value.ReturnToPool(view);
            }
        }

        public void ReleaseView<T>(T view) where T : PoolView
        {
            if (view == null)
            {
                return;
            }
            
            if (objectPoolDictionary.TryGetValue(view.PoolId, out var value))
            {
                value.ReleaseView(view);
            }
        }
        
        public void ReleaseAll()
        {
            foreach (var entry in objectPoolDictionary)
            {
                entry.Value.ReleaseAll();
            }
            
            objectPoolDictionary.Clear();
        }
        
        public void ReleaseCategory(PoolCategory category)
        {
            foreach (var entry in objectPoolDictionary)
            {
                foreach (var view in entry.Value.SelectCategory(category))
                {
                    ReleaseView(view);
                }
            }
        }
    }
}
