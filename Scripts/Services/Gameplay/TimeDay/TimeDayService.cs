using System;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using PlayVibe;
using UniRx;
using UnityEngine;
using Zenject;

namespace Services.Gameplay.TimeDay
{
    public class TimeDayService
    {
        [Inject] private PopupService popupService;
        [Inject] private EventAggregator eventAggregator;
        
        private readonly Subject<(float, float)> emitProgress = new();
        private readonly Subject<TimeDayState> emitStateChanged = new();

        private float totalTime;
        private float time;
        private CompositeDisposable compositeDisposable;
        
        public IObservable<(float, float)> EmitProgress => emitProgress;
        public IObservable<TimeDayState> EmitStateChanged => emitStateChanged;
        public float CurrentProgress { get; private set; }
        public TimeDayState CurrentState { get; private set; } = TimeDayState.Night;
        
        public void Run(double endTime, TimeDayState state)
        {
            CurrentState = state;
            emitStateChanged?.OnNext(CurrentState);
            
            time = (float)(endTime - PhotonNetwork.Time);
            totalTime = time;
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.TimeDayPopup, null, PopupGroup.Hud)).Forget();
            
            CurrentProgress = Mathf.Clamp01((totalTime - time) / totalTime);
            emitProgress.OnNext((CurrentProgress, (int)time));
            
            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();
            
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => UpdateTick()).AddTo(compositeDisposable);
        }

        private void UpdateTick()
        {
            if (time <= 0)
            {
                Stop();
            }
            
            time--;

            CurrentProgress = Mathf.Clamp01((totalTime - time) / totalTime);
            emitProgress.OnNext((CurrentProgress, (int)time));
        }

        public void Stop()
        {
            compositeDisposable?.Dispose();

            eventAggregator.SendEvent(new EndDayEvent
            {
                State = CurrentState
            });
        }
    }
}