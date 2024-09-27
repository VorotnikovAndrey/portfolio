using Gameplay.Network;
using Photon.Pun;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public class CharacterMovement : AbstractMovement
    {
        [Inject] private FloorsHandler floorsHandler;
        
        protected override void OnUpdate()
        {
            
        }
        
        public void WarpTo(Vector3 position)
        {
            photonView.RPC("WarpToRPC", RpcTarget.All, position, transform.rotation);
        }
        
        public void WarpTo(Vector3 position, Quaternion rotation)
        {
            photonView.RPC("WarpToRPC", RpcTarget.All, position, rotation);
        }
        
        [PunRPC]
        public void WarpToRPC(Vector3 position, Quaternion rotation)
        {
            characterView.Rigidbody.position = position;
            characterView.Rigidbody.rotation = rotation;

            previousNetworkPosition = position;
            previousNetworkRotation = rotation;
            networkPosition = position;
            networkRotation = rotation;
        }
    }
}