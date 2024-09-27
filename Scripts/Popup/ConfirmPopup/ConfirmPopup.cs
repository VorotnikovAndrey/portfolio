using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    public class ConfirmPopup : AbstractBasePopup
    {
        [SerializeField] protected TextMeshProUGUI messageText;
        [SerializeField] protected Button yesButton;
        [SerializeField] protected Button noButton;

        protected ConfirmPopupData popupData;
        protected bool result;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not ConfirmPopupData confirmPopupData)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }

            popupData = confirmPopupData;
            messageText.text = confirmPopupData.Message;
            result = false;
            
            yesButton.OnClickAsObservable().Subscribe(_ => OnResult(true)).AddTo(CompositeDisposable);
            noButton.OnClickAsObservable().Subscribe(_ => OnResult(false)).AddTo(CompositeDisposable);
            
            InputDisabler.Clear();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            InputDisabler.Disable();
            popupData.Action?.Invoke(result);
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
        }

        protected override void OnHiden()
        {
        }

        protected virtual void OnResult(bool result)
        {
            this.result = result;
            
            Hide().Forget();
        }
    }
}