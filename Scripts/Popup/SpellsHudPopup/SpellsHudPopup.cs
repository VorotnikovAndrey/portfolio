using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.Network;
using Gameplay.Player.Spells;
using PlayVibe.RolePopup;
using UnityEngine;
using Zenject;

namespace PlayVibe.SpellsHudPopup
{
    public class SpellsHudPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;

        [Inject] private Balance balance;
        [Inject] private SpellsSettings spellsSettings;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private GameplayController gameplayController;
        [Inject] private ControlSettings controlSettings;
        
        private CancellationTokenSource cancellationTokenSource;

        private readonly List<SpellContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            Clear();
            cancellationTokenSource = new CancellationTokenSource();
            Create(cancellationTokenSource.Token).Forget();
            Subscribes();
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            UnSubscribes();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        protected void Subscribes()
        {
            eventAggregator.Add<AddSpellEvent>(OnAddSpellEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<AddSpellEvent>(OnAddSpellEvent);
        }
        
        private void OnAddSpellEvent(AddSpellEvent sender)
        {
            var container = containers.FirstOrDefault(x => x.CurrentSpellData == null);

            if (container == null)
            {
                Debug.LogError($"Empty spell container is not founded!".AddColorTag(Color.red));
                return;
            }
            
            container.Setup(spellsSettings.GetByType(sender.Data.SpellType));
        }
        
        private async UniTask Create(CancellationToken token)
        {
            var amount = gameplayStage.LocalGameplayData.RoleType == RoleType.Prisoner
                ? balance.Spells.PrisonerSpellsLimit
                : balance.Spells.SecuritySpellsLimit;

            for (var i = 0; i < amount; i++)
            {
                var container = await objectPoolService.GetOrCreateView<SpellContainer>(Constants.Views.SpellContainer, content);

                if (token.IsCancellationRequested)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }
            
                containers.Add(container);
                diContainer.Inject(container);

                container.Clear();
                container.gameObject.SetActive(true);
                container.SetHotKey(GetHotkeyByIndex(i));
            }
        }

        private KeyCode GetHotkeyByIndex(int index)
        {
            switch (index)
            {
                case 0: return controlSettings.Data[ControlType.Spell1];
                case 1: return controlSettings.Data[ControlType.Spell2];
                case 2: return controlSettings.Data[ControlType.Spell3];
                case 3: return controlSettings.Data[ControlType.Spell4];
            }

            return KeyCode.F12;
        }
        
        private void Clear()
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            containers.ForEach(container => objectPoolService.ReturnToPool(container));
            containers.Clear();
        }
    }
}