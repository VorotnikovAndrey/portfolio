using Photon.Pun;
using PlayVibe;
using PlayVibe.RolePopup;
using UnityEngine;

namespace Gameplay.Character
{
    public sealed class InteractiveOutlineController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private LayerMask layerMask;
        
        private RoleType roleType;
        
        public void Setup(RoleType roleType)
        {
            this.roleType = roleType;
            
            var colliders = Physics.OverlapSphere(transform.position, sphereCollider.radius, layerMask, QueryTriggerInteraction.Collide);

            foreach (var element in colliders)
            {
                HandleTrigger(element, true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTrigger(other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            HandleTrigger(other, false);
        }

        private void HandleTrigger(Component other, bool isEntering)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            var interactive = other.GetComponent<AbstractInteractiveObject>();

            if (interactive == null)
            {
                return;
            }

            interactive.SetOutlineState(interactive.CanInteract(roleType) && isEntering);
        }
    }
}