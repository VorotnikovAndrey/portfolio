using System;
using Cysharp.Threading.Tasks;
using PlayVibe.AnyKeyPressPopup;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class ControlContainer : MonoBehaviour
    {
        [SerializeField] private ControlType type;
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI keyText;
        [Space]
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color duplicateColor;

        private readonly Subject<ControlContainer> emitValueChanged = new();
        private readonly CompositeDisposable compositeDisposable = new();

        [Inject] private PopupService popupService;

        public ControlType ControlType => type;
        public KeyCode KeyCode { get; private set; }
        public IObservable<ControlContainer> EmitValueChanged => emitValueChanged;

        public void Initialize(KeyCode value)
        {
            KeyCode = value;
            keyText.text = value.ToString();
 
            button.OnClickAsObservable().Subscribe(_ =>
            {
                popupService.ShowPopup(new PopupOptions(Constants.Popups.AnyKeyPressPopup, new AnyKeyPressPopupData
                {
                    Action = keyCode =>
                    {
                        if (KeyCode == keyCode)
                        {
                            return;
                        }
                    
                        KeyCode = keyCode;
                        keyText.text = KeyCode.ToString();
                        emitValueChanged.OnNext(this);
                    }
                })).Forget();
            }).AddTo(compositeDisposable);
        }

        public void SetDuplicateState(bool hasDuplicate)
        {
            background.color = hasDuplicate ? duplicateColor : defaultColor;
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
    }
}