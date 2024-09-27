using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Character;
using Gameplay.Inventory;
using Gameplay.Network;
using Gameplay.Network.NetworkData;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe.Actions;
using PlayVibe.RolePopup;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.Elements
{
    public class SubmarineInteractiveObject : NeedItemInteractiveObject
    {
        [SerializeField] private SubmarineInteractiveGasolineCanisterObject gasolineCanister;
        [SerializeField] private SubmarineDoorInteractiveObject door;
        [Space]
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private GameObject canvas;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressImage;
        
        private Tweener progressTweener;
        private const int MaxFuel = 3;

        public bool Locked { get; private set; } = true;
        public int FuelTank { get; private set; }
        public bool Activated { get; private set; }
        
        public override void TryInteractive(CharacterView view)
        {
            if (Activated || Locked || FuelTank < MaxFuel)
            {
                FailedInteractive(view);
                return;
            }
            
            if (!InTheBoat(view))
            {
                return;
            }
            
            gameplayController.GetEventHandler<InventoryNetworkEventHandler>().SendRequest(
                PhotonPeerEvents.TryUseItem,
                new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.MasterClient
                },
                new GetItemByTypeNetworkData
                {
                    Owner = gameplayStage.LocalGameplayData.ActorNumber,
                    InventoryType = InventoryType.Character,
                    ItemType = itemKey,
                    RemoveItem = removeItemAfterUse
                },
                response =>
                {
                    if (response.Data is false)
                    {
                        ShowInfoPopup($"Required item: {itemKey}");
                        return;
                    }
         
                    if (!InTheBoat(view))
                    {
                        return;
                    }
                    
                    if (Activated)
                    {
                        return;
                    }
                    
                    popupService.ShowPopup(new PopupOptions(Constants.Popups.Actions.ActionDefaultPopup, new ActionSettings
                    {
                        Duration = 5,
                        Title = "Submarine hack",
                        InvokePosition = view.transform.position,
                        InterruptAfterMove = true,
                        Action = () =>
                        {
                            if (Activated)
                            {
                                return;
                            }
                            
                            SetActivateState(true); 
                            base.TryInteractive(view);
                        }
                    })).Forget();
                });
        }

        private bool InTheBoat(CharacterView view)
        {
            var center = boxCollider.transform.TransformPoint(boxCollider.center);
            var halfExtents = boxCollider.size * 0.5f;
            var colliders = Physics.OverlapBox(center, halfExtents, boxCollider.transform.rotation);
            var characterViews = colliders
                .Select(collider => collider.GetComponent<CharacterView>())
                .Where(characterView => characterView != null)
                .ToList();

            if (!characterViews.Contains(view))
            {
                return false;
            }

            return true;
        }
        
        public void SetActivateState(bool value)
        {
            photonView.RPC("SetActivateStateRPC", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void SetActivateStateRPC(bool value)
        {
            if (Activated)
            {
                return;
            }
            
            Activated = value;

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var eventHandler = gameplayController.GetEventHandler<GameplayNetworkEventHandler>();
            var center = boxCollider.transform.TransformPoint(boxCollider.center);
            var halfExtents = boxCollider.size * 0.5f;
            var colliders = Physics.OverlapBox(center, halfExtents, boxCollider.transform.rotation);
            var characterViews = colliders
                .Select(collider => collider.GetComponent<CharacterView>())
                .Where(characterView => characterView != null)
                .ToList();

            foreach (var characterView in characterViews)
            {
                var actorId = characterView.PhotonView.Owner.ActorNumber;
            
                if (gameplayStage.GameplayDataDic[actorId].RoleType != RoleType.Prisoner)
                {
                    continue;
                }

                eventHandler.SendPrisonerEscape(actorId, EscapeType.Submarine);
            }
            
            gasolineCanister.NetworkDestroy();
            NetworkDestroy();
        }
        
        public void SetLockedState(bool value)
        {
            photonView.RPC("SetLockedStateRPC", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void SetLockedStateRPC(bool value)
        {
            Locked = value;
        }
        
        public void IncrementFuelTank()
        {
            photonView.RPC("IncrementFuelTankRPC", RpcTarget.All);
        }
        
        [PunRPC]
        public void IncrementFuelTankRPC()
        {
            FuelTank = Mathf.Clamp(FuelTank + 1, 0, MaxFuel);
            
            SetFuelTankProgress(FuelTank);
        }
        
        public void ClearFuelTank()
        {
            photonView.RPC("ClearFuelTankRPC", RpcTarget.All);
        }
        
        [PunRPC]
        public void ClearFuelTankRPC()
        {
            FuelTank = 0;
            
            SetFuelTankProgress(FuelTank);
        }

        private void SetFuelTankProgress(int value)
        {
            value = Mathf.Clamp(value, 0, MaxFuel);
            
            var percentage = (float)value / MaxFuel;

            progressText.text = $"{percentage * 100f:0}%";

            progressTweener?.Kill();
            progressTweener = progressImage.DOFillAmount(percentage, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                progressTweener = null;
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            progressTweener?.Kill();
        }
    }
}