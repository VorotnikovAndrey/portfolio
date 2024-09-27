using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace PlayVibe
{
    public class ScreenFaderMoveIn : ScreenFaderBase
    {
        public ScreenFaderMoveIn(ScreenFaderProfile profile) : base(profile)
        {

        }

        protected enum OpenOrClose
        {
            Open = 0,
            Close = 1
        };
        
        public override async UniTask Show(AbstractBasePopup popup, bool immediate = false)
        {
            popup.Body.localPosition = DetermineInitialPosition(OpenOrClose.Open);

            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.Body.DOLocalMove(Vector3.zero, profile.FadeDurationOpen).SetEase(profile.EasingOpen).OnComplete(() => completionSource.TrySetResult());
                
                await completionSource.Task;
            }
            else
            {
                popup.Body.localPosition = Vector3.zero;
            }
        }

        public override async UniTask Hide(AbstractBasePopup popup, bool immediate = false)
        {
            tweener?.Kill();
            completionSource?.TrySetResult();

            if (!immediate)
            {
                completionSource = new UniTaskCompletionSource();
                
                tweener = popup.Body.DOLocalMove(DetermineInitialPosition(OpenOrClose.Close), profile.FadeDurationClose).SetEase(profile.EasingClose).OnComplete(() => completionSource.TrySetResult());
                
                await completionSource.Task;
            }
            else
            {
                popup.Body.localPosition = DetermineInitialPosition(OpenOrClose.Close);
            }
        }

        private Vector2 DetermineInitialPosition(OpenOrClose state)
        {
            switch (state == OpenOrClose.Open ? profile.Move.OpenDirection : profile.Move.CloseDirection)
            {
                case MoveInSide.Top:
                    return new Vector2(0f, Screen.height * 2);
                case MoveInSide.Down:
                    return new Vector2(0f, -(Screen.height * 2));
                case MoveInSide.Left:
                    return new Vector2(-(Screen.width * 2), 0f);
                case MoveInSide.Right:
                    return new Vector2(Screen.width * 2, 0f);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
