using Gameplay.Character;
using Gameplay.Network;
using Gameplay.Player.Zones;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.WarpZone
{
    public class WarpZone : AbstractZoneView
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform point;
        [SerializeField] private bool switchFloor;
        [ShowIf("SwitchFloor")] [SerializeField] private int floorIndex;
        
        public bool SwitchFloor => switchFloor;
        public bool IsActive { get; protected set; } = true;

        private void OnValidate()
        {
            if (point != null)
            {
                point.gameObject.name = $"Point: {gameObject.name}";
            }

            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
        }

        protected override void HandleTrigger(Collider other, bool isEntering)
        {
            if (!isEntering || !IsActive)
            {
                return;
            }
            
            var view = other.gameObject.GetComponent<CharacterView>();
            
            if (view != null && view.PhotonView.IsMine)
            {
                view.Movement.WarpTo(point.position);
            
                if (switchFloor)
                {
                    view.SetFloorIndex(floorIndex);
                }
            }
        }
        
        public void SetActiveState(bool value)
        {
            photonView.RPC("SetActiveStateRPC", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void SetActiveStateRPC(bool value)
        {
            IsActive = value;
        }
        
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Selection.activeGameObject == gameObject || Selection.activeGameObject == point.gameObject && point != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, point.position);

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(point.position, 0.2f);
            }
#endif
        }
    }
}