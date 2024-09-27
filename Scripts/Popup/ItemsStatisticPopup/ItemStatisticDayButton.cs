using System;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class ItemStatisticDayButton : PoolView
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color selectColor;

        [Inject] private GameplayStage gameplayStage;
        
        private CompositeDisposable compositeDisposable;
        private readonly Subject<ItemStatisticDayButton> emitClick = new();

        public IObservable<ItemStatisticDayButton> EmitClick => emitClick;
        public int Index { get; private set; }
        
        public void Initialize(int index, int day, TimeDayState timeDayState, bool enable)
        {
            Index = index;
            text.text = (day + 1).ToString();
            text.text += timeDayState == TimeDayState.Day ? "-D" : "-N";

            if (index <= gameplayStage.TimeOfDayChangeCounter)
            {
                button.image.color = enable ? selectColor : Color.white;
                button.interactable = true;
            }
            else
            {
                button.image.color = Color.white;
                button.interactable = false;
            }
            
            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();

            button.OnClickAsObservable().Subscribe(_ => emitClick.OnNext(this)).AddTo(compositeDisposable);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
        }
    }
}