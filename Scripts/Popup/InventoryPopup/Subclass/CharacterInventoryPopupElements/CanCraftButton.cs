using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.Subclass.CharacterInventoryPopupElements
{
    public class CanCraftButton : PoolView
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;

        [Inject] private ItemsSettings itemsSettings;
        [Inject] private PopupService popupService;

        private readonly CompositeDisposable compositeDisposable = new();
        
        public string Key { get; private set; }

        private void Start()
        {
            button.OnClickAsObservable().Subscribe(_ =>
            {
                var popup = popupService.GetPopups<SelfCraftPopup>(Constants.Popups.SelfCraftPopup).FirstOrDefault();

                if (popup == null)
                {
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.SelfCraftPopup, new SelfCraftPopupData
                    {
                        ItemKey = Key
                    })).Forget();
                }
                else
                {
                    popup.SelectContainer(Key);
                }
            }).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }

        public void Setup(string itemKey)
        {
            Key = itemKey;

            icon.sprite = itemsSettings.Data[Key].Icon;
        }
    }
}