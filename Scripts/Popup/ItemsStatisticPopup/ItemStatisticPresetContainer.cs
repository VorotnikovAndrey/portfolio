using System.Collections.Generic;
using Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe
{
    public class ItemStatisticPresetContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI indexText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI fillPercent;
        [SerializeField] private TextMeshProUGUI countText;
        [Space]
        [SerializeField] private Color startColor;
        [SerializeField] private Color finalColor;

        public int SuccessfulCount { get; private set; }
        
        public void Initialize(int index, string presetName, StatisticItem statisticItem, HashSet<int> targets)
        {
            indexText.text = $"{index}.";
            titleText.text = presetName;

            SuccessfulCount = 0;

            SetFill(0);

            foreach (var target in targets)
            {
                statisticItem.SuccessfulCounts.TryGetValue(target, out var successfulData);

                if (successfulData != null)
                {
                    successfulData.TryGetValue(presetName, out var successfulCount);
                    
                    SuccessfulCount += successfulCount;
                }
            }
            
            countText.text = SuccessfulCount.ToString();
        }
        
        public void SetFill(float value)
        {
            value = Mathf.Clamp01(value);
            fillImage.fillAmount = value;
            fillImage.color = Color.Lerp(startColor, finalColor, value);
            fillPercent.text = value > 0 ? $"{value * 100f:0.##}%" : "0%";
        }
    }
}