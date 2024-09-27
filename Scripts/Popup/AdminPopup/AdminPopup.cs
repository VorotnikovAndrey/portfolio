using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Gameplay;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.Effects;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Services;
using Services.ExtensionsClasses;
using Services.Gameplay.Wallet;
using Services.Gameplay.Warp;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class AdminPopup : AbstractBasePopup
    {
        [SerializeField] private Button warpAllButton;
        [SerializeField] private Button warpHomeButton;
        [SerializeField] private Button addSoftButton;
        [SerializeField] private Button removeSoftButton;
        [SerializeField] private Button winButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private Button nextDayButton;
        [SerializeField] private Button ghostButton;
        [SerializeField] private TextMeshProUGUI ghostButtonText;
        [SerializeField] private TMP_Dropdown itemsDropdown;
        [SerializeField] private Button giveItemButton;
        [SerializeField] private Image giveItemIcon;
        [SerializeField] private TMP_Dropdown effectsDropdown;
        [SerializeField] private Button addEffectButton;
        [SerializeField] private Button removeEffectButton;
        [SerializeField] private Image giveEffectIcon;
        [SerializeField] private Button removeAllEffectsButton;
        [SerializeField] private Button refreshMinigamesButton;
        [SerializeField] private Button createDropButton;
        [SerializeField] private Button statisticButton;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private GameplayController gameplayController;
        [Inject] private ItemsSettings itemsSettings;
        [Inject] private ItemFactory itemFactory;
        [Inject] private WarpService warpService;
        [Inject] private EffectsSettings effectsSettings;
        [Inject] private PopupService popupService;
        [Inject] private StatisticService statisticService;
        
        protected override UniTask OnShow(object data = null)
        {
            warpAllButton.OnClickAsObservable().Subscribe(_ => OnWarpAllClick()).AddTo(CompositeDisposable);
            warpHomeButton.OnClickAsObservable().Subscribe(_ => OnWarpHomeClick()).AddTo(CompositeDisposable);
            addSoftButton.OnClickAsObservable().Subscribe(_ => OnAddSoftButtonClick()).AddTo(CompositeDisposable);
            removeSoftButton.OnClickAsObservable().Subscribe(_ => OnRemoveSoftButtonClick()).AddTo(CompositeDisposable);
            winButton.OnClickAsObservable().Subscribe(_ => OnWinButtonClick()).AddTo(CompositeDisposable);
            hideButton.OnClickAsObservable().Subscribe(_ => Hide().Forget()).AddTo(CompositeDisposable);
            giveItemButton.OnClickAsObservable().Subscribe(_ => OnGiveItemButtonClick()).AddTo(CompositeDisposable);
            nextDayButton.OnClickAsObservable().Subscribe(_ => OnNextDayButtonClick()).AddTo(CompositeDisposable);
            ghostButton.OnClickAsObservable().Subscribe(_ => OnGhostButtonClick()).AddTo(CompositeDisposable);
            addEffectButton.OnClickAsObservable().Subscribe(_ => OnEffectButtonClick(true)).AddTo(CompositeDisposable);
            removeEffectButton.OnClickAsObservable().Subscribe(_ => OnEffectButtonClick(false)).AddTo(CompositeDisposable);
            removeAllEffectsButton.OnClickAsObservable().Subscribe(_ => OnRemoveAllEffectsClick()).AddTo(CompositeDisposable);
            refreshMinigamesButton.OnClickAsObservable().Subscribe(_ => OnRefreshMinigamesButtonClick()).AddTo(CompositeDisposable);
            createDropButton.OnClickAsObservable().Subscribe(_ => OnCreateDropButtonClick()).AddTo(CompositeDisposable);
            statisticButton.OnClickAsObservable().Subscribe(_ => OnShowStatisticButtonClick()).AddTo(CompositeDisposable);
            
            itemsDropdown.ClearOptions();
            itemsDropdown.AddOptions(itemsSettings.Data.Select(x => x.Key).ToList());
            itemsDropdown.onValueChanged.AddListener(OnItemsDropdownValueChanged);
            
            effectsDropdown.ClearOptions();
            effectsDropdown.AddOptions(Enum.GetValues(typeof(EffectType)).Cast<EffectType>().Select(e => e.ToString()).ToList());
            effectsDropdown.onValueChanged.AddListener(OnEffectsDropdownValueChanged);

            UpdateItemIcon();

            nextDayButton.interactable = PhotonNetwork.IsMasterClient;
            refreshMinigamesButton.interactable = PhotonNetwork.IsMasterClient;
            createDropButton.interactable = PhotonNetwork.IsMasterClient;
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            itemsDropdown.onValueChanged.RemoveListener(OnItemsDropdownValueChanged);
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void OnWarpAllClick()
        {
            var view = gameplayStage.LocalGameplayData.CharacterView;

            if (view == null)
            {
                return;
            }

            var position = view.transform.position;
            
            foreach (var data in gameplayStage.GameplayDataDic.Values)
            {
                if (data.CharacterView == null)
                {
                    continue;
                }

                var targetView = data.CharacterView as CharacterView;

                targetView.SetFloorIndex(view.FloorIndex);
                targetView.Movement.WarpTo(position);
            }
        }
        
        private void OnWarpHomeClick()
        {
            var view = gameplayStage.LocalGameplayData.CharacterView;

            if (view == null)
            {
                return;
            }
            
            warpService.WarpToHome(gameplayStage.LocalGameplayData.ActorNumber);
        }
        
        private void OnRemoveSoftButtonClick()
        {
            gameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(gameplayStage.LocalGameplayData.ActorNumber, CurrencyType.Soft, -100);
        }

        private void OnAddSoftButtonClick()
        {
            gameplayController.GetEventHandler<WalletNetworkEventHandler>().SendModifyCurrency(gameplayStage.LocalGameplayData.ActorNumber, CurrencyType.Soft, 100);
        }
        
        private void OnWinButtonClick()
        {
            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
            {
                gameplayController.GetEventHandler<GameplayNetworkEventHandler>().SendPrisonerEscape(gameplayStage.LocalGameplayData.ActorNumber, EscapeType.Debug);
            }
            else
            {
                var eventCode = PhotonPeerEvents.WinBehavior;
                var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
                PhotonPeerService.RaiseUniversalEvent(eventCode, RoleType.Security, raiseEventOptions, SendOptions.SendReliable);
            }
        }
        
        private void OnGiveItemButtonClick()
        {
            var selectedText = itemsDropdown.captionText.text;
            var eventCode = PhotonPeerEvents.CreateItemFor;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            var data = new CreateItemNetworkData()
            {
                Owner = gameplayStage.LocalGameplayData.ActorNumber,
                InventoryType = InventoryType.Character,
                ItemKey = selectedText
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void OnItemsDropdownValueChanged(int index)
        {
            UpdateItemIcon();
        }

        private void UpdateItemIcon()
        {
            var selectedText = itemsDropdown.captionText.text;
            giveItemIcon.sprite = itemsSettings.Data[selectedText].Icon;
        }
        
        private void OnNextDayButtonClick()
        {
            gameplayController.GetEventHandler<GameplayNetworkEventHandler>().ApplyNextDayTime();
        }
        
        private void OnGhostButtonClick()
        {
            var view = gameplayStage.LocalGameplayData.CharacterView;

            if (view == null)
            {
                return;
            }

            view.Rigidbody.isKinematic = !view.Rigidbody.isKinematic;
            view.CapsuleCollider.enabled = !view.CapsuleCollider.enabled;

            ghostButtonText.text = $"Ghost - {(!view.CapsuleCollider.enabled ? "enabled" : "disabled")}";
        }
        
        private void OnEffectsDropdownValueChanged(int index)
        {
            UpdateEffectIcon();
        }
        
        private void UpdateEffectIcon()
        {
            var selectedText = effectsDropdown.captionText.text;
            giveEffectIcon.sprite = effectsSettings.Get(Enum.Parse<EffectType>(selectedText)).Icon;
        }
        
        private void OnEffectButtonClick(bool isAdd)
        {
            var selectedText = effectsDropdown.captionText.text;
            var eventCode = isAdd ? PhotonPeerEvents.AddEffect : PhotonPeerEvents.RemoveEffect;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            var data = new EffectNetworkData
            {
                Target = gameplayStage.LocalGameplayData.ActorNumber,
                EffectType = Enum.Parse<EffectType>(selectedText)
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void OnRemoveAllEffectsClick()
        {
            var eventCode = PhotonPeerEvents.RemoveAllEffects;
            var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            var data = new EffectNetworkData
            {
                Target = gameplayStage.LocalGameplayData.ActorNumber
            };
    
            PhotonPeerService.RaiseUniversalEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        
        private void OnRefreshMinigamesButtonClick()
        {
            gameplayController.GetEventHandler<QuestsNetworkEventHandler>().GenerateQuestsForPlayers();
        }
        
        private void OnCreateDropButtonClick()
        {
            var position = gameplayStage.LocalGameplayData.CharacterView.transform.position.ToCustomVector3();
            var items = new List<ItemModel>
            {
                itemFactory.CreateModel(itemsSettings.GetRandom(), 0),
                itemFactory.CreateModel(itemsSettings.GetRandom(), 1),
                itemFactory.CreateModel(itemsSettings.GetRandom(), 2),
                itemFactory.CreateModel(itemsSettings.GetRandom(), 3)
            };

            var view = gameplayStage.LocalGameplayData.CharacterView as CharacterView;

            gameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendSpawnDrop(new SpawnDropNetworkData
            {
                Position = position,
                Items = items,
                Floor = view.FloorIndex
            });
        }
        
        private void OnShowStatisticButtonClick()
        {
            statisticService.ShowItemsPopup();
        }
    }
}