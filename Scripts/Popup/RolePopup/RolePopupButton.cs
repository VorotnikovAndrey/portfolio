using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.RolePopup
{
    public class RolePopupButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private RectTransform layout;
        [SerializeField] private RoleType roleType;
        [SerializeField] private Button button;

        private readonly CompositeDisposable compositeDisposable = new();
        private readonly Subject<RolePopupButton> emitOnClick = new();

        public IObservable<RolePopupButton> EmitOnClick => emitOnClick;
        public RoleType RoleType => roleType;

        private void Start()
        {
            titleText.text = roleType.ToString();
            descriptionText.text = string.Empty;
            
            button.OnClickAsObservable().Subscribe(_ => emitOnClick.OnNext(this)).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }

        public void SetDescriptionText(string text)
        {
            descriptionText.text = text;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
        }
    }
}