using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Player.SpawnPoint;
using Services.Gameplay.TimeDay;
using Source;
using UnityEngine;
using Zenject;

namespace PlayVibe
{
    public class ActionInteractiveObject : AbstractInteractiveObject
    {
        [SerializeField] protected float duration;
        [SerializeField] protected List<TimeDayState> enableIn;

        [Inject] protected TimeDayService timeDayService;
        [Inject] protected SpawnPointHandler spawnPointHandler;
        
        public override void TryInteractive(CharacterView view)
        {
            var role = gameplayStage.LocalGameplayData.RoleType;
            
            if (!CanInteract(role))
            {
                FailedInteractive(view);
                return;
            }
            
            if (!enableIn.Contains(timeDayService.CurrentState))
            {
                ShowInfoPopup(Constants.Messages.Info.RestroomIsNotAvailable);
                return;
            }

            SuccessfulInteractive(view);
        }
        
        private void SuccessfulInteractive(CharacterView view)
        {
            view.ActionBar.Show(new CharacterActionData
            {
                Position = view.Center.position,
                Duration = duration,
                Action = () =>
                {
                    if (view == null)
                    {
                        return;
                    }
                    
                    var spawnPoint = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.PrisonerTeleport].GetRandom();

                    if (spawnPoint == null)
                    {
                        return;
                    }
                    
                    view.Movement.WarpTo(spawnPoint.Position);
                }
            });
        }

        private void FailedInteractive(CharacterView view)
        {
            Debug.Log($"FailedInteractive".AddColorTag(Color.red));
        }
        
        private void ShowInfoPopup(string message)
        {
            popupService.ShowPopup(new PopupOptions(Constants.Popups.InfoPopup, new InfoPopupData
            {
                Message = message
            }, PopupGroup.System)).Forget();
        }
    }
}