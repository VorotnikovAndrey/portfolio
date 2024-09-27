using UnityEngine;

namespace PlayVibe
{
    public class TrashCanSlot : MonoBehaviour
    {
        private static readonly int drop = Animator.StringToHash("Drop");
        
        [SerializeField] private Animator animator;

        public void PlayDropAnimation()
        {
            animator.SetTrigger(drop);
        }
    }
}