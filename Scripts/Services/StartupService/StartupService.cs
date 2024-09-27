using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using Steamworks;
using UnityEngine;

namespace Services
{
    public class StartupService : IConnectionCallbacks
    {
        private UniTaskCompletionSource connectToMasterCompletionSource;
        
        public RegionHandler RegionHandler { get; private set; }
        
        public StartupService()
        {
            PhotonNetwork.AddCallbackTarget(this);
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.EnableCloseConnection = true;
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
        }
        
        public async UniTask Run()
        {
            if (PhotonNetwork.IsConnected)
            {
                return;
            }

            await Connect();
        }

        private async UniTask Connect()
        {
            await UniTask.WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
            
            connectToMasterCompletionSource = new UniTaskCompletionSource();
            
            PhotonNetwork.ConnectUsingSettings();
            
            await connectToMasterCompletionSource.Task;
            
            Debug.Log($"PhotonNetwork initialized. GameVersion: {PhotonNetwork.GameVersion.AddColorTag(Color.yellow)}".AddColorTag(Color.cyan));
        }

        public void OnConnected()
        {
            Debug.Log($"PhotonNetwork OnConnected".AddColorTag(Color.cyan));
        }

        public void OnConnectedToMaster()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.ReconnectAndRejoin();
                Debug.Log($"PhotonNetwork OnConnectedToMaster + ReconnectAndRejoin".AddColorTag(Color.cyan));
                return;
            }

            if (SteamManager.Initialized)
            {
                PhotonNetwork.NickName = SteamFriends.GetPersonaName();
            }

            connectToMasterCompletionSource?.TrySetResult();
            
            Debug.Log($"PhotonNetwork OnConnectedToMaster".AddColorTag(Color.cyan));
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            PhotonNetwork.ReconnectAndRejoin();
            
            Debug.Log($"PhotonNetwork OnDisconnected {cause.AddColorTag(Color.yellow)}".AddColorTag(Color.cyan));
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            regionHandler.PingMinimumOfRegions(OnRegionPingCompleted, null);
            
            Debug.Log($"PhotonNetwork OnRegionListReceived".AddColorTag(Color.cyan));
        }
        
        private void OnRegionPingCompleted(RegionHandler regionHandler)
        {
            RegionHandler = regionHandler;
            
            //PhotonNetwork.ConnectToRegion(regionHandler.BestRegion.Code);
            Debug.Log($"PhotonNetwork OnRegionPingCompleted".AddColorTag(Color.cyan));
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            Debug.Log($"PhotonNetwork OnCustomAuthenticationResponse".AddColorTag(Color.cyan));
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.Log($"PhotonNetwork OnCustomAuthenticationFailed".AddColorTag(Color.cyan));
        }
    }
}