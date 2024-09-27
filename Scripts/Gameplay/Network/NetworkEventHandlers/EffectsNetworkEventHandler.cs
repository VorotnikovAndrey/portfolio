using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.Network.NetworkData;
using Gameplay.Player.Effects;
using Photon.Pun;
using Services;
using UniRx;
using Zenject;

namespace Gameplay.Network.NetworkEventHandlers
{
    public class EffectsNetworkEventHandler : AbstractNetworkEventHandler
    {
        private const float CheckEffectTimeStep = 0.5f;
        
        [Inject] private EffectsSettings effectsSettings;
        
        protected override void OnInitialized()
        {
            events[PhotonPeerEvents.AddEffect] = ReceiveAddEffect;
            events[PhotonPeerEvents.RemoveEffect] = ReceiveRemoveEffect;
            events[PhotonPeerEvents.RemoveAllEffects] = ReceiveRemoveAllEffects;

            if (PhotonNetwork.IsMasterClient)
            {
                Observable.Interval(TimeSpan.FromSeconds(CheckEffectTimeStep)).Subscribe(_ => CheckEffects()).AddTo(CompositeDisposable);
            }
        }

        protected override void OnSubscribes()
        {
            
        }

        protected override void OnUnSubscribes()
        {
            
        }
        
        private void CheckEffects()
        {
            foreach (var data in gameplayStage.GameplayDataDic)
            {
                var view = data.Value.CharacterView;

                if (view?.EffectsHandler == null)
                {
                    continue;
                }

                var handler = view.EffectsHandler;
                var effectsToRemove = handler.Data
                    .Where(effect => PhotonNetwork.Time > effect.Value.EndTime)
                    .Select(effect => effect.Key)
                    .ToList();

                foreach (var effect in effectsToRemove)
                {
                    handler.RemoveEffect(effect);
                }
            }
        }
        
        private void ReceiveAddEffect(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not EffectNetworkData data)
            {
                return;
            }
            
            gameplayStage.GameplayDataDic[data.Target].CharacterView.EffectsHandler.AddEffect(data.EffectType);
        }

        private void ReceiveRemoveEffect(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not EffectNetworkData data)
            {
                return;
            }
            
            gameplayStage.GameplayDataDic[data.Target].CharacterView.EffectsHandler.RemoveEffect(data.EffectType);
        }
        
        private void ReceiveRemoveAllEffects(PhotonPeerData peerData)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            if (peerData.CustomData is not EffectNetworkData data)
            {
                return;
            }
            
            gameplayStage.GameplayDataDic[data.Target].CharacterView.EffectsHandler.ClearEffects();
        }
    }
}