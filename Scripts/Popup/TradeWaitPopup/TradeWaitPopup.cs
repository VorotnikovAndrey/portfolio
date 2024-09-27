using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.TradeWaitPopup
{
    public class TradeWaitPopup : AbstractBasePopup
    {
        [SerializeField] protected TextMeshProUGUI messageText;
        [SerializeField] private Image timeoutFill;
        [SerializeField] private TextMeshProUGUI timeoutText;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        
        protected TradeWaitPopupData tradeWaitPopupData;

        [Inject] protected GameplayStage gameplayStage;

        private Tweener fillTweener;
        private Tweener colorTweener;
        private double startTime;
        private double endTime;
        private const float timeStep = 0.2f;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not TradeWaitPopupData tradePopupData)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }
            
            gameplayStage.LocalGameplayData.CharacterView.AddBusy(Constants.Keys.Busy.WaitConfirm);

            tradeWaitPopupData = tradePopupData;
            messageText.text = tradeWaitPopupData.Message;
            
            startTime = tradePopupData.StartTime;
            endTime = tradePopupData.EndTime;
            
            fillTweener?.Kill();
            colorTweener?.Kill();
            timeoutFill.fillAmount = 1f;
            timeoutFill.color = startColor;
            
            UpdateTick();
                
            Observable.Interval(TimeSpan.FromSeconds(timeStep)).Subscribe(_ => UpdateTick()).AddTo(CompositeDisposable);
            
            InputDisabler.Clear();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            gameplayStage.LocalGameplayData.CharacterView.RemoveBusy(Constants.Keys.Busy.WaitConfirm);
            
            fillTweener?.Kill();
            colorTweener?.Kill();
            fillTweener = null;
            colorTweener = null;
            
            InputDisabler.Disable();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
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
            
            Hide().Forget();
        }
    }
}