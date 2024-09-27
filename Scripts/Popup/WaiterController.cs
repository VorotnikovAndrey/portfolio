using DG.Tweening;
using UnityEngine;

namespace PlayVibe
{
    public class WaiterController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject content;
        [Space] 
        [SerializeField] private Ease showEase;
        [SerializeField] private float showDuration;
        [SerializeField] private Ease hideEase;
        [SerializeField] private float hideDuration;
        
        private Tweener tweener;
        
        public void Show(bool force = false)
        {
            content.SetActive(true);
            
            if (force)
            {
                tweener = null;
                canvasGroup.alpha = 1f;
                return;
            }
            
            tweener?.Kill();
            tweener = canvasGroup.DOFade(1f, showDuration).SetEase(showEase).OnComplete(() => tweener = null);
        }

        public void Hide(bool force = false)
        {
            tweener?.Kill();

            if (force)
            {
                tweener = null;
                canvasGroup.alpha = 0f;
                content.SetActive(false);
                return;
            }
            
            tweener = canvasGroup.DOFade(0f, hideDuration).SetEase(hideEase).OnComplete(() =>
            {
                tweener = null;
                content.SetActive(false);
            });
        }

        private void OnDisable()
        {
            tweener?.Kill();
        }
    }
}