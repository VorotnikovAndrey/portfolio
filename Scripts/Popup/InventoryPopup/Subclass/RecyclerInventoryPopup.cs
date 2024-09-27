using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using PlayVibe.RolePopup;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace PlayVibe.Subclass
{
    public class RecyclerInventoryPopup : InteractiveInventoryPopup
    {
        [SerializeField] private Button runButton;
        [SerializeField] private GameObject recyclerTimerParent;
        [SerializeField] private TextMeshProUGUI recyclerTimerText;
        [SerializeField] private GameObject slotBlocker;
        
        [Inject] private ViewsHandler viewsHandler;

        private CompositeDisposable timerCompositeDisposable;
        
        protected override AbstractInteractiveObject InteractiveObject { get; set; }
        protected override bool DragAllowed => InteractiveObject.CanInteract(gameplayStage.LocalGameplayData.RoleType);
        protected override int Capacity { get; set; }
        protected override InventoryType InventoryType => InventoryType.Recycler;
        
        protected override void Subscribes()
        {
            base.Subscribes();
            
            runButton.OnClickAsObservable().Subscribe(_ => OnRunButtonClick()).AddTo(CompositeDisposable);
            
            eventAggregator.Add<RecyclerDataUpdatedEvent>(OnRecyclerDataUpdatedEvent);
            
            ConnectToCharacterInventoryPopup();
        }

        protected override void UnSubscribes()
        {
            base.UnSubscribes();
            
            eventAggregator.Remove<RecyclerDataUpdatedEvent>(OnRecyclerDataUpdatedEvent);
            
            timerCompositeDisposable?.Dispose();
            timerCompositeDisposable = null;
        }

        protected override void OnInitialized()
        {
            InteractiveObject = viewsHandler.Recyclers.FirstOrDefault(x => x.NetworkKey == popupData.OwnerId);
            Capacity = balance.Inventory.RecyclerCapacity;
            
            UpdateRunButtonState();
        }
        
        protected override void UpdateTitle()
        {
            title.text = $"RecyclerInventory [id:{popupData.OwnerId}]";
        }
        
        private void OnRunButtonClick()
        {
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendTryRunRecycler(popupData.OwnerId);
        }
        
        private void OnRecyclerDataUpdatedEvent(RecyclerDataUpdatedEvent sender)
        {
            UpdateRunButtonState();
        }
        
        protected override void OnRefreshed()
        {
            base.OnRefreshed();

            gameplayStage.LevelData.RecyclersData.TryGetValue(popupData.OwnerId, out var recyclerData);
            runButton.interactable = popupData.Items.Count > 0 && (recyclerData == null || recyclerData.Enable);
        }

        private void UpdateRunButtonState()
        {
            gameplayStage.LevelData.RecyclersData.TryGetValue(popupData.OwnerId, out var data);

            if (data == null)
            {
                recyclerTimerParent.gameObject.SetActive(false);
                runButton.gameObject.SetActive(gameplayStage.LocalGameplayData.RoleType == RoleType.Security);
                slotBlocker.gameObject.SetActive(false);
                
                return;
            }
            
            var endTime = data.Time;

            if (PhotonNetwork.Time < endTime)
            {
                runButton.gameObject.SetActive(false);
                recyclerTimerParent.gameObject.SetActive(true);
                slotBlocker.gameObject.SetActive(true);
                
                timerCompositeDisposable?.Dispose();
                timerCompositeDisposable = new CompositeDisposable();
                
                recyclerTimerText.text = (endTime - PhotonNetwork.Time).ToTimeFormat();
                
                Observable.Interval(TimeSpan.FromSeconds(1))
                    .Subscribe(_ =>
                    {
                        var currentTime = PhotonNetwork.Time;
                        var timeLeft = endTime - currentTime;
                        
                        recyclerTimerText.text = timeLeft.ToTimeFormat();
                    })
                    .AddTo(timerCompositeDisposable); 
            }
            else
            {
                recyclerTimerParent.gameObject.SetActive(false);
                runButton.gameObject.SetActive(gameplayStage.LocalGameplayData.RoleType == RoleType.Security);
                slotBlocker.gameObject.SetActive(false);
            }
        }

        public void Reactivate()
        {
            recyclerTimerParent.gameObject.SetActive(false);
            runButton.gameObject.SetActive(gameplayStage.LocalGameplayData.RoleType == RoleType.Security);
            slotBlocker.gameObject.SetActive(false);
            
            popupData.Items.Clear();
                        
            timerCompositeDisposable?.Dispose();
            timerCompositeDisposable = null;
                        
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
                
            Refresh(cancellationTokenSource.Token, popupData.Items).Forget();

            ConnectToCharacterInventoryPopup();
        }

        private void ConnectToCharacterInventoryPopup()
        {
            var popups = popupService.GetPopups<CharacterInventoryPopup>(Constants.Popups.Inventory.CharacterInventoryPopup);

            foreach (var popup in popups)
            {
                popup.SecuritySlot.Button.OnClickAsObservable().Subscribe(_ =>
                {
                    gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRandomSeizedItemToRecycler(popupData.OwnerId);
                }).AddTo(CompositeDisposable);
            }
        }
    }
}