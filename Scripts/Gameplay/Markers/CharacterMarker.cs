using System.Collections.Generic;
using Photon.Pun;
using PlayVibe;
using PlayVibe.RolePopup;
using TMPro;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.Markers
{
    public class CharacterMarker : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private MarkersColors markersColors;

        [Inject] private GameplayStage gameplayStage;
        
        public MarkerType CurrentMarker { get; protected set; }

        public void UpdateColor(HashSet<MarkerType> markers)
        {
            Color color;
            
            if (markers.Contains(MarkerType.Wanted))
            {
                color = markersColors.Get(MarkerType.Wanted);
                CurrentMarker = MarkerType.Wanted;
            }
            else if (markers.Contains(MarkerType.Night))
            {
                color = markersColors.Get(MarkerType.Night);
                CurrentMarker = MarkerType.Night;
            }
            else if (markers.Contains(MarkerType.Violator))
            {
                color = markersColors.Get(MarkerType.Violator);
                CurrentMarker = MarkerType.Violator;
            }
            else if (markers.Contains(MarkerType.Smuggler))
            {
                color = markersColors.Get(MarkerType.Smuggler);
                CurrentMarker = MarkerType.Smuggler;
            }
            else
            {
                var marker = gameplayStage.GameplayDataDic[photonView.Owner.ActorNumber].RoleType == RoleType.Prisoner
                    ? MarkerType.Prisoner
                    : MarkerType.Security;
                
                CurrentMarker = marker;
                
                color = markersColors.Get(marker);
            }
            
            nicknameText.color = color;
        }
    }
}