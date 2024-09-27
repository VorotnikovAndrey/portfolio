using Gameplay.Player.Minigames;
using Gameplay.Player.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.QuestsIndicatorPopup
{
    public class QuestsIndicatorView : PoolView
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI distanceText;
        
        [Inject] private MinigamesHandler minigamesHandler;
        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;

        private Transform target;
        private Camera locationCamera;
        private float screenOffset;
        
        public QuestData Data { get; private set; }
        
        public void Setup(QuestData data)
        {
            Data = data;

            if (Data == null)
            {
                return;
            }

            screenOffset = balance.Interactive.IndicatorScreenOffsetFactor;
            locationCamera = gameplayStage.LocalGameplayData.LocationCamera.Camera;
            target = minigamesHandler.Data[Data.TargetNetworkId].transform;
        }

        private void LateUpdate()
        {
            if (Data == null || target == null)
            {
                return;
            }

            var targetPosition = target.position;
            var screenPos = locationCamera.WorldToScreenPoint(targetPosition);
            var isTargetOnScreen = screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height;

            if (isTargetOnScreen)
            {
                icon.enabled = false;
                distanceText.enabled = false;
            }
            else
            {
                icon.enabled = true;
                distanceText.enabled = true;
                
                if (screenPos.z < 0)
                {
                    screenPos *= -1;
                }
                
                var screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;

                screenPos -= screenCenter;

                var angle = Mathf.Atan2(screenPos.y, screenPos.x);
                angle -= 90 * Mathf.Deg2Rad;

                var cos = Mathf.Cos(angle);
                var sin = -Mathf.Sin(angle);
                var m = cos / sin;

                var screenBounds = screenCenter * screenOffset;

                screenPos = cos > 0 ? new Vector3(screenBounds.y / m, screenBounds.y, 0) : new Vector3(-screenBounds.y / m, -screenBounds.y, 0);

                if (screenPos.x > screenBounds.x)
                {
                    screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
                }
                else if (screenPos.x < -screenBounds.x)
                {
                    screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
                }

                screenPos += screenCenter;

                rectTransform.position = screenPos;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

                distanceText.text = $"{(int)Vector3.Distance(targetPosition, gameplayStage.LocalGameplayData.CharacterView.Center.position)}m";
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();

            Data = null;
        }
    }
}