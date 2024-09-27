using System;
using Photon.Pun;
using Steamworks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils.Steam;

namespace PlayVibe.FriendsPopup
{
    public class FriendContainer : PoolView
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [SerializeField] private TextMeshProUGUI inGameText;
        [SerializeField] private GameObject actionParent;
        [SerializeField] private CanvasGroup canvasGroup;

        private CompositeDisposable compositeDisposable;
        
        private readonly Subject<FriendContainer> emitClick = new();

        public IObservable<FriendContainer> EmitClick => emitClick;
        public FriendStatus Status { get; private set; }

        public void Setup(ref CSteamID steamID)
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            LoadNickname(steamID);
            LoadAvatar(steamID);
            LoadStatus(steamID);
        }

        private void LoadNickname(CSteamID cSteamID)
        {
            var nickname = SteamFriends.GetFriendPersonaName(cSteamID);

            if (nickname.Length > Constants.Global.MaxNicknameLength)
            {
                nickname = nickname[..Constants.Global.MaxNicknameLength] + "...";
            }

            nicknameText.text = nickname;
        }
        
        private void LoadAvatar(CSteamID cSteamID)
        {
            var avatarId = SteamFriends.GetMediumFriendAvatar(cSteamID);
            var avatarTexture = SteamUnitsExtension.GetSteamImageAsTexture2D(avatarId);
            
            rawImage.texture = avatarTexture;
        }

        private void LoadStatus(CSteamID cSteamID)
        {
            switch (SteamFriends.GetFriendPersonaState(cSteamID))
            {
                case EPersonaState.k_EPersonaStateOnline when SteamFriends.GetFriendGamePlayed(cSteamID, out var friendGameInfo):
                {
                    canvasGroup.alpha = 1;
                    inGameText.gameObject.SetActive(true);
                    
                    if (friendGameInfo.m_gameID.AppID().m_AppId == Constants.Global.SteamAppId)
                    {
                        Status = FriendStatus.OnlineInGame;
                        inGameText.text = "In-game";
                        inGameText.color = ColorUtility.HexToColor(Constants.Colors.FriendInGameColor);
                        
                    }
                    else
                    {
                        Status = FriendStatus.OnlineInAnotherGame;
                        inGameText.text = "In another game";
                        inGameText.color = ColorUtility.HexToColor(Constants.Colors.FriendInAnotherGameColor);
                    }

                    break;
                }
                case EPersonaState.k_EPersonaStateBusy:
                case EPersonaState.k_EPersonaStateSnooze:
                case EPersonaState.k_EPersonaStateAway:
                case EPersonaState.k_EPersonaStateOnline:
                    canvasGroup.alpha = 1;
                    Status = FriendStatus.Online;
                    inGameText.gameObject.SetActive(true);
                    inGameText.text = "Online";
                    inGameText.color = ColorUtility.HexToColor(Constants.Colors.FriendOnline);
                    break;
                default:
                    Status = FriendStatus.Offline;
                    canvasGroup.alpha = 0.2f;
                    inGameText.gameObject.SetActive(false);
                    break;
            }
            
            DefineAction(cSteamID);
        }

        private void DefineAction(CSteamID cSteamID)
        {
            compositeDisposable?.Dispose();
            compositeDisposable = new CompositeDisposable();
            
            switch (Status)
            {
                case FriendStatus.OnlineInGame:
                case FriendStatus.OnlineInAnotherGame:
                case FriendStatus.Online:
                    
                    if (PhotonNetwork.InRoom)
                    {
                        actionParent.SetActive(true);
                        actionButtonText.text = "Invite";
                        actionButton.OnClickAsObservable().Subscribe(_ => ActionInvite(cSteamID)).AddTo(compositeDisposable);
                    }
                    else
                    {
                        actionParent.SetActive(false);
                    }
                    break;
                case FriendStatus.Offline:
                    actionParent.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ActionInvite(CSteamID cSteamID)
        {
            var roomName = PhotonNetwork.CurrentRoom.Name;
            SteamFriends.InviteUserToGame(cSteamID, $"join?room={roomName}");
        }
    }
}