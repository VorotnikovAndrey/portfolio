using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player;
using Newtonsoft.Json;
using Photon.Realtime;
using Services;
using Services.Gameplay.TimeDay;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class ItemsStatisticPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform buttonContent;
        [SerializeField] private RectTransform itemsContent;
        [Space] 
        [SerializeField] private TextMeshProUGUI totalText;
        [SerializeField] private Button hideButton;
        [SerializeField] private GameObject waitUI;

        [Inject] private TimeDayService timeDayService;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private GameplayController gameplayController;

        private StatisticData popupData;
        private CancellationTokenSource cancellationTokenSource;
        private CompositeDisposable compositeDisposable;
        
        private readonly HashSet<int> targetTimeOfDayChangeCounter = new();
        private readonly Dictionary<string, ItemsStatisticContainer> itemsStatisticContainers = new();
        private readonly List<ItemStatisticDayButton> itemStatisticDayButtons = new();
        private readonly List<ItemStatisticPresetContainer> itemStatisticPresetContainers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            if (data is not StatisticData statisticData)
            {
                Hide().Forget();
                return UniTask.CompletedTask;
            }

            popupData = statisticData;
            
            targetTimeOfDayChangeCounter.Clear();
            
            for (var i = 0; i < balance.Main.RoundMax * 2; i++)
            {
                if (i > gameplayStage.TimeOfDayChangeCounter)
                {
                    continue;
                }
                
                targetTimeOfDayChangeCounter.Add(i);
            }
            
            Refresh();
            Subscribes();

            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            InputDisabler.Clear();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            InputDisabler.Disable();
            UnSubscribes();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            Clear();
        }
        
        private void Subscribes()
        {
            eventAggregator.Add<TimeOfDayChangeCounterUpdatedEvent>(OnTimeOfDayChangeCounterUpdatedEvent);
        }

        private void UnSubscribes()
        {
            eventAggregator.Remove<TimeOfDayChangeCounterUpdatedEvent>(OnTimeOfDayChangeCounterUpdatedEvent);
        }
        
        private void OnTimeOfDayChangeCounterUpdatedEvent(TimeOfDayChangeCounterUpdatedEvent sender)
        {
            targetTimeOfDayChangeCounter.Add(sender.Index);

            gameplayController.GetEventHandler<ItemsNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.GetStatistic,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                null,
                response =>
                {
                    popupData = JsonConvert.DeserializeObject<StatisticData>(response.Data.ToString());
                        
                    Refresh();
                }
            );
        }

        private void Refresh()
        {
            Clear();
            
            compositeDisposable = new CompositeDisposable();
            cancellationTokenSource = new CancellationTokenSource();
            
            Create(cancellationTokenSource.Token).Forget();
        }

        private async UniTask Create(CancellationToken token)
        {
            var index = 0;

            totalText.text = string.Empty;
            waitUI.SetActive(true);

            // buttons
            for (var i = 0; i < balance.Main.RoundMax; i++)
            {
                for (var y = 0; y < 2; y++)
                {
                    var dayButton = await objectPoolService.GetOrCreateView<ItemStatisticDayButton>(Constants.Views.ItemStatisticDayButton, buttonContent);

                    if (token.IsCancellationRequested)
                    {
                        objectPoolService.ReturnToPool(dayButton);
                        return;
                    }
                
                    diContainer.InjectGameObject(dayButton.gameObject);
                    itemStatisticDayButtons.Add(dayButton);
                
                    dayButton.Initialize(index, i, y == 0 ? TimeDayState.Day : TimeDayState.Night, targetTimeOfDayChangeCounter.Contains(index));
                    index++;
                    
                    dayButton.EmitClick.Subscribe(x =>
                    {
                        if (!targetTimeOfDayChangeCounter.Add(x.Index))
                        {
                            targetTimeOfDayChangeCounter.Remove(x.Index);
                        }

                        Refresh();
                    }).AddTo(compositeDisposable);
                    dayButton.gameObject.SetActive(true);
                }
            }

            // items
            foreach (var itemData in itemsSettings.Data)
            {
                var container = await objectPoolService.GetOrCreateView<ItemsStatisticContainer>(Constants.Views.ItemStatisticContainer, itemsContent);
                
                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }
                
                diContainer.InjectGameObject(container.gameObject);
                
                container.Initialize(itemData.Key);
                
                itemsStatisticContainers.Add(container.ItemKey, container);
            }

            // init
            foreach (var pair in popupData.StatisticItemData)
            {
                var container = itemsStatisticContainers[pair.Key];
                
                container.SetStatistic(pair.Value, targetTimeOfDayChangeCounter);
            }
            
            // sort
            var sortedContainers = itemsStatisticContainers
                .OrderByDescending(pair => pair.Value.SuccessfulCount)
                .Select(pair => pair.Value)
                .ToList();
            
            for (var i = 0; i < sortedContainers.Count; i++)
            {
                var target = sortedContainers[i];
                target.transform.SetSiblingIndex(i);
                target.gameObject.SetActive(true);
                target.SetIndex(i);
            }
            
            // fill
            var maxSuccessfulCount = itemsStatisticContainers.Values.Sum(container => container.SuccessfulCount);

            totalText.text = $"Total: {maxSuccessfulCount}";

            if (maxSuccessfulCount != 0)
            {
                foreach (var pair in itemsStatisticContainers)
                {
                    var container = pair.Value;
                    var fillAmount = (float)container.SuccessfulCount / maxSuccessfulCount;

                    container.SetFill(fillAmount);
                }
            }
            
            // presets

            foreach (var container in itemsStatisticContainers)
            {
                popupData.StatisticItemData.TryGetValue(container.Key, out var result);

                if (result == null)
                {
                    continue;
                }
                
                var presetNames = new HashSet<string>();

                foreach (var dayEntry in result.SuccessfulCounts)
                {
                    foreach (var element in dayEntry.Value.Keys)
                    {
                        presetNames.Add(element);
                    }
                }

                var presetIndex = 1;
                var containers = new List<ItemStatisticPresetContainer>();

                foreach (var presetName in presetNames)
                {
                    var preset = await objectPoolService.GetOrCreateView<ItemStatisticPresetContainer>(Constants.Views.ItemStatisticPresetContainer, container.Value.PresetsParent);
                 
                    if (token.IsCancellationRequested)
                    {
                        objectPoolService.ReturnToPool(preset);
                        return;
                    }
                
                    diContainer.InjectGameObject(preset.gameObject);
                
                    preset.Initialize(presetIndex, presetName, result, targetTimeOfDayChangeCounter);
                    preset.gameObject.SetActive(true);
                    
                    containers.Add(preset);
                    
                    presetIndex++;
                }
                
                var presetsMaxSuccessfulCount = containers.Sum(x => x.SuccessfulCount);
                
                if (presetsMaxSuccessfulCount != 0)
                {
                    foreach (var element in containers)
                    {
                        var fillAmount = (float)element.SuccessfulCount / presetsMaxSuccessfulCount;

                        element.SetFill(fillAmount);
                    }
                }

                foreach (var element in containers)
                {
                    itemStatisticPresetContainers.Add(element);
                }
            }
            
            waitUI.SetActive(false);
        }

        private void Clear()
        {
            compositeDisposable?.Dispose();
            compositeDisposable = null;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            
            itemsStatisticContainers.ForEach(x => objectPoolService.ReturnToPool(x.Value));
            itemStatisticDayButtons.ForEach(x => objectPoolService.ReturnToPool(x));
            itemStatisticPresetContainers.ForEach(x => objectPoolService.ReturnToPool(x));
            
            itemsStatisticContainers.Clear();
            itemStatisticDayButtons.Clear();
            itemStatisticPresetContainers.Clear();
        }
    }
}