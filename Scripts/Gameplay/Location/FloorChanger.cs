using Gameplay.Installers;
using Photon.Pun;

namespace Gameplay.Network
{
    public class FloorChanger : MonoBehaviourPunCallbacks
    {
        public void ChangeFloor(int index)
        {
            photonView.RPC("ChangeFloorRPC", RpcTarget.All, index);
        }
        
        [PunRPC]
        public void ChangeFloorRPC(int index)
        {
            transform.SetParent(GameplayInstaller.DiContainer.Resolve<FloorsHandler>().GetFloorParent(index).transform);
        }
    }
}