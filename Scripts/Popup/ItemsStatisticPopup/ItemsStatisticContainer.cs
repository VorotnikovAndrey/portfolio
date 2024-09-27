using System.Collections.Generic;
using Gameplay;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class ItemsStatisticContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI fillPercent;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform presetsParent;
        [Space] 
        [SerializeField] private Color startColor;
        [SerializeField] private Color finalColor;
        [SerializeField] private float disableAlpha = 0.7f;

        [Inject] private ItemsSettings itemsSettings;
        
        public string ItemKey { get; private set; }
        public int SuccessfulCount { get; private set; }
        public RectTransform PresetsParent => presetsParent;

        public void Initialize(string itemKey)
        {
            var settings = itemsSettings.Data[itemKey];
            
            ItemKey = itemKey;
            icon.sprite = settings.Icon;
            titleText.text = itemKey;
            countText.text = "0";
            fillImage.fillAmount = 0;
            fillPercent.text = "0%";
            canvasGroup.alpha = disableAlpha;
            countText.text = "0";
        }

        public void SetFill(float value)
        {
            value = Mathf.Clamp01(value);
            fillImage.fillAmount = value;
            fillImage.color = Color.Lerp(startColor, finalColor, value);
            fillPercent.text = value > 0 ? $"{value * 100f:0.##}%" : "0%";
        }
        
        public void SetStatistic(StatisticItem statisticItem, HashSet<int> targets)
        {
            SuccessfulCount = 0;

            foreach (var index in targets)
            {
                if (statisticItem.SuccessfulCounts.TryGetValue(index, out var successfulCount))
                {
                    foreach (var pair in successfulCount)
                    {
                        SuccessfulCount += pair.Value;
                    }
                }
            }

            countText.text = SuccessfulCount.ToString();
            canvasGroup.alpha = SuccessfulCount > 0 ? 1f : disableAlpha;
        }

        public void SetIndex(int index)
        {
            indexText.text = $"#{index + 1}";
        }
    }
}