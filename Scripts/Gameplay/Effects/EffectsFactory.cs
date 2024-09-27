using Photon.Pun;
using Zenject;

namespace Gameplay.Player.Effects
{
    public class EffectsFactory : PlaceholderFactory<EffectModel>
    {
        [Inject] private EffectsSettings effectsSettings;

        public EffectModel Create(EffectType type)
        {
            var settings = effectsSettings.Get(type);

            if (settings == null)
            {
                return null;
            }

            var model = new EffectModel
            {
                EffectType = type,
                EndTime = PhotonNetwork.Time + settings.Duration
            };

            return model;
        }
    }
}