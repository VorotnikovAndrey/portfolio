using Gameplay.Character;
using PlayVibe.RolePopup;
using Services.Gameplay.TimeDay;
using UniRx;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.Zones
{
    public class PrisonerHomeZone : AbstractZoneView
    {
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private Transform point;
        
        [Inject] private TimeDayService timeDayService;

        private CompositeDisposable compositeDisposable = new();

        private void Start()
        {
            timeDayService.EmitStateChanged.Subscribe(_ => ExpelIntruders()).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
            compositeDisposable = null;
        }

        protected override void HandleTrigger(Collider other, bool isEntering)
        {
            if (!isEntering)
            {
                return;
            }

            if (timeDayService.CurrentState == TimeDayState.Day)
            {
                return;
            }

            var view = other.GetComponent<CharacterView>();

            if (view == null)
            {
               return; 
            }

            var data = gameplayStage.GameplayDataDic[view.PhotonView.OwnerActorNr];

            if (data == null)
            {
                return;
            }

            var role = data.RoleType;

            if (role != RoleType.Security)
            {
                return;
            }
            
            view.Movement.WarpTo(point.position);
        }

        private void ExpelIntruders()
        {
            if (timeDayService.CurrentState == TimeDayState.Day)
            {
                return;
            }
            
            var center = boxCollider.transform.TransformPoint(boxCollider.center);
            var halfExtents = boxCollider.size / 2;
            var colliders = Physics.OverlapBox(center, halfExtents, boxCollider.transform.rotation);

            foreach (var element in colliders)
            {
                var view = element.GetComponent<CharacterView>();
                
                if (view != null && gameplayStage.GameplayDataDic[view.PhotonView.OwnerActorNr].RoleType == RoleType.Security)
                {
                    view.Movement.WarpTo(point.position);
                }
            }
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