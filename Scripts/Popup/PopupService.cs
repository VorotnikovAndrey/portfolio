using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace PlayVibe
{
    public sealed class PopupService
    {
        [Inject] private EventAggregator eventAggregator;
        [Inject] private SceneContextRegistry sceneContextRegistry;
        [Inject] private ObjectPoolService objectPoolService;
        
        private readonly HashSet<string> processes = new();
        
        public PopupsCanvas PopupsCanvas { get; private set; }

        public Dictionary<PopupGroup, List<AbstractBasePopup>> Popups { get; } = new()
        {
            { PopupGroup.Hud, new List<AbstractBasePopup>() },
            { PopupGroup.Gameplay, new List<AbstractBasePopup>() },
            { PopupGroup.System, new List<AbstractBasePopup>() },
            { PopupGroup.Tutorial, new List<AbstractBasePopup>() },
            { PopupGroup.Overlay, new List<AbstractBasePopup>() },
        };

        public PopupService()
        {
            LoadCanvas().Forget();
        }

        public async UniTask<AbstractBasePopup> ShowPopup(PopupOptions popupOptions, Action hideAction = null)
        {
            if (popupOptions == null || string.IsNullOrEmpty(popupOptions.PopupKey))
            {
                return null;
            }

            if (!processes.Add(popupOptions.PopupKey))
            {
                return null;
            }

            await UniTask.WaitUntil(() => PopupsCanvas != null);

            var asset = await Addressables.LoadAssetAsync<GameObject>(popupOptions.PopupKey);

            if (CheckSingle(asset))
            {
                processes.Remove(popupOptions.PopupKey);
                return null;
            }

            AbstractBasePopup popup;
            var canvas = PopupsCanvas.GetCanvas(popupOptions.PopupGroup);

            if (asset.GetComponent<AbstractBasePopup>().PopupHideType == PopupHideType.Pool)
            {
                popup = await objectPoolService.GetOrCreateView<AbstractBasePopup>(popupOptions.PopupKey,PopupsCanvas.GetCanvasTransform(popupOptions.PopupGroup));
            }
            else
            {
                var handle = Addressables.InstantiateAsync(popupOptions.PopupKey, PopupsCanvas.GetCanvasTransform(popupOptions.PopupGroup));
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    processes.Remove(popupOptions.PopupKey);
                    Debug.LogError($"Failed to instantiate object: {handle.OperationException.Message.AddColorTag(Color.yellow)}".AddColorTag(Color.red));
                    return null;
                }

                popup = handle.Result.GetComponent<AbstractBasePopup>();
            }

            GetContainer()?.InjectGameObject(popup.gameObject);
            ResetRect(popup);

            if (popupOptions.SortingOrder != null)
            {
                popup.transform.SetSiblingIndex(popupOptions.SortingOrder.Value);
            }

            AbstractBasePopup lastPopupInGroup = Popups[popupOptions.PopupGroup].LastOrDefault();
            int sortingOrder =  lastPopupInGroup != null ? lastPopupInGroup.Canvas.sortingOrder + 1 : canvas.sortingOrder + Popups[popupOptions.PopupGroup].Count + 1;
            popup.gameObject.SetActive(true);
            popup.Canvas.overrideSorting = true;
            popup.Canvas.sortingOrder = sortingOrder;

            Popups[popupOptions.PopupGroup].Add(popup);

            await popup.Show(popupOptions.Data, () =>
            {
                Popups[popupOptions.PopupGroup].Remove(popup);

                hideAction?.Invoke();

                eventAggregator.SendEvent(new PopupHiddenEvent
                {
                    Group = popupOptions.PopupGroup,
                    Popup = popup
                });
            });

            eventAggregator.SendEvent(new PopupShownEvent
            {
                Group = popupOptions.PopupGroup,
                Popup = popup
            });

            processes.Remove(popupOptions.PopupKey);

            return popup;
        }

        private bool CheckSingle(GameObject asset)
        {
            var popup = asset.GetComponent<AbstractBasePopup>();

            if (popup != null)
            {
                if (popup.MultiplicityType == PopupMultiplicityType.Single && HasPopup(popup.PopupKey))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPopup(string key)
        {
            return Popups.Any(x => x.Value.Any(popup => popup.PopupKey == key));
        }

        public bool HasPopupInGroup(string key, PopupGroup group)
        {
            return Popups[group].Any(y => y.PopupKey == key);
        }

        public bool HasPopupByType<T>() where T : AbstractBasePopup
        {
            return Popups.Any(x => x.Value.Any(popup => popup is T));
        }

        public async UniTask TryHidePopup(string key, bool force = false)
        {
            var array = new List<AbstractBasePopup>();
            
            foreach (var groups in Popups.Values)
            {
                foreach (var popup in groups)
                {
                    if (popup.PopupKey == key)
                    {
                        array.Add(popup);
                    }
                }
            }
            
            await UniTask.WhenAll(array.Select(x => TryHidePopup(x, force)));
        }

        public async UniTask TryHidePopup(AbstractBasePopup popup, bool force = false)
        {
            if (popup == null)
            {
                return;
            }

            await popup.Hide(force);
        }

        public List<T> GetPopups<T>(string key) where T : AbstractBasePopup
        {
            var result = new List<T>();

            foreach (var groups in Popups.Values)
            {
                foreach (var popup in groups)
                {
                    if (popup.PopupKey == key)
                    {
                        result.Add(popup as T);
                    }
                }
            }

            return result;
        }

        private async UniTask<bool> LoadCanvas()
        {
            var handle = Addressables.InstantiateAsync(nameof(PlayVibe.PopupsCanvas));

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var instance = handle.Result;

                GetContainer()?.InjectGameObject(instance);

                PopupsCanvas = instance.GetComponent<PopupsCanvas>();
                PopupsCanvas.Initialize();

                return true;
            }

            Debug.LogError($"Failed to instantiate object: {handle.OperationException.Message}".AddColorTag(Color.red));

            return false;
        }

        private DiContainer GetContainer()
        {
            return sceneContextRegistry.SceneContexts.FirstOrDefault()?.Container;
        }

        private void ResetRect(AbstractBasePopup popup)
        {
            var rectTransform = popup.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.rotation = Quaternion.identity;
        }

        public bool HasAnyOpenedPopup(List<PopupGroup> excludeGroupsFromCheck)
        {
            Func<KeyValuePair<PopupGroup, List<AbstractBasePopup>>, bool> CheckExcludedGroups(List<PopupGroup> excludedList)
            {
                return x =>
                {
                    if (excludedList == null && x.Value.Count > 0) return true;
                    return excludeGroupsFromCheck != null && !excludeGroupsFromCheck.Contains(x.Key) && x.Value.Count > 0;
                };
            }

            return Popups.Any(CheckExcludedGroups(excludeGroupsFromCheck));
        }

        public void HideGroup(PopupGroup group, bool force)
        {
            foreach (var popup in Popups[group].ToArray())
            {
                popup.Hide(force).Forget();
            }
        }
    }
}