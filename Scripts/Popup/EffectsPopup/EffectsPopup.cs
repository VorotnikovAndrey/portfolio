using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Player.Effects;
using Gameplay.Player.Effects.Events;
using UnityEngine;
using Zenject;

namespace PlayVibe.EffectsPopup
{
    public class EffectsPopup : AbstractBasePopup
    {
        [SerializeField] private RectTransform content;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private EffectsSettings effectsSettings;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly List<EffectContainer> containers = new();
        
        protected override UniTask OnShow(object data = null)
        {
            eventAggregator.Add<AddEffectEvent>(OnAddEffectEvent);
            eventAggregator.Add<RemoveEffectEvent>(OnRemoveEffectEvent);
            eventAggregator.Add<ClearEffectsEvent>(OnClearEffectsEvent);
            eventAggregator.Add<UpdateEffectEvent>(OnUpdateEffectEvent);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            eventAggregator.Remove<AddEffectEvent>(OnAddEffectEvent);
            eventAggregator.Remove<RemoveEffectEvent>(OnRemoveEffectEvent);
            eventAggregator.Remove<ClearEffectsEvent>(OnClearEffectsEvent);
            eventAggregator.Remove<UpdateEffectEvent>(OnUpdateEffectEvent);
            
            cancellationTokenSource?.Dispose();

            Clear();

            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }

        private async void OnAddEffectEvent(AddEffectEvent sender)
        {
            if (sender.View.PhotonView.Owner.ActorNumber != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            var effectType = sender.EffectType;
            var settings = effectsSettings.Get(effectType);

            if (settings == null)
            {
                Debug.LogError($"EffectSettings for {effectType.AddColorTag(Color.yellow)} is not found!".AddColorTag(Color.red));
                return;
            }
            
            var container = await objectPoolService.GetOrCreateView<EffectContainer>(Constants.Views.EffectContainer, content);

            if (cancellationTokenSource.Token.IsCancellationRequested)
            {
                objectPoolService.ReturnToPool(container);
                return;
            }
            
            containers.Add(container);
            diContainer.InjectGameObject(container.gameObject);

            gameplayStage.LocalGameplayData.CharacterView.EffectsHandler.Data.TryGetValue(effectType, out var effectModel);

            if (effectModel == null)
            {
                objectPoolService.ReturnToPool(container);
                return;
            }
                
            container.Setup(settings, effectModel);
            container.gameObject.SetActive(true);
        }

        private void OnRemoveEffectEvent(RemoveEffectEvent sender)
        {
            if (sender.View.PhotonView.Owner.ActorNumber != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            foreach (var container in containers)
            {
                if (container.EffectModel.EffectType != sender.EffectType)
                {
                    continue;
                }
                
                objectPoolService.ReturnToPool(container);
                return;
            }
        }

        private void OnClearEffectsEvent(ClearEffectsEvent sender)
        {
            if (sender.View.PhotonView.Owner.ActorNumber != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            Clear();
        }

        private void OnUpdateEffectEvent(UpdateEffectEvent sender)
        {
            if (sender.View.PhotonView.Owner.ActorNumber != gameplayStage.LocalGameplayData.ActorNumber)
            {
                return;
            }
            
            var effectType = sender.EffectType;
            
            foreach (var container in containers)
            {
                if (container.EffectModel.EffectType != sender.EffectType)
                {
                    continue;
                }
                
                gameplayStage.LocalGameplayData.CharacterView.EffectsHandler.Data.TryGetValue(effectType, out var effectModel);

                if (effectModel == null)
                {
                    objectPoolService.ReturnToPool(container);
                    return;
                }

                container.OverrideTime(effectModel.EndTime);
                
                return;
            }
        }

        private void Clear()
        {
            foreach (var container in containers)
            {
                objectPoolService.ReturnToPool(container);
            }
            
            containers.Clear();
        }
    }
}