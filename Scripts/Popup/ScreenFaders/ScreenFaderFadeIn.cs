using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;

namespace PlayVibe
{
    public class ScreenFaderFadeIn : ScreenFaderBase
    {
        public ScreenFaderFadeIn(ScreenFaderProfile profile) : base(profile)
        {
            
        }

        public override async UniTask Show(AbstractBasePopup popup, bool immediate = false)
        {
            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.CanvasGroup.DOFade(1f, profile.FadeDurationOpen).SetEase(profile.EasingOpen).OnComplete(() => completionSource.TrySetResult());

                await completionSource.Task;
            }
            else
            {
                popup.CanvasGroup.alpha = 1f;
            }
        }

        public override async UniTask Hide(AbstractBasePopup popup, bool immediate = false)
        {
            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.CanvasGroup.DOFade(0f, profile.FadeDurationClose).SetEase(profile.EasingClose).OnComplete(() => completionSource.TrySetResult());
                
                await completionSource.Task;
            }
            else
            {
                popup.CanvasGroup.alpha = 0f;
            }
        }
    }
}