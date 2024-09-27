using System.Linq;
using Gameplay.Character;
using Gameplay.Network;
using Gameplay.Player.SpawnPoint;
using PlayVibe;
using PlayVibe.RolePopup;
using PlayVibe.SpectatorPopup;
using Zenject;

namespace Services.Gameplay.Warp
{
    public class WarpService
    {
        [Inject] private GameplayStage gameplayStage;
        [Inject] private SpawnPointHandler spawnPointHandler;
        [Inject] private FloorsHandler floorsHandler;

        public void WarpToHome(int actorNumber)
        {
            var actorData = gameplayStage.GameplayDataDic[actorNumber];
            var index = actorData.CharacterSpawnPointIndex;
            var point = spawnPointHandler.GetRoomDic(actorData.RoleType).FirstOrDefault(x => x.PersonalId == index);
            var view = (CharacterView)actorData.CharacterView;

            if (view == null)
            {
                return;
            }

            var floorIndex = actorData.RoleType == RoleType.Prisoner
                ? floorsHandler.PrisonerHomeFloor
                : floorsHandler.SecurityHomeFloor;
            
            view.SetFloorIndex(floorIndex);
            view.Movement.WarpTo(point.Position);
            
            if (SpectatorPopup.Target == view.transform)
            {
                floorsHandler.SetFloor(floorIndex);
            }
        }
        
        public void WarpToSolitary(int actorNumber, int id)
        {
            var actorData = gameplayStage.GameplayDataDic[actorNumber];
            var point = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.Solitary].FirstOrDefault(x => x.PersonalId == id);
            var view = (CharacterView)actorData.CharacterView;
            
            if (view == null)
            {
                return;
            }

            var floorIndex = floorsHandler.SolitaryFloor;
            
            view.SetFloorIndex(floorIndex);
            view.Movement.WarpTo(point.Position);
            
            if (SpectatorPopup.Target == view.transform)
            {
                floorsHandler.SetFloor(floorIndex);
            }
        }

        public void WarpToSolitaryExit(int actorNumber, int id)
        {
            var actorData = gameplayStage.GameplayDataDic[actorNumber];
            var point = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.SolitaryExit].FirstOrDefault(x => x.PersonalId == id);
            var view = (CharacterView)actorData.CharacterView;
            
            if (view == null)
            {
                return;
            }
            
            var floorIndex = floorsHandler.SolitaryFloor;
            
            view.SetFloorIndex(floorIndex);
            view.Movement.WarpTo(point.Position);
            
            if (SpectatorPopup.Target == view.transform)
            {
                floorsHandler.SetFloor(floorIndex);
            }
        }
    }
}