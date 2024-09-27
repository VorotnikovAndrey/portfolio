using Gameplay.Character;
using PlayVibe;

namespace Gameplay.Player.Effects.Events
{
    public abstract class AbstractEffectEvent : AbstractBaseEvent
    {
        public CharacterView View;
    }
}