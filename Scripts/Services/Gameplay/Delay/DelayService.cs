using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using PlayVibe;
using UniRx;
using Utils;

namespace Services.Gameplay.Delay
{
    public class DelayService
    {
        private readonly List<DelayData> data = new();
        private readonly EventAggregator eventAggregator;

        private CompositeDisposable compositeDisposable = new();
        
        private DelayService(EventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    if (data.Count == 0)
                    {
                        return;
                    }

                    var currentTime = PhotonNetwork.Time;
                    var array = data.Where(x => currentTime >= x.Time).ToList();

                    foreach (var element in array)
                    {
                        element.Action?.Invoke();
                        data.Remove(element);
                    }
                })
                .AddTo(compositeDisposable);

            Subscribes();
        }

        ~DelayService()
        {
            UnSubscribes();
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
        }

        public void Add(DelayData delayData)
        {
            Remove(delayData.Id);
            
            data.Add(delayData);
        }

        public void Remove(string id)
        {
            data.RemoveAll(x => x.Id == id);
        }

        public void Clear()
        {
            data.Clear();
        }
        
        protected void Subscribes()
        {
            eventAggregator.Add<LeaveRoomEvent>(OnLeaveRoomEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<LeaveRoomEvent>(OnLeaveRoomEvent);
        }

        private void OnLeaveRoomEvent(LeaveRoomEvent sender)
        {
            Clear();
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
        }
    }
}