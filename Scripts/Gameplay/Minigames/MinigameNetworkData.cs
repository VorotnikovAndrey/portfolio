using System;

namespace Gameplay.Player.Minigames
{
    [Serializable]
    public class MinigameNetworkData
    {
        public int Owner;
        public MinigameType Type;
        public int InteractiveNetworkId;
    }
}