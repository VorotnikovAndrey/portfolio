using System;
using DG.Tweening;

namespace UI
{
    public class AnimatedProgress
    {
        public event Action<float> OnValueChanged;

        protected Tweener tweener = default;
        protected float progress;

        public AnimatedProgress(float startValue = 0f)
        {
            Progress = startValue;
        }

        public virtual float Progress
        {
            get => progress;
            private set
            {
                progress = value;
                OnValueChanged?.Invoke(Progress);
            }
        }

        public virtual void ResetProgress()
        {
            Progress = 0f;
        }

        public virtual void DropTweener()
        {
            if (tweener == null) return;

            tweener.onKill = null;
            tweener?.Kill();
            tweener = null;
        }

        public virtual Tweener ApplyProgress(float value = 1f, float duration = 1f, Ease ease = Ease.Unset, Action action = null)
        {
            tweener?.Kill();
            tweener = DOTween.To(() => Progress, x => Progress = x, value, duration).SetEase(ease).OnKill(() => tweener = null);

            if (action != null)
            {
                tweener.onComplete += action.Invoke;
            }

            return tweener;
        }
    }
}
