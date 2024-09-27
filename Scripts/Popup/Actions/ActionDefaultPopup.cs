using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Network;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.Actions
{
    public class ActionDefaultPopup : AbstractBasePopup
    {
        [SerializeField] private Color startColor;
        [SerializeField] private Color finalColor;
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private Button startButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private TextMeshProUGUI title;

        [Inject] private GameplayController gameplayController;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;

        private Tweener progressTweener;
        private ActionSettings settings;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is ActionSettings actionSettings)
            {
                settings = actionSettings;
            }

            title.text = $"Action: {settings.Title}";

            startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClick()).AddTo(CompositeDisposable);
            hideButton.OnClickAsObservable().Subscribe(_ => OnHideButtonClick()).AddTo(CompositeDisposable);

            progressBar.SetProgress01(0);
            startButton.interactable = true;
            InputDisabler.Clear();

            if (settings.InterruptAfterMove)
            {
                BeginObservablePositionHandle();
            }

            Subscribes();

            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            UnSubscribes();

            progressTweener?.Kill();
            progressTweener = null;

            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {

        }

        protected override void OnHiden()
        {

        }

        protected void Subscribes()
        {
            eventAggregator.Add<EndDayEvent>(OnEndDayEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<EndDayEvent>(OnEndDayEvent);
        }

        private void OnEndDayEvent(EndDayEvent sender)
        {
            Hide().Forget();
        }

        private void OnStartButtonClick()
        {
            startButton.interactable = false;

            progressTweener?.Kill();
            progressTweener = DOTween.To(() => 0f, x =>
            {
                progressBar.Icon.color = Color.Lerp(startColor, finalColor, x);
                progressBar.SetProgress01(x);
            }, 1f, settings.Duration).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    settings.Action?.Invoke();
                    InputDisabler.Disable();
                    Hide().Forget();
                });
        }

        private void OnHideButtonClick()
        {
            Hide().Forget();
        }
        
        protected void BeginObservablePositionHandle()
        {
            Observable.EveryUpdate().Where(_ =>
            {
                var view = gameplayStage.LocalGameplayData.CharacterView;
                
                if (view == null)
                {
                    return true;
                }
                
                return Vector3.Distance(view.transform.position, settings.InvokePosition) > 0.1f;
            }).Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
        }
    }
}
