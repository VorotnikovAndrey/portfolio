using System;
using UniRx;
using UnityEngine;

namespace Utils.Timer
{
    public class UniversalTimer
    {
        private readonly CompositeDisposable compositeDisposable = new();
        private readonly Subject<Unit> completeSubject = new();
        private readonly Subject<Unit> restartAfterClickSubject = new();

        private IDisposable mainObservable;
        private IDisposable resetAfterClickObservable;

        private float time;
        private int cycles;
        private int cycleCounter;
        private bool restartAfterClick;

        public IObservable<Unit> OnComplete => completeSubject;
        public IObservable<Unit> OnRestartAfterClick => restartAfterClickSubject;

        /// <summary>
        /// Cycles -1 is Loop behavior
        /// </summary>
        /// <param name="time"></param>
        /// <param name="cycles"></param>
        /// <param name="restartAfterClick"></param>
        public void Run(float time, int cycles = -1, bool restartAfterClick = true)
        {
            this.time = Mathf.Clamp(time, 0, float.MaxValue);
            this.cycles = cycles;
            this.restartAfterClick = restartAfterClick;

            cycleCounter = 0;

            ResetTimer();
        }

        public void DoRun(float time, int cycles = -1, bool restartAfterClick = true)
        {
            completeSubject.OnNext(Unit.Default);
            
            Run(time, cycles, restartAfterClick);
        }

        public void Stop()
        {
            resetAfterClickObservable?.Dispose();
            mainObservable?.Dispose();
        }

        private void ResetTimer()
        {
            Stop();

            mainObservable = Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(_ =>
            {
                resetAfterClickObservable?.Dispose();
                completeSubject.OnNext(Unit.Default);

                cycleCounter++;

                if (cycleCounter < cycles || cycles == -1)
                {
                    ResetTimer();
                }
            });

            if (restartAfterClick)
            {
                resetAfterClickObservable = Observable.EveryUpdate()
                    .Where(_ => Input.GetMouseButtonDown(0))
                    .Subscribe(_ =>
                    {
                        ResetTimer();

                        restartAfterClickSubject.OnNext(Unit.Default);
                    });
            }
        }

        public void Dispose()
        {
            Stop();

            completeSubject?.Dispose();
            compositeDisposable?.Dispose();
        }
    }
}