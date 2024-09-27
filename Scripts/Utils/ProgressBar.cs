using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Image progressIcon;
        [SerializeField] private TextMeshProUGUI progressText;

        public Image Icon => progressIcon;
        
        public void SetProgress01(float value)
        {
            var value01 = Mathf.Clamp01(value);
            
            progressIcon.fillAmount = value01;
            progressText.text = $"{(int)(value01 * 100f)}%";
        }
    }
}