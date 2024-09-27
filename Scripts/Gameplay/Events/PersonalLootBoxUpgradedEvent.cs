using Gameplay.Network.NetworkData;
using PlayVibe;

namespace Gameplay.Events
{
    public class PersonalLootBoxUpgradedEvent : AbstractBaseEvent
    {
        public UpgradeLootBoxData Data;
    }
}