using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Services;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class AbstractBasePopup : PoolView, IEsc
    {
        [SerializeField] private string popupKey;
        [SerializeField] private PopupHideType hideType;
        [SerializeField] private PopupMultiplicityType multiplicityType;
        [SerializeField] private bool closeWhenPressingESC;
        [SerializeField] private RectTransform body;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [Space]
        [SerializeField] private bool useBackground;
        [ShowIf("useBackground")]
        [SerializeField] private PopupBackground background;
        [Space]
        [SerializeField] private List<ScreenFaderProfile> screenFaderProfiles;
   
        private List<ScreenFaderBase> screenFaders;
        private Tweener backgroundTweener;
        private Action hideAction;
        private bool hideCaused;
        
        [Inject] private ScreenFaderFactory screenFaderFactory;
        [Inject] protected DiContainer diContainer;
        [Inject] protected EscService escService;

        public string PopupKey => popupKey;
        public PopupHideType PopupHideType => hideType;
        public PopupMultiplicityType MultiplicityType => multiplicityType;
        public RectTransform Body => body;
        public Canvas Canvas => canvas;
        public CanvasGroup CanvasGroup => canvasGroup;
        public PopupState State { get; private set; } = PopupState.None;
        public DisableHandler<GraphicRaycaster>  InputDisabler { get; private set; }
        protected CompositeDisposable CompositeDisposable { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            
            gameObject.SetActive(false);
            InputDisabler = new DisableHandler<GraphicRaycaster>(graphicRaycaster);
        }

        private void OnDestroy()
        {
            backgroundTweener?.Kill();

            if (CompositeDisposable != null && !CompositeDisposable.IsDisposed)
            {
                CompositeDisposable.Dispose();
                CompositeDisposable = null;
            }
        }

        protected abstract UniTask OnShow(object data = null);
        protected abstract UniTask OnHide();
        protected abstract void OnShowen();
        protected abstract void OnHiden();

        /// <summary>
        /// Execute only from PopupService.cs
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hideAction"></param>
        public async UniTask Show(object data = null, Action hideAction = null)
        {
            if (State is PopupState.Show or PopupState.Shown)
            {
                return;
            }

            hideCaused = false;

            State = PopupState.Show;

            this.hideAction = hideAction;
            
            CompositeDisposable?.Clear();
            CompositeDisposable = new CompositeDisposable();
            
            eventAggregator.SendEvent(new PopupShowEvent
            {
                Popup = this
            });

            await OnShow(data);

            screenFaders ??= screenFaderProfiles.Select(profile => screenFaderFactory.GetFader(profile)).ToList();

            if (useBackground && background != null && background.CanvasGroup != null)
            {
                background.CanvasGroup.alpha = 0;
                backgroundTweener?.Kill();
                backgroundTweener = background.CanvasGroup.DOFade(1, background.ShowDuration).SetEase(background.ShowEase).OnComplete(() => backgroundTweener = null);
            }
            
            if (screenFaders != null && screenFaders.Count > 0)
            {
                await UniTask.WhenAll(screenFaders.Select(fader => fader.Hide(this, true)));
                await UniTask.WhenAll(screenFaders.Select(fader => fader.Show(this)));
            }

            if (!hideCaused)
            {
                State = PopupState.Shown;

                OnShowen();
                
                if (closeWhenPressingESC)
                {
                    escService.Add(this);
                }
            }
        }

        public async UniTask Hide(bool force = false)
        {
            if (State is PopupState.Hide or PopupState.Hidden)
            {
                return;
            }
            
            State = PopupState.Hide;

            hideCaused = true;
            
            if (closeWhenPressingESC)
            {
                escService.Remove(this);
            }

            await OnHide();

            if (!force)
            {
                if (useBackground && background != null && background.CanvasGroup != null)
                {
                    backgroundTweener?.Kill();
                    backgroundTweener = background.CanvasGroup.DOFade(0, background.HideDuration).SetEase(background.HideEase).OnComplete(() => backgroundTweener = null);
                }

                if (screenFaders != null && screenFaders.Count > 0)
                {
                    await UniTask.WhenAll(screenFaders.Select(fader => fader.Hide(this)));
                }
            }
            else
            {
                backgroundTweener?.Kill();
            }
            
            OnHiden();
            
            CompositeDisposable?.Dispose();

            if (hideType == PopupHideType.Release)
            {
                Addressables.ReleaseInstance(gameObject);
            }
            else
            {
                objectPoolService.ReturnToPool(this);
            }
            
            State = PopupState.Hidden;
            
            hideAction?.Invoke();

            eventAggregator.SendEvent(new PopupHideEvent
            {
                Popup = this
            });
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            InputDisabler.Clear();
            
            backgroundTweener?.Kill();
            
            if (CompositeDisposable != null && !CompositeDisposable.IsDisposed)
            {
                CompositeDisposable.Dispose();
                CompositeDisposable = null;
            }
        }
        
        protected void ResetAllTriggers(Animator animator)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(parameter.name);
                }
            }
        }

        protected override void OnSceneChanged()
        {
            switch (sceneChangeBehavior)
            {
                case PopupSceneChangeBehavior.ReturnToPool:
                    Hide(true).Forget();
                    break;
                case PopupSceneChangeBehavior.Release:
                    hideType = PopupHideType.Release;
                    Hide(true).Forget();
                    objectPoolService.ReleaseView(this);
                    break;
            }
        }

        public void EscClick()
        {
            if (!closeWhenPressingESC)
            {
                return;
            }
            
            Hide().Forget();
        }
    }
}