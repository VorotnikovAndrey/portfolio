using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Events;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.TimeDayPopup
{
    public class TimeDayPopup : AbstractBasePopup
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;

        [Inject] private TimeDayService timeDayService;
        [Inject] private GameplayStage gameplayStage;

        private Tweener timeTweener;
        private float lastProgress;
        
        protected override UniTask OnShow(object data = null)
        {
            fillImage.fillAmount = 0f;
            
            UpdateDay();
            
            timeDayService.EmitProgress.Subscribe(x => SetTime(x.Item1, x.Item2)).AddTo(CompositeDisposable);
            
            eventAggregator.Add<TimeOfDayChangeCounterUpdatedEvent>(OnTimeOfDayChangeCounterUpdatedEvent);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            eventAggregator.Remove<TimeOfDayChangeCounterUpdatedEvent>(OnTimeOfDayChangeCounterUpdatedEvent);
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            timeTweener?.Kill();
            timeTweener = null;
        }
        
        private void SetTime(float progress, float time)
        {
            if (lastProgress > progress)
            {
                fillImage.fillAmount = progress;
            }
            
            lastProgress = progress;
            
            timeTweener?.Kill();
            timeTweener = fillImage.DOFillAmount(progress, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                timeTweener = null;
            });
            
            timeText.text = (time + 1).ToString(CultureInfo.InvariantCulture);
        }
        
        private void OnTimeOfDayChangeCounterUpdatedEvent(TimeOfDayChangeCounterUpdatedEvent sender)
        {
            UpdateDay();
        }

        private void UpdateDay()
        {
            dayText.text = $"{timeDayService.CurrentState.ToString()} {gameplayStage.CurrentDay}";
        }
    }
}