using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe
{
    public class RoomContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI occupancyText;
        [SerializeField] private TextMeshProUGUI ownerText;
        [SerializeField] private TextMeshProUGUI regionText;
        [SerializeField] private TextMeshProUGUI pingText;
        [SerializeField] private Button joinButton;
        [Space] 
        [SerializeField] private UnityEvent<string> EmitStatus;

        [Inject] private PopupService popupService;
        [Inject] private StartupService startupService;

        private CompositeDisposable compositeDisposable;

        public RoomInfo RoomInfo { get; private set; }
        
        private void OnEnable()
        {
            compositeDisposable = new CompositeDisposable();
            joinButton.OnClickAsObservable().Subscribe(_ => OnJoinButtonClick()).AddTo(compositeDisposable);
        }

        private void OnDisable()
        {
            compositeDisposable?.Dispose();
        }

        public void Set(RoomInfo roomInfo)
        {
            if (roomInfo == null)
            {
                return;
            }
            
            RoomInfo = roomInfo;
            titleText.text = roomInfo.Name;

            if (roomInfo.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Owner, out var owner))
            {
                ownerText.text = owner.ToString();
            }

            EmitStatus?.Invoke(
                RoomInfo.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Password, out var password)
                    ? RoomContainerStatusType.Lock.ToString()
                    : RoomContainerStatusType.Unlock.ToString());

            if (roomInfo.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Region, out var region))
            {
                if (!string.IsNullOrEmpty(regionText.ToString()))
                {
                    var key = region.ToString();
                    
                    regionText.text = key;
                    pingText.text = "Pinging";

                    if (startupService.RegionHandler != null)
                    {
                        foreach (var element in startupService.RegionHandler.EnabledRegions)
                        {
                            if (element.Code != key)
                            {
                                continue;
                            }

                            var ping = element.Ping;

                            pingText.text = ping > 1000 ? $"1000+ ms" : $"{ping} ms";
                    
                            break;
                        }
                    }
                }
            }

            RefreshOccupancy(roomInfo.PlayerCount, roomInfo.MaxPlayers);
        }

        public void RefreshOccupancy(int currentOccupancy, int totalOccupancy)
        {
            occupancyText.text = $"{currentOccupancy}/{totalOccupancy}";
        }

        private async void OnJoinButtonClick()
        {
            if (RoomInfo.CustomProperties.TryGetValue(Constants.Room.CustomProperties.Password, out var hashedPasswordObject))
            {
                var hashedPassword = hashedPasswordObject as string;
                
                if (hashedPassword == null)
                {
                    Debug.LogError("Failed to retrieve hashed password.");
                    return;
                }
                
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    TryJoinRoom().Forget();
                    return;
                }

                var passwordPopup = await popupService.ShowPopup(new PopupOptions(Constants.Popups.PasswordPopup)) as PasswordPopup;
                passwordPopup.PasswordEntered.Subscribe(enteredPassword =>
                {
                    if (enteredPassword.ToHashPassword() == hashedPassword)
                    {
                        TryJoinRoom().Forget();
                    }
                    else
                    {
                        popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
                        {
                            Message = $"Incorrect password!"
                        }, PopupGroup.System)).Forget();
                        
                        passwordPopup.Hide().Forget();
                    }
                }).AddTo(compositeDisposable);
            }
            else
            {
                TryJoinRoom().Forget();
            }
        }

        private async UniTask TryJoinRoom()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogError("PhotonNetwork is not connected and ready.");
                return;
            }

            if (!PhotonNetwork.InLobby)
            {
                Debug.LogError("Client is not in lobby.");
                return;
            }

            if (RoomInfo == null)
            {
                Debug.LogError("RoomInfo is null.");
                return;
            }

            var joinProcess = new UniTaskCompletionSource();

            await popupService.ShowPopup(new PopupOptions(Constants.Popups.NetworkLoadingPopup, new NetworkLoadingPopupData
            {
                Source = joinProcess
            }, PopupGroup.System));

            PhotonNetwork.JoinRoom(RoomInfo.Name);

            await UniTask.WhenAny(UniTask.WaitUntil(() => PhotonNetwork.InRoom), UniTask.Delay(TimeSpan.FromSeconds(5)));

            joinProcess.TrySetResult();
        }
    }
}