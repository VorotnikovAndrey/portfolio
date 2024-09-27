using System;
using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using Photon.Realtime;
using Services;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Timer;

namespace PlayVibe
{
    public class RecyclerInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] private GameObject vfx;

        private CompositeDisposable timerCompositeDisposable;
        private UniversalTimer universalTimer;

        private void Start()
        {
            SetActiveVFX(false);
        }
        
        protected override void Subscribes()
        {
            eventAggregator.Add<RecyclerDataUpdatedEvent>(OnRecyclerDataUpdatedEvent);
        }

        protected override void UnSubscribes()
        {
            eventAggregator.Remove<RecyclerDataUpdatedEvent>(OnRecyclerDataUpdatedEvent);
        }
        
        private void OnRecyclerDataUpdatedEvent(RecyclerDataUpdatedEvent sender)
        {
            if (sender.Data.OwnerId != networkKey)
            {
                return;
            }
            
            var currentTime = PhotonNetwork.Time;
            var endTime = sender.Data.Time;
            var timeLeft = endTime - currentTime;

            universalTimer?.Stop();
            universalTimer = null;
            
            timerCompositeDisposable?.Dispose();
            timerCompositeDisposable = null;
            
            if (timeLeft > 0)
            {
                SetActiveVFX(true);
                
                timerCompositeDisposable = new CompositeDisposable();
                universalTimer = new UniversalTimer();
                
                universalTimer.OnComplete.Subscribe(_ =>
                {
                    SetActiveVFX(false);
                }).AddTo(timerCompositeDisposable);
                
                universalTimer.Run((float)timeLeft, 1, false);
            }
            else
            {
                SetActiveVFX(false);
            }
        }
        
        public override void TryInteractive(CharacterView view)
        {
            if (!canInteract.Contains(gameplayStage.GameplayDataDic[view.photonView.OwnerActorNr].RoleType))
            {
                return;
            }
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.GetRecyclersItemsRequest,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                networkKey,
                response =>
                {   
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Inventory.RecyclerInventoryPopup, response.Data)).Forget();
                });
        }
        
        private void SetActiveVFX(bool value)
        {
            if (vfx == null)
            {
                return;
            }
            
            vfx.SetActive(value);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            universalTimer?.Stop();
            universalTimer = null;
            
            timerCompositeDisposable?.Dispose();
            timerCompositeDisposable = null;
        }
    }
}