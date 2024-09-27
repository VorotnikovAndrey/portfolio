using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Player.Minigames;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class MinigameInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] private MinigameType minigameType;
        [PreviewField] [SerializeField] private Sprite questIcon;
        [TextArea(3, 6)][SerializeField] private string description;
        
        [Inject] private MinigamesSettings minigamesSettings;

        public bool State { get; protected set; }

        public MinigameType MinigameType => minigameType;
        public string Description => description;
        public Sprite QuestIcon => questIcon;
        
        private void OnValidate()
        {
            gameObject.name = $"Minigame_{minigameType}";

            if (canInteract.Count == 0)
            {
                return;
            }
            
            foreach (var role in canInteract)
            {
                gameObject.name += $"_{role}";
            }
            
            if (hColorData != null)
            {
                hColorData.TextColor = ColorUtility.HexToColor("73d437");
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            SetState(false);
        }
        
        public override void TryInteractive(CharacterView view)
        {
            if (!State)
            {
                return;
            }
            
            if (!canInteract.Contains(gameplayStage.GameplayDataDic[view.photonView.OwnerActorNr].RoleType))
            {
                return;
            }
            
            popupService.ShowPopup(new PopupOptions(Constants.Popups.Minigames.MinigameDefaultPopup, this)).Forget();   
        }

        public void SetState(bool value)
        {
            State = value;
            
            SetOutlineState(value);
        }

        public void SetId(int id)
        {
            networkKey = id;
        }
        
        public override void SetOutlineState(bool value)
        {
            if (interactiveOutline == null)
            {
                return;
            }
            
            interactiveOutline.enabled = State;
        }
    }
}