using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay.Network.NetworkData;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public abstract class AbstractNetworkEventHandler : IOnEventCallback
    {
        [Inject] protected EventAggregator eventAggregator;
        [Inject] protected PopupService popupService;
        [Inject] protected ObjectPoolService objectPoolService;
        [Inject] protected GameplayStage gameplayStage;

        protected readonly CompositeDisposable CompositeDisposable = new();
        protected readonly Dictionary<PhotonPeerEvents, Action<PhotonPeerData>> events = new();
        protected readonly Dictionary<PhotonPeerEvents, Dictionary<int, Action<RRData>>> responseCallbacks = new();
        
        protected int requestIdCounter;
        
        protected GameplayController GameplayController { get; private set; }
        
        public void Initialize(GameplayController controller)
        {
            requestIdCounter = 0;
            GameplayController = controller;
            
            Subscribes();
            OnInitialized();
        }
        
        public void DeInitialize()
        {
            CompositeDisposable?.Dispose();
            responseCallbacks.Clear();
            events.Clear();
            
            UnSubscribes();
        }
        
        protected void Subscribes()
        {
            PhotonNetwork.AddCallbackTarget(this);
            
            OnSubscribes();
        }

        protected void UnSubscribes()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            
            OnUnSubscribes();
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != PhotonPeerService.UniversalEventCode)
            {
                return;
            }
            
            if (photonEvent.CustomData is not PhotonPeerData peerData)
            {
                return;
            }
            
            if (!events.TryGetValue(peerData.Code, out var action))
            {
                return;
            }
            
            if (TryExecuteResponse(peerData))
            {
                return;
            }
                
            action?.Invoke(peerData);
        }
        
        protected abstract void OnInitialized();
        protected abstract void OnSubscribes();
        protected abstract void OnUnSubscribes();

        public void SendRequest(PhotonPeerEvents eventCode, RaiseEventOptions eventOptions, object data, Action<RRData> response, Action timeoutError = null, float timeoutSeconds = 10f)
        {
            var requestId = requestIdCounter++;

            if (!responseCallbacks.ContainsKey(eventCode))
            {
                responseCallbacks.Add(eventCode, new Dictionary<int, Action<RRData>>());
            }
    
            responseCallbacks[eventCode][requestId] = response;

            Log("Sent request", eventCode, requestId);
    
            var eventData = new RRData
            {
                RequestId = requestId,
                Type = RRType.Request,
                Data = data
            };

            PhotonPeerService.RaiseUniversalEvent(eventCode, eventData, eventOptions, SendOptions.SendReliable);

            WaitForResponseTimeout(eventCode, requestId, timeoutError, timeoutSeconds).Forget();
        }

        private async UniTaskVoid WaitForResponseTimeout(PhotonPeerEvents eventCode, int requestId, Action timeoutError, float timeoutSeconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));

            if (responseCallbacks.ContainsKey(eventCode) && responseCallbacks[eventCode].ContainsKey(requestId))
            {
                responseCallbacks[eventCode].Remove(requestId);
                Log("Timeout error", eventCode, requestId);
                timeoutError?.Invoke();
            }
        }

        private bool TryExecuteResponse(PhotonPeerData peerData)
        {
            if (!responseCallbacks.TryGetValue(peerData.Code, out var responseCode))
            {
                return false;
            }
            
            if (peerData.CustomData is not RRData responseData)
            {
                return false;
            }

            if (responseData.Type != RRType.Response)
            {
                return false;
            }

            if (!responseCode.TryGetValue(responseData.RequestId, out var responseAction))
            {
                return false;
            }
            
            responseAction?.Invoke(responseData);
            responseCode.Remove(responseData.RequestId);
            
            Log("Received response", peerData.Code, responseData.RequestId);
                    
            return true;
        }

        private void Log(string message, PhotonPeerEvents eventCode, int requestId)
        {
            return;
            
            Debug.Log(
                $"{message} {eventCode.AddColorTag(Color.yellow)} {$"request id: {requestId}".AddColorTag(Color.white)} {$"Total: {responseCallbacks[eventCode].Count}".AddColorTag(Color.cyan)}"
                    .AddColorTag(Color.green));
        }

        protected void ShowInfoPopup(string message, float lifeTime = 3f)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message,
                LifeTime = lifeTime
            }, PopupGroup.System)).Forget();
        }
    }
}
