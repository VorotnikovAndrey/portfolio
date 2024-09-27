using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.RolePopup
{
    public class RolePopup : AbstractBasePopup
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private List<RolePopupButton> buttons;

        [Inject] private GameplayController gameplayController;
        [Inject] private GameplayStage gameplayStage;

        private CompositeDisposable timerCompositeDisposable;
        private RolePopupSettings settings;
        private double endTime;
        private RolePopupButton lastButtonPressed;
        private bool readyStatus;
        
        protected override UniTask OnShow(object data = null)
        {
            settings = data as RolePopupSettings;

            if (settings == null)
            {
                throw new Exception($"RolePopupSettings is null".AddColorTag(Color.red));
            }
            
            buttons.ForEach(button =>
            {
                button.EmitOnClick.Subscribe(OnRoleButtonClick).AddTo(CompositeDisposable);
            });

            InputDisabler.Clear();
            lastButtonPressed = null;
            readyStatus = false;
            readyButton.gameObject.SetActive(false);
            readyButton.OnClickAsObservable().Subscribe(_ => OnReadyClick()).AddTo(CompositeDisposable);
            
            eventAggregator.Add<PlayerSelectRoleEvent>(OnPlayerSelectRoleEvent);
            
            StartTimer();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            timerCompositeDisposable?.Dispose();
            eventAggregator.Remove<PlayerSelectRoleEvent>(OnPlayerSelectRoleEvent);
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }

        private void OnRoleButtonClick(RolePopupButton button)
        {
            if (readyStatus)
            {
                return;
            }
            
            lastButtonPressed = button;
            readyButton.gameObject.SetActive(true);
            
            gameplayController.GetEventHandler<RolesNetworkEventHandler>().SendSelectRole(button.RoleType, readyStatus, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        
        private void OnReadyClick()
        {
            if (lastButtonPressed == null)
            {
                return;
            }

            readyStatus = true;
            readyButton.gameObject.SetActive(false);
            
            gameplayController.GetEventHandler<RolesNetworkEventHandler>().SendSelectRole(lastButtonPressed.RoleType, readyStatus, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        
        private void OnPlayerSelectRoleEvent(PlayerSelectRoleEvent sender)
        {
            foreach (var button in buttons)
            {
                var description = string.Empty;

                foreach (var data in gameplayStage.GameplayDataDic.Values)
                {
                    if (data.RoleType != button.RoleType)
                    {
                        continue;
                    }
                    
                    var player = PhotonNetwork.CurrentRoom.Players[data.ActorNumber];
                    
                    description += $"[{(data.SelectRoleReady ? "Ready" : "Not ready")}] {player.NickName}: {player.ActorNumber}\n";
                }
            
                button.SetDescriptionText(description);
            }
        }

        private void StartTimer()
        {
            endTime = settings.Time;
            
            UpdateTimer();

            timerCompositeDisposable?.Dispose();
            timerCompositeDisposable = new CompositeDisposable();
            
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => UpdateTimer()).AddTo(timerCompositeDisposable);
        }

        private void UpdateTimer()
        {
            var remainingTime = endTime - PhotonNetwork.Time;

            timerText.text = remainingTime.ToTimeFormat();

            if (remainingTime > 0)
            {
                return;
            }

            TimeOut();
        }

        private void TimeOut()
        {
            timerCompositeDisposable?.Dispose();
            InputDisabler.Disable();
            timerText.text = string.Empty;
            gameplayController.GetEventHandler<BalanceNetworkEventHandler>().SendMasterBalanceRoles();
        }
    }
}