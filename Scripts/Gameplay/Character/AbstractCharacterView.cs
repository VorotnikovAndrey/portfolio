using System.Collections.Generic;
using Gameplay.Installers;
using Gameplay.Network;
using Gameplay.Player.Effects;
using Photon.Pun;
using PlayVibe;
using PlayVibe.SpectatorPopup;
using UniRx;
using UnityEngine;
using Zenject;

namespace Gameplay.Character
{
    public abstract class AbstractCharacterView : MonoBehaviourPunCallbacks
    {
        [SerializeField] protected Rigidbody rigidBody;
        [SerializeField] protected CapsuleCollider capsuleCollider;
        [SerializeField] protected Transform center;
        [SerializeField] protected Transform overhead;
        [SerializeField] protected EffectsHandler effectsHandler;

        protected readonly HashSet<string> busyList = new();
        protected readonly CompositeDisposable compositeDisposable = new();

        [Inject] protected GameplayStage gameplayStage;
        [Inject] protected FloorsHandler floorsHandler;
        
        public Rigidbody Rigidbody => rigidBody;
        public CapsuleCollider CapsuleCollider => capsuleCollider;
        public Transform Center => center;
        public Transform Overhead => overhead;
        public EffectsHandler EffectsHandler => effectsHandler;
        public PhotonView PhotonView => photonView;
        public int FloorIndex { get; private set; }
        public bool IsBusy => busyList.Count > 0;

        private void Awake()
        {
            GameplayInstaller.DiContainer.InjectGameObject(gameObject);

            gameplayStage.GameplayDataDic[photonView.Owner.ActorNumber].CharacterView = this;
            
            if (photonView.IsMine)
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            }
            else
            {
                rigidBody.isKinematic = false;
                rigidBody.useGravity = true;
            }
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
        
        public void AddBusy(string value)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            
            photonView.RPC("AddBusyRPC", RpcTarget.All, value);
        }

        public void RemoveBusy(string value)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            
            photonView.RPC("RemoveBusyRPC", RpcTarget.All, value);
        }
        
        [PunRPC]
        public void AddBusyRPC(string value)
        {
            if (!busyList.Add(value))
            {
                return;
            }
        }
        
        [PunRPC]
        public void RemoveBusyRPC(string value)
        {
            if (!busyList.Remove(value))
            {
                return;
            }
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
        
        public void SetFloorIndex(int index)
        {
            photonView.RPC("SetFloorIndexRPC", RpcTarget.All, index);
        }
        
        [PunRPC]
        public void SetFloorIndexRPC(int index)
        {
            FloorIndex = index;

            if (photonView.IsMine || SpectatorPopup.Target == transform)
            {
                floorsHandler.SetFloor(FloorIndex);
            }
        }
    }
}