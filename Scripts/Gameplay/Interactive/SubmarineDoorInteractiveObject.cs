using Gameplay.Character;
using UnityEngine;

namespace PlayVibe.Elements
{
    public class SubmarineDoorInteractiveObject : NeedItemInteractiveObject
    {
        [SerializeField] private SubmarineInteractiveObject submarineInteractiveObject;
        
        public override void TryInteractive(CharacterView view)
        {
            if (!submarineInteractiveObject.Locked)
            {
                FailedInteractive(view);
                return;
            }

            base.TryInteractive(view);
        }
        
        protected override void SuccessfulInteractive(CharacterView view)
        {
            base.SuccessfulInteractive(view);
            
            submarineInteractiveObject.SetLockedState(false);
        }
    }
}