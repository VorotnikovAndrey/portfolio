using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe
{
    public class TradeConfirmPopup : ConfirmPopup
    {
        [SerializeField] private Image timeoutFill;
        [SerializeField] private TextMeshProUGUI timeoutText;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        
        protected TradeConfirmPopupData tradeConfirmPopupData;

        [Inject] protected GameplayStage gameplayStage;
        [Inject] protected Balance balance;

        private Tweener fillTweener;
        private Tweener colorTweener;
        private double startTime;
        private double endTime;
        private bool isTimeout;
        private const float timeStep = 0.2f;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not TradeConfirmPopupData tradePopupData)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }
            
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.InTradeConfirm);

            tradeConfirmPopupData = tradePopupData;
            messageText.text = tradeConfirmPopupData.Message;
            result = false;
            isTimeout = false;
            
            yesButton.OnClickAsObservable().Subscribe(_ => OnResult(true)).AddTo(CompositeDisposable);
            noButton.OnClickAsObservable().Subscribe(_ => OnResult(false)).AddTo(CompositeDisposable);
            
            InputDisabler.Clear();

            BeginObservablePositionHandle();
            
            startTime = tradePopupData.StartTime;
            endTime = tradePopupData.EndTime;

            UpdateTick();
                
            Observable.Interval(TimeSpan.FromSeconds(timeStep)).Subscribe(_ => UpdateTick()).AddTo(CompositeDisposable);
            
            InputDisabler.Clear();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            fillTweener?.Kill();
            colorTweener?.Kill();
            fillTweener = null;
            colorTweener = null;
            
            InputDisabler.Disable();
            
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.InTradeConfirm);

            if (!isTimeout)
            {
                tradeConfirmPopupData.Action?.Invoke(result);
                tradeConfirmPopupData = null;
            }
            
            return UniTask.CompletedTask;
        }
        
        protected void BeginObservablePositionHandle()
        {
            if (tradeConfirmPopupData == null)
            {
                Hide().Forget();
                return;
            }
            
            var view = gameplayStage.LocalGameplayData?.CharacterView;
            var target = gameplayStage.GameplayDataDic[tradeConfirmPopupData.ActorNumber]?.CharacterView;

            if (view == null || target == null)
            {
                Hide().Forget();
                return;
            }

            Observable.EveryUpdate()
                .Where(_ =>
                {
                    if (view == null || target == null)
                    {
                        return true;
                    }
                    
                    return Vector3.Distance(view.Center.position, target.Center.position) > balance.Interactive.MaxTradeDistance;
                }).Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
        }
        
        private void UpdateTick()
        {
            var totalTime = endTime - startTime;
            var remainingTime = endTime - PhotonNetwork.Time;

            timeoutText.text = Math.Ceiling(Math.Max(remainingTime, 0)).ToString(CultureInfo.InvariantCulture);

            var value = Math.Max((float)(remainingTime / totalTime), 0);
            
            fillTweener?.Kill();
            colorTweener?.Kill();
            fillTweener = timeoutFill.DOFillAmount(value, timeStep).SetEase(Ease.Linear).OnComplete(() => fillTweener = null);
            colorTweener = timeoutFill.DOColor(Color.Lerp(endColor, startColor, value), timeStep).SetEase(Ease.Linear).OnComplete(() => colorTweener = null);

            if (value > 0)
            {
                return;
            }
            
            isTimeout = true;
            Hide().Forget();
        }
    }
}