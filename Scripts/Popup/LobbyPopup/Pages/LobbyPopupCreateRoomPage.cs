using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using PlayVibe.Photon;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PlayVibe.Pages
{
    [Serializable]
    public class LobbyPopupCreateRoomPage : AbstractLobbyPopupPage
    {
        [SerializeField] private TMP_InputField roomNameInputField;
        [SerializeField] private TMP_InputField roomPasswordInputField;
        [SerializeField] private Button createRoomButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Toggle adminPopupToggle;
        [SerializeField] private Toggle autoRoleBalanceToggle;
        [SerializeField] private TMP_Dropdown locationDropdown;
        
        protected override UniTask OnInitialize()
        {
            createRoomButton.OnClickAsObservable().Subscribe(_ => OnCreateRoomButtonClick()).AddTo(CompositeDisposable);
            backButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(CompositeDisposable);

            if (PlayerPrefs.HasKey(Constants.PlayerPrefs.User.PrevRoomName))
            {
                roomNameInputField.text = PlayerPrefs.GetString(Constants.PlayerPrefs.User.PrevRoomName);
            }

            SetupLocations();

            return UniTask.CompletedTask;
        }

        protected override UniTask OnDeinitialize()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnShow()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        private void OnCreateRoomButtonClick()
        {
            PlayerPrefs.SetString(Constants.PlayerPrefs.User.PrevRoomName, roomNameInputField.text);
            
            var selectedIndex = locationDropdown.value;
            var selectedValue = locationDropdown.options[selectedIndex].text;

            eventAggregator.SendEvent(new TryCreateRoomEvent
            {
                OwnerName = PhotonNetwork.NickName,
                RoomName = roomNameInputField.text,
                RoomPassword = roomPasswordInputField.text,
                Region = PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion,
                EnableAdminPopup = adminPopupToggle.isOn,
                AutoRoleBalanceEnabled = autoRoleBalanceToggle.isOn,
                Location = selectedValue
            });
        }

        private void OnBackButtonClick()
        {
            LobbyPopup.SetPage(LobbyPopupPageType.Menu);
        }
        
        private void SetupLocations()
        {
            locationDropdown.ClearOptions();
            locationDropdown.AddOptions(new List<string>
            {
                "Location1",
                "Location2",
                "Location3"
            });
        }
    }
}