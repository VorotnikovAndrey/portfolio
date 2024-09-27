using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace PlayVibe.AnyKeyPressPopup
{
    public class AnyKeyPressPopup : AbstractBasePopup
    {
        protected override UniTask OnShow(object data = null)
        {
            if (data is not AnyKeyPressPopupData popupData)
            {
                Hide().Forget();
                return UniTask.CompletedTask;
            }
            
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    var pressedKey = GetPressedKey();

                    if (pressedKey == KeyCode.None)
                    {
                        return;
                    }
                    
                    var keyString = pressedKey.ToString();

                    if (!FilterEnglishLetters(pressedKey, keyString))
                    {
                        return;
                    }
                    
                    popupData.Action?.Invoke(pressedKey);
                    
                    Hide().Forget();
                })
                .AddTo(CompositeDisposable);
            
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
        
        private KeyCode GetPressedKey()
        {
            return Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().FirstOrDefault(Input.GetKeyDown);
        }

        private bool FilterEnglishLetters(KeyCode pressedKey, string input)
        {
            if (pressedKey == KeyCode.Tab)
            {
                return true;
            }

            if (input.Length != 1)
            {
                return false;
            }

            var c = input[0];
            
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || char.IsDigit(c);
        }
    }
}