using System.Collections.Generic;
using Gameplay.Network.NetworkEventHandlers;
using Photon.Pun;
using PlayVibe;
using Zenject;

namespace Gameplay.Network
{
    public class GameplayController
    {
        private readonly HashSet<AbstractNetworkEventHandler> eventsHandler = new()
        {
            new BalanceNetworkEventHandler(),
            new ViewsNetworkEventHandler(),
            new ItemsNetworkEventHandler(),
            new RolesNetworkEventHandler(),
            new InventoryNetworkEventHandler(),
            new UpgradeNetworkEventHandler(),
            new WalletNetworkEventHandler(),
            new CraftNetworkEventHandler(),
            new MinigamesNetworkEventHandler(),
            new QuestsNetworkEventHandler(),
            new GameplayNetworkEventHandler(),
            new EffectsNetworkEventHandler(),
            new SpellsNetworkEventHandler(),
        };

        private readonly EventAggregator eventAggregator;

        public GameplayController(DiContainer diContainer, EventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                return;
            }
            
            foreach (var handler in eventsHandler)
            {
                diContainer.Inject(handler);
                handler.Initialize(this);
            }

            Subscribes();
        }

        public void Deinitialize()
        {
            UnSubscribes();
            
            foreach (var handler in eventsHandler)
            {
                handler.DeInitialize();
            }
        }
        
        protected void Subscribes()
        {
            eventAggregator.Add<LeaveRoomEvent>(OnLeaveRoomEvent);
        }

        protected void UnSubscribes()
        {
            eventAggregator.Remove<LeaveRoomEvent>(OnLeaveRoomEvent);
        }

        public T GetEventHandler<T>() where T : AbstractNetworkEventHandler
        {
            foreach (var handler in eventsHandler)
            {
                if (handler is T result)
                {
                    return result;
                }
            }

            return null;
        }
        
        private void OnLeaveRoomEvent(LeaveRoomEvent sender)
        {
            Deinitialize();
        }
    }
}