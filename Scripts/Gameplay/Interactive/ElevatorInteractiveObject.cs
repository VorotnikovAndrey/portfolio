using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Character;
using Photon.Pun;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.Elements
{
    public class ElevatorInteractiveObject : NeedItemInteractiveObject
    {
        [SerializeField] private float interactiveDuration;
        [SerializeField] private float callTime = 30;
        [SerializeField] private float activeTime = 10;
        [SerializeField] private GameObject canvas;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image progressImage;

        private Tweener timeTweener;
        private CompositeDisposable compositeDisposable;
        private double startPhotonTime;
        private double endPhotonTime;
        private CancellationTokenSource cancellationTokenSource;
        
        public bool Activated { get; private set; }

        private void Start()
        {
            canvas.SetActive(false);
        }

        public override void TryInteractive(CharacterView view)
        {
            if (Activated)
            {
                FailedInteractive(view);
                return;
            }
            
            view.ActionBar.Show(new CharacterActionData
            {
                Position = view.Center.position,
                Duration = interactiveDuration,
                Action = () =>
                {
                    if (view == null)
                    {
                        return;
                    }
                    
                    if (Activated)
                    {
                        return;
                    }
                    base.TryInteractive(view);
                }
            });
        }
        
        protected override void SuccessfulInteractive(CharacterView view)
        {
            base.SuccessfulInteractive(view);

            ActivateElevator();
        }
        
        public void ActivateElevator()
        {
            if (Activated)
            {
                return;
            }
            
            var startTime = PhotonNetwork.Time;
            var endTime = PhotonNetwork.Time + callTime;
            
            photonView.RPC("ActivateElevatorRPC", RpcTarget.All, startTime, endTime);
        }
        
        [PunRPC]
        public void ActivateElevatorRPC(double start, double end)
        {
            if (Activated)
            {
                return;
            }
            
            Activated = true;
            canvas.SetActive(true);
            progressImage.fillAmount = 0;
            startPhotonTime = start;
            endPhotonTime = end;

            UpdateText();

            compositeDisposable = new CompositeDisposable();
            
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => UpdateTick()).AddTo(compositeDisposable);
        }

        private float GetProgress()
        {
            var currentTime = PhotonNetwork.Time;
            var totalDuration = endPhotonTime - startPhotonTime;
            var elapsed = currentTime - startPhotonTime;
            var progress = (float)elapsed / (float)totalDuration;

            return Mathf.Clamp01(progress);
        }

        private void UpdateTick()
        {
            var progress = GetProgress();

            UpdateText();
            
            timeTweener?.Kill();
            timeTweener = progressImage
                .DOFillAmount(progress, 1f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    timeTweener = null;
                });

            if (progress < 1f)
            {
                return;
            }
            
            Restart().Forget();
        }

        private async UniTask Restart()
        {
            ShowInfoPopup($"The elevator has been launched!");
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
            
            gameObject.SetActive(false);

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            
            await UniTask.Delay(TimeSpan.FromSeconds(activeTime), cancellationToken: cancellationTokenSource.Token);

            Activated = false;
            canvas.SetActive(false);
            gameObject.SetActive(true);
            
            cancellationTokenSource = null;
        }

        private void UpdateText()
        {
            var currentTime = PhotonNetwork.Time;
            var totalSeconds = (int)(endPhotonTime - currentTime);
            var minutes = Mathf.FloorToInt(totalSeconds / 60F);
            var seconds = Mathf.FloorToInt(totalSeconds % 60F);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            compositeDisposable?.Dispose();
            compositeDisposable = null;
            
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }
}