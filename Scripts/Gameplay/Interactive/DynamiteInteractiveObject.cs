using System;
using Gameplay.Character;
using Gameplay.Events;
using Photon.Pun;
using UniRx;
using UnityEngine;

namespace PlayVibe.Elements
{
    public sealed class DynamiteInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] private string actionAnimationKey;
        [SerializeField] private float deactivationDuration = 5;
        [SerializeField] private float explosionDuration = 10;

        private CompositeDisposable compositeDisposable;
        
        private void Start()
        {
            if (photonView.IsMine)
            {
                compositeDisposable?.Dispose();
                compositeDisposable = new CompositeDisposable();
                
                Observable.Timer(TimeSpan.FromSeconds(explosionDuration)).Subscribe(_ =>
                {
                    PhotonNetwork.Instantiate(Constants.Resources.VFX.DynamiteExplosionVFX, transform.position + Vector3.up, Quaternion.identity);
                    eventAggregator.SendEvent(new DynamiteExplosionEvent());
                    NetworkDestroy();
                }).AddTo(compositeDisposable);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            compositeDisposable?.Dispose();
        }
        
        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;

            if (!CanInteract(role))
            {
                FailedInteractive(view);
                return;
            }

            view.ActionBar.Show(new CharacterActionData
            {
                AnimationKey = actionAnimationKey,
                Position = view.Center.position,
                Duration = deactivationDuration,
                Action = () =>
                {
                    if (view == null)
                    {
                        return;
                    }
                    
                    SuccessfulInteractive(view);
                }
            });
        }

        private void SuccessfulInteractive(CharacterView view)
        {
            NetworkDestroy();
            
            Debug.Log($"SuccessfulInteractive".AddColorTag(Color.green));
        }

        private void FailedInteractive(CharacterView view)
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
        }
    }
}