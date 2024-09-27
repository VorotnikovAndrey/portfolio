using System;
using DG.Tweening;
using Photon.Pun;
using PlayVibe;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Gameplay.Player.Effects
{
    public class EffectContainer : PoolView
    {
        private const float UpdateSpeed = 0.2f;
        
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image icon;

        private Tweener fillTweener;
        private CompositeDisposable compositeDisposable;
        private EffectData effectData;
        private double endTime;

        public EffectModel EffectModel { get; private set; }

        public void Setup(EffectData data, EffectModel model)
        {
            EffectModel = model;
            effectData = data;
            
            icon.sprite = data.Icon;
            fillImage.color = data.BackgroundColor;
            endTime = EffectModel.EndTime;
            
            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();

            UpdateTick(true);
            
            Observable.Interval(TimeSpan.FromSeconds(UpdateSpeed)).Subscribe(_ => UpdateTick()).AddTo(compositeDisposable);
        }

        private void UpdateTick(bool force = false)
        {
            if (EffectModel == null)
            {
                Debug.LogError("Model is null".AddColorTag(Color.red));
                return;
            }

            var totalSeconds = endTime - PhotonNetwork.Time;
            var displaySeconds = Mathf.Clamp((int)Mathf.Floor((float)totalSeconds), 0, int.MaxValue);
            var progress = Mathf.Clamp01((float)totalSeconds / effectData.Duration);
            
            timerText.text = displaySeconds.ToString();

            fillTweener?.Kill();

            if (force)
            {
                fillImage.fillAmount = progress;
                return;
            }
            
            fillTweener = fillImage.DOFillAmount(progress, UpdateSpeed).SetEase(Ease.Linear).OnComplete(() => fillTweener = null);
        }
        
        public void OverrideTime(double value)
        {
            endTime = value;
            UpdateTick(true);
        }
        
        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
            fillTweener?.Kill();
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
            compositeDisposable = null;
            fillTweener?.Kill();
        }
    }
}