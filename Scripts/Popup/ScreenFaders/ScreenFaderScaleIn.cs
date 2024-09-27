using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace PlayVibe
{
    public class ScreenFaderScaleIn : ScreenFaderBase
    {
        public ScreenFaderScaleIn(ScreenFaderProfile profile) : base(profile)
        {

        }

        public override async UniTask Show(AbstractBasePopup popup, bool immediate = false)
        {
            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.Body.DOScale(profile.Scale.ShowScaleB, profile.FadeDurationOpen).SetEase(profile.EasingOpen).OnComplete(() => completionSource.TrySetResult());
                
                await completionSource.Task;
            }
            else
            {
                popup.Body.localScale = profile.Scale.ShowScaleB;
            }
        }

        public override async UniTask Hide(AbstractBasePopup popup, bool immediate = false)
        {
            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.Body.DOScale(profile.Scale.HideScaleB, profile.FadeDurationClose).SetEase(profile.EasingClose).OnComplete(() => completionSource.TrySetResult());
                
                await completionSource.Task;
            }
            else
            {
                popup.Body.localScale = Vector3.zero;
            }
        }
    }
}
