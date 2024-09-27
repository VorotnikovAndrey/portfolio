using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Player.Effects.Events;
using Photon.Pun;
using PlayVibe;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.Effects
{
    public class EffectsHandler : MonoBehaviourPunCallbacks
    {
        [Inject] private EffectsFactory effectsFactory;
        [Inject] private EventAggregator eventAggregator;

        private CharacterView view;

        public Dictionary<EffectType, EffectModel> Data { get; } = new();

        public void Setup(CharacterView view)
        {
            this.view = view;
        }

        public void AddEffect(EffectType effectType)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            var model = effectsFactory.Create(effectType);
            
            if (model == null)
            {
                Debug.LogError($"{typeof(EffectsFactory)} could not create a model for the {effectType.AddColorTag(Color.yellow)}".AddColorTag(Color.red));
                return;
            }
            
            photonView.RPC("AddEffectRPC", RpcTarget.All, SerializationUtils.SerializeObject(model));
        }
        
        public void RemoveEffect(EffectType effectType)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            photonView.RPC("RemoveEffectRPC", RpcTarget.All, effectType);
        }

        public void ClearEffects()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            
            photonView.RPC("ClearEffectsRPC", RpcTarget.All);
        }
        
        [PunRPC]
        public void AddEffectRPC(byte[] data)
        {
            var model = SerializationUtils.DeserializeObject(data) as EffectModel;

            Data.TryGetValue(model.EffectType, out var oldModel);
        
            if (oldModel != null)
            {
                oldModel.EndTime = model.EndTime;
                
                eventAggregator.SendEvent(new UpdateEffectEvent
                {
                    View = view,
                    EffectType = model.EffectType
                });
                
                return;
            }
            
            Data.Add(model.EffectType, model);
            
            eventAggregator.SendEvent(new AddEffectEvent
            {
                View = view,
                EffectType = model.EffectType
            });
        }
        
        [PunRPC]
        public void RemoveEffectRPC(EffectType effectType)
        {
            if (!Data.ContainsKey(effectType))
            {
                return;
            }

            Data.Remove(effectType);
            
            eventAggregator.SendEvent(new RemoveEffectEvent
            {
                View = view,
                EffectType = effectType
            });
        }
        
        [PunRPC]
        public void ClearEffectsRPC()
        {
            Data.Clear();
            
            eventAggregator.SendEvent(new ClearEffectsEvent
            {
                View = view
            });
        }
    }
}