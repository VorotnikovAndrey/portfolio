using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.Minigames;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.Minigames
{
    public class MinigameDefaultPopup : AbstractBasePopup
    {
        [SerializeField] private Color startColor;
        [SerializeField] private Color finalColor;
        [SerializeField] private ProgressBar progressBar;
        [SerializeField] private Button startButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private TextMeshProUGUI title;

        [Inject] private MinigamesSettings minigamesSettings;
        [Inject] private GameplayController gameplayController;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;
        
        private Tweener progressTweener;
        private MinigamesSettings.MinigameSettingsData settings;
        private MinigameInteractiveObject interactiveObject;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is MinigameInteractiveObject interactive)
            {
                interactiveObject = interactive;
            }
            
            settings = minigamesSettings.GetSettings(interactiveObject.MinigameType);
            
            if (settings == null || interactiveObject == null)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }

            title.text = $"{interactiveObject.MinigameType} {settings.Difficulty} id:{interactiveObject.NetworkKey}";
            
            startButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClick()).AddTo(CompositeDisposable);
            hideButton.OnClickAsObservable().Subscribe(_ => OnHideButtonClick()).AddTo(CompositeDisposable);
            
            progressBar.SetProgress01(0);
            startButton.interactable = true;
            InputDisabler.Clear();

            BeginObservablePositionHandle();

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
            }, 1f, settings.FakeDuration).SetEase(Ease.Linear).OnComplete(
                () =>
                {
                    gameplayStage.LocalGameplayData.Quests.Remove(interactiveObject.NetworkKey);
                    eventAggregator.SendEvent(new QuestsUpdatedEvent());
                    
                    gameplayController.GetEventHandler<MinigamesNetworkEventHandler>().SendCompleteMinigame(new MinigameNetworkData
                    {
                        Owner = gameplayStage.LocalGameplayData.ActorNumber,
                        Type = interactiveObject.MinigameType,
                        InteractiveNetworkId = interactiveObject.NetworkKey
                    });
                    
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
            var view = gameplayStage.LocalGameplayData.CharacterView;
            var radius = (view as CharacterView).InteractiveRadius;

            Observable.EveryUpdate()
                .Where(_ => !Physics
                    .OverlapSphere(view.Center.position, radius, balance.Interactive.InteractiveLayer,
                        QueryTriggerInteraction.Collide).Contains(interactiveObject.InteractiveCollider))
                .Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
        }
    }
}