using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Gameplay.Character;
using Gameplay.Network.NetworkData;
using Gameplay.Player.SpawnPoint;
using Photon.Pun;
using Photon.Realtime;
using PlayVibe;
using Services;
using Services.ExtensionsClasses;
using Source;
using UnityEngine;
using Zenject;

namespace Gameplay.Items
{
    public class UseItemBehaviorHandler
    {
        [Inject] private GameplayStage gameplayStage;
        [Inject] private SpawnPointHandler spawnPointHandler;
        
        private readonly Dictionary<string, Action<int>> data;

        private UseItemBehaviorHandler()
        {
            data = new Dictionary<string, Action<int>>
            {
                { "TeleportPotion", TeleportPotionBehavior },
                { "Trap", TrapBehavior },
            };
        }

        public void UseItem(ItemData itemData, int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            data.TryGetValue(itemData.Key, out var action);
            action?.Invoke(actorNumber);
        }
        
        private void TeleportPotionBehavior(int actorNumber)
        {
            var view = gameplayStage.GameplayDataDic[actorNumber].CharacterView as CharacterView;

            if (view == null)
            {
                return;
            }

            var spawnPoint = spawnPointHandler.SpawnPointsDictionary[SpawnPointType.PrisonerTeleport].GetRandom();

            if (spawnPoint == null)
            {
                return;
            }

            PhotonNetwork.Instantiate(Constants.Resources.VFX.TeleportVFX, view.transform.position, Quaternion.identity);
            PhotonNetwork.Instantiate(Constants.Resources.VFX.TeleportVFX, spawnPoint.Position, Quaternion.identity);
            
            view.Movement.WarpTo(spawnPoint.Position);
            view.SetFloorIndex(1);
        }
        
        private void TrapBehavior(int actorNumber)
        {
            var view = gameplayStage.GameplayDataDic[actorNumber].CharacterView as CharacterView;

            if (view == null)
            {
                return;
            }
            
            var eventCode = PhotonPeerEvents.SpawnPhotonView;
            var raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = new[] { actorNumber }
            };
            
            PhotonPeerService.RaiseUniversalEvent(eventCode, new CreateViewNetworkData
            {
                Name = Constants.Resources.Gameplay.TrapInteractiveObject,
                Position = view.transform.position.ToCustomVector3(),
                FloorIndex = view.FloorIndex
            }, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}