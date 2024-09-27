using DG.Tweening;
using UnityEngine;

namespace PlayVibe
{
    public class PopupBackground : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [Space]
        [SerializeField] private Ease showEase = Ease.InSine; 
        [SerializeField] private float showDuration = 1; 
        [SerializeField] private Ease hideEase = Ease.OutSine; 
        [SerializeField] private float hideDuration = 1;

        public CanvasGroup CanvasGroup => canvasGroup;
        public Ease ShowEase => showEase;
        public Ease HideEase => hideEase;
        public float ShowDuration => showDuration;
        public float HideDuration => hideDuration;
    }
}