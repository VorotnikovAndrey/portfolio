using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class TradeOtherSlot : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject prohibitedIcon;
        [SerializeField] private TextMeshProUGUI networkIdText;

        [Inject] private ItemsSettings itemsSettings;
        
        public int Index => index;

        public void Setup(ItemModel model)
        {
            if (model == null)
            {
                return;
            }
            
            itemsSettings.Data.TryGetValue(model.ItemKey, out var data);

            if (data == null)
            {
                return;
            }

            icon.gameObject.SetActive(true);
            icon.sprite = data.Icon;
            prohibitedIcon.SetActive(data.Classification == ItemClassification.Prohibited);
            networkIdText.text = $"id: { model.NetworkId}";
        }

        public void Clear()
        {
            icon.sprite = null;
            icon.gameObject.SetActive(false);
            prohibitedIcon.SetActive(false);
            networkIdText.text = string.Empty;
        }
    }
}