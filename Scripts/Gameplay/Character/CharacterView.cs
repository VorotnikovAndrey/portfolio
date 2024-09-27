using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Gameplay.Player.Markers;
using Gameplay.Player.Minigames;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using PlayVibe.RolePopup;
using PlayVibe.SpectatorPopup;
using PlayVibe.Subclass;
using PlayVibe.TradeWaitPopup;
using Services;
using Services.Gameplay.TimeDay;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Gameplay.Character
{
    public class CharacterView : AbstractCharacterView
    {
        [SerializeField] private float interactiveRadius = 1.5f;
        [SerializeField] private LayerMask interactiveLayerMask;
        [SerializeField] private LayerMask arrestLayerMask;
        [SerializeField] private CharacterViewGraphicContainer graphicContainer;
        [SerializeField] private CharacterMovement movement;
        [SerializeField] private CharacterMarker marker;
        [SerializeField] private GameObject modelHolder;
        [SerializeField] private LayerMask visibleLayerMask;
        [SerializeField] private Image mapIndicator;
        [SerializeField] private InteractiveOutlineController interactiveOutlineController;
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterActionBar actionBar;
        [SerializeField] private CharacterAnimationSync animationSync;

        [Inject] private PopupService popupService;
        [Inject] private GameplayController gameplayController;
        [Inject] private Balance balance;
        [Inject] private ViewsHandler viewsHandler;
        [Inject] private TimeDayService timeDayService;
        [Inject] private MinigamesHandler minigamesHandler;
        [Inject] private ControlSettings controlSettings;

        public float InteractiveRadius => interactiveRadius;
        public CharacterMovement Movement => movement;
        public CharacterMarker Marker => marker;
        public GameObject ModelHolder => modelHolder;
        public bool EnemySpotted { get; protected set; }
        public Image MapIndicator => mapIndicator;
        public CharacterActionBar ActionBar => actionBar;
        public CharacterAnimationSync AnimationSync => animationSync;

        private void Start()
        {
            if (photonView.IsMine)
            {
                Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.BackQuote)).Subscribe(_ => OpenAdminPopup()).AddTo(compositeDisposable);
                Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.Interact])).Subscribe(_ => TryInteractive()).AddTo(compositeDisposable);
                Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.Map])).Subscribe(_ => OpenMap()).AddTo(compositeDisposable);
                Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.CraftOrQuests])).Subscribe(_ => OnTabPress()).AddTo(compositeDisposable);
                Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.SpellShop])).Subscribe(_ => OnShowSpellShop()).AddTo(compositeDisposable);
                
                switch (gameplayStage.LocalGameplayData.RoleType)
                {
                    case RoleType.Security:
                        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.Affect])).Subscribe(_ => TryArrest()).AddTo(compositeDisposable);
                        break;
                    case RoleType.Prisoner:
                        Observable.EveryUpdate().Where(_ => Input.GetKeyDown(controlSettings.Data[ControlType.Affect])).Subscribe(_ => TryTrade()).AddTo(compositeDisposable);
                        break;
                    case RoleType.None:
                        break;
                    case RoleType.Random:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                MapIndicator.color = Color.green;
                
                SetFloorIndex(1);
                
                graphicContainer.SetStateForInteractiveOutline(true);
            }
            else
            {
                rigidBody.isKinematic = true;
                
                var viewRole = gameplayStage.GameplayDataDic[photonView.Owner.ActorNumber].RoleType;
                var playerRole = gameplayStage.LocalGameplayData.RoleType;
                
                MapIndicator.color = viewRole == playerRole ? Color.green : Color.red;
                
                graphicContainer.SetStateForInteractiveOutline(false);
            }

            var role = gameplayStage.GameplayDataDic[photonView.Owner.ActorNumber].RoleType;
            
            effectsHandler.Setup(this);
            graphicContainer.SetRole(role);
            interactiveOutlineController.Setup(role);
            nicknameText.text = gameplayStage.GameplayDataDic[PhotonView.OwnerActorNr].Nickname;
            nicknameText.gameObject.SetActive(balance.Main.ShowNickname);
            actionBar.Setup(this);
            actionBar.Hide();
        }

        private void OpenMap()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (popupService.HasPopup(Constants.Popups.MapPopup))
            {
                popupService.TryHidePopup(Constants.Popups.MapPopup).Forget();
            }
            else
            {
                if (IsBusy)
                {
                    return;
                }
                
                popupService.ShowPopup(new PopupOptions(Constants.Popups.MapPopup, null, PopupGroup.Overlay)).Forget();
            }
        }
        
        private void OpenAdminPopup()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(Constants.Room.CustomProperties.EnableAdminPopup, out var enableAdminPopup);

            if (enableAdminPopup is false)
            {
                return;
            }
            
            if (popupService.HasPopup(Constants.Popups.AdminPopup))
            {
                popupService.TryHidePopup(Constants.Popups.AdminPopup).Forget();
            }
            else
            {
                popupService.ShowPopup(new PopupOptions(Constants.Popups.AdminPopup, null, PopupGroup.Overlay)).Forget();
            }
        }

        private void Update()
        {
            CheckVisibility();
        }

        private void TryTrade()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (IsBusy)
            {
                return;
            }

            var distance = balance.Interactive.MinTradeDistance;
            var view = CheckCharacterViewRaycast(transform.forward, distance);

            if (view == null)
            {
                var rayCount = 20;
                var angleStep = 360f / rayCount;

                for (var i = 0; i < rayCount; i++)
                {
                    var angle = i * angleStep;
                    var direction = Quaternion.Euler(0, angle, 0) * transform.forward;

                    view = CheckCharacterViewRaycast(direction, distance);

                    if (view != null)
                    {
                        break;
                    }
                }
            }

            if (view == null || view == this)
            {
                return;
            }

            if (gameplayStage.GameplayDataDic[view.PhotonView.Owner.ActorNumber].RoleType != RoleType.Prisoner)
            {
                return;
            }

            var viewActorNumber = view.PhotonView.OwnerActorNr;
            var nickname = gameplayStage.GameplayDataDic[viewActorNumber].Nickname;

            if (view.IsBusy)
            {
                ShowInfoPopup(string.Format(Constants.Messages.Trade.IsBusy, nickname));
                return;
            }
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.TradeWaitPopup, new TradeWaitPopupData
            {
                Message = string.Format(Constants.Messages.Trade.WaitResponse, nickname),
                ActorNumber = viewActorNumber,
                StartTime = PhotonNetwork.Time,
                EndTime = PhotonNetwork.Time + balance.RequestTimeout.Trade
                
            })).Forget();

            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.OfferTrade,
                new RaiseEventOptions
                {
                    TargetActors = new[] { viewActorNumber }
                },
                null,
                response =>
                {
                    if (response.Data is false)
                    {
                        ShowInfoPopup(string.Format(Constants.Messages.Trade.CancelTrade, gameplayStage.GameplayDataDic[viewActorNumber].Nickname));
                    }

                    popupService.TryHidePopup(Constants.Popups.TradeWaitPopup).Forget();
                }, () =>
                {
                    ShowInfoPopup(string.Format(Constants.Messages.Trade.WaitConfirmTimeOut, gameplayStage.GameplayDataDic[viewActorNumber].Nickname));
                    
                    popupService.TryHidePopup(Constants.Popups.TradeWaitPopup).Forget();
                }, balance.RequestTimeout.Trade
            );
        }

        private CharacterView CheckCharacterViewRaycast(Vector3 direction, float distance)
        {
#if UNITY_EDITOR
            Debug.DrawRay(Center.position, direction * distance, Color.blue, 1f);
#endif
            
            if (Physics.Raycast(Center.position, direction, out var hit, distance,arrestLayerMask, QueryTriggerInteraction.Collide))
            {
                var view = hit.collider.gameObject.GetComponent<CharacterView>();

                if (view != null)
                {
                    return view;
                }
            }

            return null;
        }
        
        private void TryArrest()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (IsBusy)
            {
                return;
            }
            
            var colliders = Physics.OverlapSphere(Center.position, balance.Interactive.ArrestDistance, arrestLayerMask, QueryTriggerInteraction.Collide);
            
            foreach (var element in colliders)
            {
                if (element == CapsuleCollider)
                {
                    continue;
                }
                
                var view = element.gameObject.GetComponent<CharacterView>();

                if (view == null || gameplayStage.GameplayDataDic[view.PhotonView.Owner.ActorNumber].RoleType == RoleType.Security)
                {
                    continue;
                }

                var isHit = true;
                var direction = (view.Overhead.position - Overhead.position).normalized;
                var distanceToOtherPlayer = Vector3.Distance(view.Overhead.position, Overhead.position);
                var hits = Physics.RaycastAll(Overhead.position, direction, distanceToOtherPlayer, visibleLayerMask);
                                
                if (hits.Select(hit => hit.collider.gameObject.layer).Any(hitLayer => hitLayer == 0))
                {
                    isHit = false;
                }

                if (isHit)
                {
                    gameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendArrest(new ArrestNetworkData
                    {
                        CasterId = photonView.Owner.ActorNumber,
                        TargetId = view.PhotonView.Owner.ActorNumber
                    });
                }
                
                return;
            }
        }

        private bool CanInteractive()
        {
            if (popupService.HasPopupByType<RecyclerInventoryPopup>())
            {
                return false;
            }
            
            if (popupService.HasPopupByType<MapItemBoxInventoryPopup>())
            {
                return false;
            }
            
            return true;
        }
        
        private void TryInteractive()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (IsBusy)
            {
                return;
            }
            
            if (!CanInteractive())
            {
                return;
            }
            
            if (CheckInteractiveRaycast(transform.forward))
            {
                return;
            }

            var rayCount = 20;
            var angleStep = 360f / rayCount;

            for (var i = 0; i < rayCount; i++)
            {
                var angle = i * angleStep;
                var direction = Quaternion.Euler(0, angle, 0) * transform.forward;

                if (CheckInteractiveRaycast(direction))
                {
                    return;
                }
            }
            
            var colliders = Physics.OverlapSphere(Center.position, interactiveRadius, interactiveLayerMask, QueryTriggerInteraction.Collide);

            foreach (var element in colliders)
            {
                var interactive = element.GetComponent<AbstractInteractiveObject>();

                if (interactive == null)
                {
                    continue;
                }

                interactive.TryInteractive(this);
            }
        }

        private bool CheckInteractiveRaycast(Vector3 direction, bool tryInteractive = true)
        {
#if UNITY_EDITOR
            Debug.DrawRay(Center.position, direction * interactiveRadius, Color.blue, 1f);
#endif
            
            if (Physics.Raycast(Center.position, direction, out var hit, interactiveRadius,interactiveLayerMask, QueryTriggerInteraction.Collide))
            {
                var interactive = hit.collider.gameObject.GetComponent<AbstractInteractiveObject>();

                if (interactive == null)
                {
                    return false;
                }

                if (tryInteractive)
                {
                    interactive.TryInteractive(this);
                }
                
                return true;
            }

            return false;
        }

        public List<T> GetInteractiveObjects<T>() where T : AbstractInteractiveObject
        {
            var colliders = Physics.OverlapSphere(Center.position, interactiveRadius, interactiveLayerMask, QueryTriggerInteraction.Collide);
            return colliders.Select(element => element.gameObject.GetComponent<T>()).Where(interactive => interactive != null).ToList();
        }
        
        private void CheckVisibility()
        {
            if (!photonView.IsMine && SpectatorPopup.Target != transform)
            {
                return;
            }

            if (SpectatorPopup.Target == transform)
            {
                ModelHolder.SetActive(true);
            }

            var anyHit = false;
            
            foreach (var data in gameplayStage.GameplayDataDic)
            {
                if (data.Key == photonView.Owner.ActorNumber)
                {
                    continue;
                }
                
                var view = data.Value.CharacterView as CharacterView;
                
                if (view == null)
                {
                    continue;
                }

                var directionToOtherPlayer = (view.Overhead.position - Overhead.position).normalized;
                var distanceToOtherPlayer = Vector3.Distance(view.Overhead.position, Overhead.position);
                var isHit = true;

                if (distanceToOtherPlayer > 1f)
                {
                    if (distanceToOtherPlayer <= balance.Main.VisibilityDistance)
                    {
                        var hits = Physics.RaycastAll(Overhead.position, directionToOtherPlayer, distanceToOtherPlayer, visibleLayerMask);

                        if (hits.Select(hit => hit.collider.gameObject.layer).Any(hitLayer => hitLayer == 0))
                        {
                            isHit = false;
                        }

                        if (!isHit)
                        {
                            if (gameplayStage.GameplayDataDic[PhotonView.OwnerActorNr].RoleType == RoleType.Security)
                            {
                                foreach (var observer in viewsHandler.SecurityObservers.Where(x => x.EnableIn.Contains(timeDayService.CurrentState)))
                                {
                                    var observerDirection = (view.Overhead.position - observer.transform.position).normalized;
                                    var observerDistanceToOtherPlayer = Mathf.Clamp(Vector3.Distance(view.Overhead.position, observer.transform.position), 0, observer.Distance);
                                    var observerHits = Physics.RaycastAll(observer.transform.position, observerDirection, observerDistanceToOtherPlayer, visibleLayerMask);
                                
                                    if (observerHits.Select(hit => hit.collider.gameObject.layer).Any(hitLayer => hitLayer == 0))
                                    {
                                        isHit = false;
                                    }
                                    else
                                    {
                                        isHit = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                isHit = false;
                            }
                        }
                    }
                    else
                    {
                        isHit = false;
                    }
                }

                view.ModelHolder.SetActive(isHit);

                if (!isHit)
                {
                    continue;
                }
                
                anyHit = true;

                if (photonView.IsMine)
                {
                    if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
                    {
                        continue;
                    }

                    if (gameplayStage.GameplayDataDic[view.PhotonView.Owner.ActorNumber].RoleType == RoleType.Security)
                    {
                        continue;
                    }
                    
                    if ((timeDayService.CurrentState == TimeDayState.Night || view.Marker.CurrentMarker is MarkerType.Violator or MarkerType.Night) && distanceToOtherPlayer < balance.Markers.DetectionRadius)
                    {
                        var hits = Physics.RaycastAll(Overhead.position, directionToOtherPlayer, distanceToOtherPlayer, visibleLayerMask);

                        if (hits.Select(hit => hit.collider.gameObject.layer).Any(hitLayer => hitLayer == 0))
                        {
                            
                        }
                        else
                        {
                            gameplayController.GetEventHandler<ViewsNetworkEventHandler>().SendAddMarker(view.PhotonView.Owner.ActorNumber, new List<MarkerType>
                            {
                                MarkerType.Wanted
                            });
                        }
                    }
                }
            }

            EnemySpotted = anyHit;
        }
        
        private void OnTabPress()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }
            
            if (gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner)
            {
                popupService.ShowPopup(new PopupOptions(Constants.Popups.SelfCraftPopup)).Forget();
            }

            if (gameplayStage.LocalGameplayData.Quests.Count > 0)
            {
                popupService.ShowPopup(new PopupOptions(Constants.Popups.QuestsPopup)).Forget();
            }
        }
        
        private void OnShowSpellShop()
        {
            if (ChatPopup.InFocus)
            {
                return;
            }

            if (popupService.HasPopup(Constants.Popups.SpellShopPopup))
            {
                popupService.TryHidePopup(Constants.Popups.SpellShopPopup).Forget();
                return;
            }

            popupService.ShowPopup(new PopupOptions(Constants.Popups.SpellShopPopup)).Forget();
        }
        
        private void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }
    }
}