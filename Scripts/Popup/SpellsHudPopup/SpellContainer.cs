using Gameplay.Player.Spells;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.SpellsHudPopup
{
    public class SpellContainer : PoolView
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI hotKeyText;
        [SerializeField] private Image cooldownImage;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Button button;
        [SerializeField] private GameObject lockImage;

        public SpellData CurrentSpellData { get; private set; }
        
        public void Setup(SpellData spellData)
        {
            CurrentSpellData = spellData;

            icon.enabled = true;
            icon.sprite = CurrentSpellData.Icon;
            lockImage.SetActive(false);
            cooldownImage.fillAmount = 0f;
            cooldownText.text = string.Empty;
            button.interactable = true;
        }

        public void SetHotKey(KeyCode keyCode)
        {
            hotKeyText.text = keyCode.ToString();
        }

        public void Clear()
        {
            icon.enabled = false;
            cooldownImage.fillAmount = 0f;
            cooldownText.text = string.Empty;
            button.interactable = false;
            lockImage.SetActive(true);
            CurrentSpellData = null;
        }
    }
}