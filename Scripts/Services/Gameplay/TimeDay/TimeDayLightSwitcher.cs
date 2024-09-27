using DG.Tweening;
using PlayVibe;
using UniRx;
using UnityEngine;
using Zenject;

namespace Services.Gameplay.TimeDay
{
    public class TimeDayLightSwitcher : MonoBehaviour
    {
        [SerializeField] private Light globalLight;
        [Space] 
        [SerializeField] private Color dayColor;
        [SerializeField] private Color nightColor;
        
        [Inject] private TimeDayService timeDayService;
        [Inject] private Balance balance;

        private Tweener tweener;

        private readonly CompositeDisposable compositeDisposable = new();

        private void Start()
        {
            globalLight.color = dayColor;
            
            timeDayService.EmitStateChanged.Subscribe(state =>
            {
                tweener?.Kill();
                tweener = globalLight
                    .DOColor(state == TimeDayState.Day ? dayColor : nightColor, balance.TimeDay.SwitchDuration)
                    .SetEase(balance.TimeDay.SwitchEase).OnComplete(() => tweener = null);
            }).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
    }
}