using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Services.Gameplay.TimeDay
{
    public class TimeDaySwitcher : MonoBehaviour
    {
        [Inject] private TimeDayService timeDayService;

        private readonly CompositeDisposable compositeDisposable = new();

        public UnityEvent<string> EmitStateChanged;

        private void Start()
        {
            timeDayService.EmitStateChanged.Subscribe(state => EmitStateChanged?.Invoke(state.ToString())).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
    }
}