using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils.Steam;

namespace PlayVibe
{
    public class LobbyRoomUserContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private Button kickButton;
        [SerializeField] private RawImage avatar;

        private CompositeDisposable compositeDisposable;
        
        public Player Player { get; private set; }

        private void OnEnable()
        {
            compositeDisposable = new CompositeDisposable();
            kickButton.OnClickAsObservable().Subscribe(_ => OnKickHandler()).AddTo(compositeDisposable);
        }

        private void OnDisable()
        {
            compositeDisposable?.Dispose();
        }

        public void Set(Player player)
        {
            Player = player;

            userNameText.text = player.NickName;
            kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && !Equals(Player, PhotonNetwork.MasterClient));

            LoadAvatar();
        }

        private void LoadAvatar()
        {
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == Player.ActorNumber);
            
            if (player != null && player.CustomProperties.ContainsKey("SteamID"))
            {
                var steamID = (ulong)player.CustomProperties["SteamID"];
                var avatarId = SteamFriends.GetMediumFriendAvatar(new CSteamID(steamID));
                var avatarTexture = SteamUnitsExtension.GetSteamImageAsTexture2D(avatarId);
            
                avatar.texture = avatarTexture;
            }
            else
            {
                Debug.LogWarning("Player not found or SteamID is missing.");
            }
        }
        
        private void OnKickHandler()
        {
            if (PhotonNetwork.IsMasterClient && Player != null)
            {
                PhotonNetwork.CloseConnection(Player);
            }
        }
    }
}