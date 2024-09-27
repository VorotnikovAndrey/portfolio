using System;
using Gameplay.Inventory;
using PlayVibe;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Gameplay
{
    public class ItemView : PoolView, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI networkText;
        [SerializeField] private GameObject prohibitedIcon;

        [Inject] private ItemsSettings itemsSettings;
        [Inject] private PopupService popupService;

        private readonly Subject<ItemDragData> emitEndDrag = new();
        
        private bool isDragging;
        private Vector3 beginDragPosition;
        private Vector3 dragOffset;
        private Transform prevParent;
        private Transform dragParent;

        public ItemModel ItemModel { get; private set; }
        public bool DragAllowed { get; private set; }
        public RectTransform RectTransform => rectTransform;
        public InventoryType InventoryType { get; private set; }
        public IObservable<ItemDragData> EmitEndDrag => emitEndDrag;

        public void Setup(ItemModel model, bool dragAllowed, InventoryType inventoryType)
        {
            ItemModel = model;
            DragAllowed = dragAllowed;
            InventoryType = inventoryType;

            var setting = itemsSettings.Data[model.ItemKey];

            if (setting == null)
            {
                return;
            }
            
            icon.sprite = setting.Icon;
            networkText.text = $"id: { model.NetworkId}";
            prohibitedIcon.SetActive(setting.Classification == ItemClassification.Prohibited);

            if (dragParent == null)
            {
                dragParent = popupService.PopupsCanvas.GetCanvasTransform(PopupGroup.Overlay);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            
            if (!DragAllowed)
            {
                return;
            }
            
            isDragging = true;
            
            beginDragPosition = Input.mousePosition;
            dragOffset = beginDragPosition - rectTransform.position;

            prevParent = rectTransform.parent;
            rectTransform.parent = dragParent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            rectTransform.position = Input.mousePosition - dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }
            
            isDragging = false;
            
            emitEndDrag?.OnNext(new ItemDragData
            {
                PointerEventData = eventData,
                ItemView = this
            });
        }

        public void InterruptDrag()
        {
            isDragging = false;
            ReturnPrevPosition();
        }

        public void ReturnPrevPosition()
        {
            rectTransform.parent = prevParent;
        }
        
        public void SaveParent()
        {
            prevParent = rectTransform.parent;
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            isDragging = false;
        }
    }
}