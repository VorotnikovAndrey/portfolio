using Cysharp.Threading.Tasks;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player;
using Newtonsoft.Json;
using Photon.Realtime;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class StatisticService
    {
        [Inject] private PopupService popupService;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private GameplayController gameplayController;

        private readonly CompositeDisposable compositeDisposable = new();
        
        public StatisticService()
        {
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.F1)).Subscribe(_ => ShowItemsPopup()).AddTo(compositeDisposable);
        }

        public void ShowItemsPopup()
        {
            gameplayController.GetEventHandler<ItemsNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.GetStatistic,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                null,
                response =>
                {
                    var data = JsonConvert.DeserializeObject<StatisticData>(response.Data.ToString());
                    
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.ItemsStatisticPopup, data, PopupGroup.Overlay)).Forget();
                }
            );
        }

        ~StatisticService()
        {
            compositeDisposable?.Dispose();
        }
    }
}