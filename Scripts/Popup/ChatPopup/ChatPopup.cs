using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.RolePopup;
using Popup;
using Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class ChatPopup : AbstractBasePopup, IOnEventCallback
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform layoutGroup;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button enterButton;
        [SerializeField] private CanvasGroup chatCanvasGroup;
        [SerializeField] private Ease chatAlphaEase;
        [SerializeField] private float chatAlphaDuration;
        [SerializeField] private float chatInactiveTime;

        [Inject] private ChatColors chatColors;
        [Inject] private Balance balance;
        [Inject] private StageService stageService;

        private Tweener chatAlphaTweener;
        private Tweener scrollTweener;
        private IDisposable timerSubscription;
        private CompositeDisposable hideChatCompositeDisposable;
        private bool enterLock;
        private CompositeDisposable enterTimerCompositeDisposable;
        private RoleType ownerRole;
        
        private readonly List<ChatMessageContainer> containers = new();

        public static bool InFocus { get; protected set; }
        
        protected override UniTask OnShow(object data = null)
        {
            ownerRole = (RoleType)data;
            InFocus = false;
            inputField.text = string.Empty;
            inputField.characterLimit = balance.ChatSettings.MessageCharLimit;
            inputField.onValueChanged.AddListener(_ =>
            {
                InFocus = true;
                ShowChat();
            });
            scrollRect.onValueChanged.AddListener(_ => ShowChat());
            inputField.onSubmit.AddListener(_ => SendChatMessageProcess());
            inputField.onEndEdit.AddListener(_ =>
            {
                inputField.DeactivateInputField();
                InFocus = false;
            });
            enterButton.OnClickAsObservable().Subscribe(_ => SendChatMessageProcess()).AddTo(CompositeDisposable);

            HideChat(true);
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Return)).Subscribe(_ => OnPressEnter()).AddTo(CompositeDisposable);
            
            PhotonNetwork.AddCallbackTarget(this);

            return UniTask.CompletedTask;
        }

        private void OnPressEnter()
        {
            if (enterLock)
            {
                return;
            }
            
            if (!InFocus)
            {
                inputField.ActivateInputField();
                InFocus = true;
            }
            
            ShowChat();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }

        protected override UniTask OnHide()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            
            scrollRect.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();
            timerSubscription?.Dispose();
            scrollTweener?.Kill();
            hideChatCompositeDisposable?.Dispose();

            Clear();
            
            InFocus = false;
            enterTimerCompositeDisposable?.Dispose();
            enterTimerCompositeDisposable = null;
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }

        public void ScrollToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
            
            scrollTweener?.Kill();
            scrollTweener = scrollRect.DOVerticalNormalizedPos(0f, 0.25f).SetEase(Ease.OutCubic).OnComplete(() => scrollTweener = null);
        }

        public void Clear()
        {
            foreach (var container in containers)
            {
                objectPoolService.ReturnToPool(container);
            }
            
            containers.Clear();
        }
        
        private int GetPlayerIndexInRoom()
        {
            var localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            var activePlayers = PhotonNetwork.CurrentRoom.Players.Values
                .Where(player => player != null && player.IsInactive == false)
                .OrderBy(player => player.ActorNumber)
                .ToList();

            for (var i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].ActorNumber == localActorNumber)
                {
                    return i;
                }
            }

            return -1;
        }

        private void SendChatMessageProcess()
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                var playerIndex = GetPlayerIndexInRoom();
                var eventData = new ChatMessageEventData(PhotonNetwork.LocalPlayer.NickName, playerIndex, inputField.text, ownerRole);
                var eventCode = PhotonPeerEvents.SendChatMessageEvent;
                var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
    
                PhotonPeerService.RaiseUniversalEvent(eventCode, eventData, raiseEventOptions, SendOptions.SendReliable);

                ScrollToBottom();
            }
            
            inputField.text = string.Empty;

            if (InFocus)
            {
                inputField.DeactivateInputField();
                InFocus = false;
                enterLock = true;
            
                enterTimerCompositeDisposable?.Dispose();
                enterTimerCompositeDisposable = new CompositeDisposable();
            
                Observable.Timer(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ => enterLock = false)
                    .AddTo(enterTimerCompositeDisposable);
            }
        }

        public async void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != PhotonPeerService.UniversalEventCode)
            {
                return;
            }
            
            if (photonEvent.CustomData is not PhotonPeerData peerData)
            {
                return;
            }

            if (peerData.Code != PhotonPeerEvents.SendChatMessageEvent)
            {
                return;
            }

            if (peerData.CustomData is not ChatMessageEventData eventData)
            {
                return;
            }
            
            if (stageService.CurrentStage is GameplayStage)
            {
                if (eventData.RoleType != ownerRole)
                {
                    return;
                }
            }
            
            var scrollState = scrollRect.verticalNormalizedPosition > 0.01f;
            var container = await objectPoolService.GetOrCreateView<ChatMessageContainer>(Constants.Views.ChatMessageContainer, content, true);
    
            if (State is PopupState.Show or PopupState.Shown)
            {
                var clampIndex = Mathf.Clamp(eventData.Index, 0, chatColors.Data.Count - 1);
                var message = $"<color={chatColors.Data[clampIndex].ToHtmlStringRGB()}>{eventData.Nickname}</color> {eventData.Message}";
                    
                container.Set(message);
                containers.Add(container);

                if (containers.Count > balance.ChatSettings.MessagesLimit)
                {
                    var first = containers.FirstOrDefault();

                    if (first != null)
                    {
                        containers.Remove(first);
                        objectPoolService.ReturnToPool(first);
                    }
                }
                    
                if (scrollState)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
                }
                else
                {
                    ScrollToBottom();
                }

                ShowChat();
            }
            else
            {
                Debug.LogError("Invalid custom data format for chat message event.");
                objectPoolService.ReturnToPool(container);
            }
        }

        public void ShowChat(bool force = false)
        {
            chatAlphaTweener?.Kill();

            if (force)
            {
                chatCanvasGroup.alpha = balance.ChatSettings.MaxAlpha;
                return;
            }

            chatAlphaTweener = chatCanvasGroup.DOFade(balance.ChatSettings.MaxAlpha, balance.ChatSettings.ShowDuration)
                .SetEase(balance.ChatSettings.ShowEase).OnComplete(() => chatAlphaTweener = null);

            hideChatCompositeDisposable?.Dispose();
            hideChatCompositeDisposable = new CompositeDisposable();

            Observable.Timer(TimeSpan.FromSeconds(balance.ChatSettings.HideDelay))
                .Subscribe(_ => { HideChat(); })
                .AddTo(hideChatCompositeDisposable);
        }

        private void HideChat(bool force = false)
        {
            inputField.DeactivateInputField();
            InFocus = false;
            
            chatAlphaTweener?.Kill();

            if (force)
            {
                chatCanvasGroup.alpha = balance.ChatSettings.MinAlpha;
                return;
            }

            chatAlphaTweener = chatCanvasGroup.DOFade(balance.ChatSettings.MinAlpha, balance.ChatSettings.HideDuration)
                .SetEase(balance.ChatSettings.HideEase).OnComplete(() => chatAlphaTweener = null);
        }
    }
}