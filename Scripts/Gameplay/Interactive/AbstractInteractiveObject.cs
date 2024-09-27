using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Events;
using Gameplay.Installers;
using Gameplay.Network;
using Photon.Pun;
using PlayVibe.RolePopup;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public abstract class AbstractInteractiveObject : MonoBehaviourPunCallbacks, HColor
    {
        [SerializeField] [HideInInspector] protected HColorData hColorData;

        [ReadOnly] [SerializeField] protected int networkKey;
        [SerializeField] protected InteractiveOutline interactiveOutline;
        [SerializeField] protected List<RoleType> canInteract;
        [SerializeField] protected Collider interactiveCollider;
        
        [Inject] protected GameplayStage gameplayStage;
        [Inject] protected GameplayController gameplayController;
        [Inject] protected PopupService popupService;
        [Inject] protected EventAggregator eventAggregator;
        [Inject] protected FloorsHandler floorsHandler;
        
        public HColorData HColorData => hColorData;
        public Collider InteractiveCollider => interactiveCollider;
        public virtual int NetworkKey => networkKey;
        public PhotonView PhotonView => photonView;

        private void OnValidate()
        {
            if (hColorData != null)
            {
                hColorData.TextColor = Color.cyan;
            }
            
            interactiveOutline = GetComponentInChildren<InteractiveOutline>();
        }

        protected virtual void Awake()
        {
            GameplayInstaller.DiContainer.InjectGameObject(gameObject);

            SetOutlineState(false);
            Subscribes();
        }

        protected virtual void OnDestroy()
        {
            UnSubscribes();
        }

        protected virtual void Subscribes()
        {
            eventAggregator.Add<ArrestLocalPrisoner>(OnArrestLocalPrisoner);
        }

        protected virtual void UnSubscribes()
        {
            eventAggregator.Remove<ArrestLocalPrisoner>(OnArrestLocalPrisoner);
        }

        public virtual void SetNetworkKey(int value) => networkKey = value;

        public abstract void TryInteractive(CharacterView view);

        public virtual bool CanInteract(RoleType roleType)
        {
            return canInteract.Contains(roleType);
        }

        public virtual void SetOutlineState(bool value)
        {
            if (interactiveOutline == null)
            {
                return;
            }
            
            interactiveOutline.enabled = value;
        }
        
        private void OnArrestLocalPrisoner(ArrestLocalPrisoner sender)
        {
            SetOutlineState(false);
        }
        
        public void ChangeFloor(int index)
        {
            photonView.RPC("ChangeFloorRPC", RpcTarget.All, index);
        }
        
        [PunRPC]
        public void ChangeFloorRPC(int index)
        {
            transform.SetParent(floorsHandler.GetFloorParent(index).transform);
        }
        
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