using Gameplay.Player.Minigames;
using Gameplay.Player.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe.QuestsPopup
{
    public class QuestContainer : PoolView
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image icon;
        [SerializeField] private Image border;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI networkIdText;

        [Inject] private MinigamesSettings minigamesSettings;
        [Inject] private MinigamesHandler minigamesHandler;
        
        public RectTransform RectTransform => rectTransform;
        public QuestData QuestData { get; private set; }

        public void Setup(QuestData questData)
        {
            QuestData = questData;

            var interactiveObject = minigamesHandler.GetMinigameWithNetworkId(questData.TargetNetworkId);

            if (interactiveObject == null)
            {
                return;
            }

            icon.sprite = interactiveObject.QuestIcon;
            border.color = minigamesSettings.GetQuestColor(questData.Difficulty);
            descriptionText.text = interactiveObject.Description;
            networkIdText.text = $"id:{interactiveObject.NetworkKey}";
        }
    }
}