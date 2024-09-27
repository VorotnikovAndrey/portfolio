using System;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace Gameplay.Player.VFX
{
    public class NetworkVFX : MonoBehaviourPunCallbacks
    {
        [SerializeField] private float lifeTime;

        private readonly CompositeDisposable compositeDisposable = new();
        
        private void Start()
        {
            Observable.Timer(TimeSpan.FromSeconds(lifeTime)).Subscribe(_ =>
            {
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
    }
}