using System;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using Utils;
using Zenject;

namespace PlayVibe
{
    public class ArrestPopup : AbstractBasePopup
    {
        [SerializeField] private TextMeshProUGUI timeText;
        
        private const float timeStep = 0.25f;
        
        private double endTime;
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not ArrestPopupNetworkData arrestPopupNetworkData)
            {
                Hide(true).Forget();
                
                return UniTask.CompletedTask;
            }

            endTime = arrestPopupNetworkData.EndTime;

            UpdateTick();
            
            Observable.Interval(TimeSpan.FromSeconds(timeStep)).Subscribe(_ => UpdateTick()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
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
            var remainingTime = endTime - PhotonNetwork.Time;

            if (remainingTime <= 0)
            {
                Hide(true).Forget();
                
                return;
            }

            timeText.text = remainingTime.ToTimeFormat();
        }
    }
}