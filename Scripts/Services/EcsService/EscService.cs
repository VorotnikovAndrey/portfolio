using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlayVibe;
using UniRx;
using UnityEngine;
using Zenject;

namespace Services
{
    public class EscService
    {
        private readonly CompositeDisposable compositeDisposable = new();
        private readonly HashSet<IEsc> data = new();

        [Inject] private StageService stageService;
        
        public EscService()
        {
            var updateStream = Observable.EveryUpdate();
            
            updateStream.Where(_ => Input.GetKeyDown(KeyCode.Escape)).Subscribe(_ => OnClick()).AddTo(compositeDisposable);
        }

        ~EscService()
        {
            compositeDisposable.Dispose();
        }

        private void OnClick()
        {
            if (data.Count > 0)
            {
                var lastItem = data.Last();

                data.Remove(lastItem);
                
                lastItem?.EscClick();
            }
        }

        public void Add(IEsc value)
        {
            if (value == null)
            {
                return;
            }
            
            data.Add(value);
        }

        public void Remove(IEsc value)
        {
            if (data.Contains(value))
            {
                data.Remove(value);
            }
        }
    }
}