using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    public class PasswordPopup : AbstractBasePopup
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button okButton;
        [SerializeField] private Button backButton;
        
        private Subject<string> passwordEntered = new Subject<string>();
        
        public IObservable<string> PasswordEntered => passwordEntered;
        
        protected override UniTask OnShow(object data = null)
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
            
            okButton.OnClickAsObservable().Subscribe(_ =>
            {
                passwordEntered.OnNext(inputField.text);
                Hide().Forget();
            }).AddTo(CompositeDisposable);

            backButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
    }
}