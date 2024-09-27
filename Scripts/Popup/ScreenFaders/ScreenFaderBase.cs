using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;

namespace PlayVibe
{
    [Serializable]
    public class ScreenFaderBase
    {
        protected ScreenFaderProfile profile;
        protected Tweener tweener;
        protected UniTaskCompletionSource completionSource;

        protected ScreenFaderBase(ScreenFaderProfile profile)
        {
            this.profile = profile;
        }

        public virtual UniTask Show(AbstractBasePopup popup, bool immediate = false)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask Hide(AbstractBasePopup popup, bool immediate = false)
        {
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            tweener?.Kill();
            completionSource?.TrySetResult();
        }
    }
}
