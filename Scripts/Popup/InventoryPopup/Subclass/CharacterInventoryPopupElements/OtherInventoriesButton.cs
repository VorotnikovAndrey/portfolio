using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.Subclass.CharacterInventoryPopupElements
{
    public class OtherInventoriesButton : PoolView
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI idText;
        [SerializeField] private Color selectedColor;

        private readonly Subject<OtherInventoriesButton> emitClick = new();
        private CompositeDisposable compositeDisposable;
        
        public MapItemboxInteractiveObject InteractiveObject { get; private set; }
        public IObservable<OtherInventoriesButton> EmitClick => emitClick;

        private void OnEnable()
        {
            compositeDisposable = new CompositeDisposable();
            button.OnClickAsObservable().Subscribe(_ => OnClick()).AddTo(compositeDisposable);
        }

        private void OnDisable()
        {
            compositeDisposable?.Dispose();
            compositeDisposable = null;
        }

        public void Setup(MapItemboxInteractiveObject interactiveObject)
        {
            InteractiveObject = interactiveObject;

            idText.text = $"id: {interactiveObject.NetworkKey}";
        }

        public void SetColor(bool isSelected)
        {
            button.image.color = isSelected ? selectedColor : Color.white;
        }
        
        private void OnClick()
        {
            emitClick?.OnNext(this);
        }
    }
}