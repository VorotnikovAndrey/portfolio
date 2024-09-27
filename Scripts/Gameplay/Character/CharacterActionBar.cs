using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Character
{
    public class CharacterActionBar : MonoBehaviour
    {
        [SerializeField] private GameObject parent;
        [SerializeField] private Image fill;

        private CharacterView view;
        private Tweener tweener;
        private CharacterActionData data;
        private CompositeDisposable compositeDisposable;
        
        public bool IsBusy { get; protected set; }

        public void Setup(CharacterView view)
        {
            this.view = view;
        }
        
        public void Show(CharacterActionData data)
        {
            if (IsBusy)
            {
                return;
            }
            
            this.data = data;

            if (data == null)
            {
                return;
            }
            
            parent.SetActive(true);
            
            fill.fillAmount = 0;
            
            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();
            BeginObservablePositionHandle();

            if (!string.IsNullOrEmpty(data.AnimationKey))
            {
                view.AnimationSync.Animator.SetBool(data.AnimationKey, true);
            }
            
            tweener?.Kill();
            tweener = fill.DOFillAmount(1f, data.Duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                tweener = null;
                data.Action?.Invoke();
                Hide();
            });

            IsBusy = true;
        }
        
        public void Hide()
        {
            if (data != null && !string.IsNullOrEmpty(data.AnimationKey))
            {
                view.AnimationSync.Animator.SetBool(data.AnimationKey, false);
            }
            
            compositeDisposable?.Dispose();
            tweener?.Kill();
            tweener = null;
            data = null;
            
            parent.SetActive(false);

            IsBusy = false;
        }
        
        protected void BeginObservablePositionHandle()
        {
            if (data == null)
            {
                return;
            }

            Observable.EveryUpdate().Where(_ => Vector3.Distance(view.Center.position, data.Position) > 0.01f).Subscribe(_ => Hide()).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
            tweener?.Kill();
            tweener = null;
            data = null;
            
            IsBusy = false;
        }
    }
}