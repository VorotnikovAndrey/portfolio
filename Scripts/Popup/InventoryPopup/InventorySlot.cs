using System;
using System.Collections.Generic;
using Gameplay;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class InventorySlot : MonoBehaviour, ISlot, IPointerClickHandler
    {
        [SerializeField] private int index;
        [SerializeField] private Image selected;
        [SerializeField] private Image border;
        [SerializeField] private RectTransform itemParent;

        private readonly Subject<ItemTransitionData> emitClick = new();
        private readonly Subject<ItemTransitionData> emitTrySnap = new();
        
        private CompositeDisposable compositeDisposable;
        private const float step = 25f;
        private List<Vector2> offsets;
        
        [Inject] private ObjectPoolService objectPoolService;
        [Inject] private GameplayController gameplayController;

        public bool IsEmpty => CurrentItem == null;
        public ItemView CurrentItem { get; private set; }

        public int Index => index;
        public int OwnerId { get; protected set; }
        public InventoryType InventoryType { get; protected set; }
        public RectTransform SlotRect => itemParent;
        public IObservable<ItemTransitionData> EmitClick => emitClick;
        public IObservable<ItemTransitionData> EmitTrySnap => emitTrySnap;
        
        private void Start()
        {
            offsets = new List<Vector2>
            {
                new(-step, -step),
                new(0, -step),
                new(step, -step),
                new(-step, 0),
                new(0, 0),
                new(step, 0),
                new(-step, step),
                new(0, step),
                new(step, step)
            };
        }

        public void Initialize(int ownerId, InventoryType inventoryType)
        {
            OwnerId = ownerId;
            InventoryType = inventoryType;
        }
        
        public bool TrySnap(ItemView item)
        {
            if (CurrentItem != null)
            {
                return false;
            }
            
            CurrentItem = item;
            CurrentItem.RectTransform.parent = itemParent;
            CurrentItem.SaveParent();

            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();
            
            CurrentItem.EmitEndDrag.Subscribe(OnDragEnded).AddTo(compositeDisposable);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent);

            return true;
        }

        public bool DropItem(DropReason reason = DropReason.Default)
        {
            if (CurrentItem == null)
            {
                return false;
            }
            
            CurrentItem.InterruptDrag();

            if (reason == DropReason.Default)
            {
                objectPoolService.ReturnToPool(CurrentItem);
            }
            
            CurrentItem = null;
            
            compositeDisposable?.Dispose();
                
            return true;
        }

        public void SetSelectState(bool value)
        {
            selected.enabled = value;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
            {
                return;
            }
            
            if (CurrentItem == null)
            {
                return;
            }
            
            emitClick.OnNext(new ItemTransitionData
            {
                ItemView = CurrentItem,
                FromSlot = this,
                ToSlot = this
            });
        }

        private void OnDragEnded(ItemDragData data)
        {
            var slots = new HashSet<ISlot>();
            var originPosition = data.PointerEventData.position;
            var rayCastResults = new List<RaycastResult>();

            foreach (var offset in offsets)
            {
                data.PointerEventData.position = originPosition + offset;
                rayCastResults.Clear();
                EventSystem.current.RaycastAll(data.PointerEventData, rayCastResults);

                foreach (var element in rayCastResults)
                {
                    var slot = element.gameObject.GetComponent<ISlot>();
                    
                    if (slot != null)
                    {
                        slots.Add(slot);
                    }
                    
                    var trashSlot = element.gameObject.GetComponent<TrashCanSlot>();

                    if (trashSlot != null && data.ItemView.InventoryType is InventoryType.Character
                            or InventoryType.LootBox 
                            or InventoryType.Secret)
                    {
                        trashSlot.PlayDropAnimation();
                        
                        objectPoolService.ReturnToPool(data.ItemView);
                        gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendDropItem(new TrashNetworkData
                        {
                            ItemModel = data.ItemView.ItemModel,
                            FromInventoryType = data.ItemView.InventoryType
                        });
                        
                        return;
                    }
                    
                    var seizedSlot = element.gameObject.GetComponent<SecuritySlot>();

                    if (seizedSlot != null)
                    {
                        emitTrySnap.OnNext(new ItemTransitionData
                        {
                            ItemView = data.ItemView,
                            FromSlot = this,
                            ToSlot = seizedSlot,
                        });
                        
                        return;
                    }
                }
            }

            ISlot nearestSlot = null;
            var nearestDistance = float.MaxValue;
            var itemPosition = data.ItemView.RectTransform.position;

            foreach (var slot in slots)
            {
                var distance = Vector2.Distance(slot.SlotRect.position, itemPosition);
                
                if (!(distance < nearestDistance))
                {
                    continue;
                }
                
                nearestDistance = distance;
                nearestSlot = slot;
            }

            if (nearestSlot != null)
            {
                emitTrySnap.OnNext(new ItemTransitionData
                {
                    ItemView = data.ItemView,
                    FromSlot = this,
                    ToSlot = nearestSlot,
                });
            }
            else
            {
                data.ItemView.ReturnPrevPosition();
            }
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
    }
}