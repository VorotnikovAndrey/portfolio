using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class CharacterAnimationSync : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterMovement movement;
        
        private static readonly int SpeedAnimationKey = Animator.StringToHash("Speed");
        private static readonly int isMoveAnimationKey = Animator.StringToHash("IsMove");
        private static readonly int IsWorkingOnDevice = Animator.StringToHash("WorkingOnDevice");

        public Animator Animator => animator;
        
        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            
            animator.SetFloat(SpeedAnimationKey, movement.LastSpeed);
            animator.SetBool(isMoveAnimationKey, movement.LastSpeed > 0f);
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(movement.LastSpeed);
                stream.SendNext(movement.LastSpeed > 0f);
                stream.SendNext(animator.GetBool(IsWorkingOnDevice));
            }
            else
            {
                animator.SetFloat(SpeedAnimationKey, (float)stream.ReceiveNext());
                animator.SetBool(isMoveAnimationKey, (bool)stream.ReceiveNext());
                animator.SetBool(IsWorkingOnDevice, (bool)stream.ReceiveNext());
            }
        }
    }
}