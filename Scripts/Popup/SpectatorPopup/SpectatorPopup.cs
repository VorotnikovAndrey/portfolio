using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Network;
using Gameplay.Player;
using Photon.Pun;
using PlayVibe.RolePopup;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.SpectatorPopup
{
    public class SpectatorPopup : AbstractBasePopup
    {
        public static Transform Target;
        
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private FloorsHandler floorsHandler;

        private GameplayData target;
        private List<GameplayData> players;
        private RoleType ownerRole;
        private LocationCameraController locationCameraController;
        private int targetIndex;
        
        protected override UniTask OnShow(object data = null)
        {
            if (gameplayStage.LocalGameplayData.RoleType != RoleType.Prisoner)
            {
                Hide().Forget();
                
                return UniTask.CompletedTask;
            }
            
            ownerRole = gameplayStage.LocalGameplayData.RoleType;
            locationCameraController = gameplayStage.LocalGameplayData.LocationCamera;
            players = gameplayStage.GameplayDataDic.Values.Where(x => x.RoleType == ownerRole && !x.Escaped).ToList();

            leftButton.OnClickAsObservable().Subscribe(_ => OnLeftButtonClick()).AddTo(CompositeDisposable);
            rightButton.OnClickAsObservable().Subscribe(_ => OnRightButtonClick()).AddTo(CompositeDisposable);
            
            eventAggregator.Add<PrisonerEscapedEvent>(OnPrisonerEscapedEvent);
            
            rightButton.interactable = leftButton.interactable = false;
            
            DefineTarget();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            eventAggregator.Remove<PrisonerEscapedEvent>(OnPrisonerEscapedEvent);
            
            Target = null;
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void OnLeftButtonClick()
        {
            if (!HasAvailableTargets())
            {
                return;
            }
            
            targetIndex--;
                
            if (targetIndex < 0)
            {
                targetIndex = players.Count - 1;
            }

            DefineTarget();
        }

        private void OnRightButtonClick()
        {
            if (!HasAvailableTargets())
            {
                return;
            }
            
            targetIndex++;
                
            if (targetIndex >= players.Count)
            {
                targetIndex = 0;
            }

            DefineTarget();
        }

        private void DefineTarget()
        {
            if (players == null || players.Count == 0 || !HasAvailableTargets())
            {
                Hide().Forget();
                return;
            }

            if (players[targetIndex].Escaped)
            {
                return;
            }

            var data = players[targetIndex];

            if (data == null || target == data || data == gameplayStage.LocalGameplayData)
            {
                return;
            }

            target = data;
            Target = target.CharacterView.transform;

            locationCameraController.MoveTo(target.CharacterView.transform.position);
            locationCameraController.FollowTo(target.CharacterView.transform);
            floorsHandler.SetFloor(target.CharacterView.FloorIndex);

            nicknameText.text = $"{PhotonNetwork.PlayerList.FirstOrDefault(x => x.ActorNumber == target.ActorNumber)?.NickName}";

            var interactableButton = players.Count(x => x.CharacterView != null) > 1;
            
            rightButton.interactable = leftButton.interactable = interactableButton;
        }

        private bool HasAvailableTargets()
        {
            return players.Any(player => !player.Escaped);
        }
        
        private void OnPrisonerEscapedEvent(PrisonerEscapedEvent sender)
        {
            if (!HasAvailableTargets())
            {
                return;
            }
            
            players = gameplayStage.GameplayDataDic.Values.Where(x => x.RoleType == ownerRole && !x.Escaped).ToList();
            
            if (players == null || players.Count == 0 || !HasAvailableTargets())
            {
                Hide().Forget();
                return;
            }
            
            targetIndex = 0;

            DefineTarget();
        }
    }
}