using System.Collections.Generic;
using Gameplay;
using Services.Gameplay.Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class SelfCraftContainer : PoolView
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image result;
        [SerializeField] private GameObject stateImage;
        [SerializeField] private GameObject selectedImage;
        [SerializeField] private Image progressImage;
        [SerializeField] private TextMeshProUGUI progressText;
        [Space]
        [SerializeField] private List<SelfCraftContainerPair> pairs;

        [Inject] private ItemsSettings itemsSettings;
        
        public CraftBank.CraftBankPair CurrentPair { get; private set; }
        public bool State { get; private set; }
        public Button Button => button;

        public void SetState(bool value)
        {
            State = value;
            stateImage.SetActive(!value);
        }
        
        public void SetSelected(bool value)
        {
            selectedImage.SetActive(value);
        }

        public void SetProgress01(float value)
        {
            progressImage.fillAmount = value;
            progressText.text = $"{(int)(value * 100f)}%";
            progressText.gameObject.SetActive(value > 0);
        }
        
        public void Setup(CraftBank.CraftBankPair pair)
        {
            CurrentPair = pair;
            
            result.sprite = itemsSettings.Data[pair.ItemKey].Icon;

            var index = 0;
            
            foreach (var itemKey in pair.ComponentsKeys)
            {
                if (string.IsNullOrEmpty(itemKey))
                {
                    continue;
                }

                var element = pairs[index];

                if (element.Icon != null)
                {
                    element.Icon.gameObject.SetActive(true);
                }
                
                if (element.Text != null)
                {
                    element.Text.gameObject.SetActive(true);
                }
                
                element.Icon.sprite = itemsSettings.Data[itemKey].Icon;

                index++;
            }

            for (var i = index; i < pairs.Count; i++)
            {
                var element = pairs[i];
                
                if (element.Icon != null)
                {
                    element.Icon.gameObject.SetActive(false);
                }
                
                if (element.Text != null)
                {
                    element.Text.gameObject.SetActive(false);
                }
            }
        }
    }
}