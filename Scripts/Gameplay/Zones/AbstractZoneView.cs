using Gameplay.Installers;
using Gameplay.Network;
using Photon.Pun;
using PlayVibe;
using UnityEngine;

namespace Gameplay.Player.Zones
{
    public abstract class AbstractZoneView : MonoBehaviourPunCallbacks
    {
        protected GameplayStage gameplayStage;
        protected GameplayController gameplayController;
        protected FloorsHandler floorsHandler;

        public PhotonView PhotonView => photonView;

        private void Awake()
        {
            gameplayStage = GameplayInstaller.DiContainer.Resolve<GameplayStage>();
            gameplayController = GameplayInstaller.DiContainer.Resolve<GameplayController>();
            floorsHandler = GameplayInstaller.DiContainer.Resolve<FloorsHandler>();
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            HandleTrigger(other, true);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            HandleTrigger(other, false);
        }

        protected abstract void HandleTrigger(Collider other, bool isEntering);
        
        public void NetworkDestroy()
        {
            photonView.RPC("NetworkDestroyRPC", RpcTarget.All);
        }
        
        [PunRPC]
        public void NetworkDestroyRPC()
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}