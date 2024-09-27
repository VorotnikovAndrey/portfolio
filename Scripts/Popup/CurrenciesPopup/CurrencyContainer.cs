using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayVibe.CurrenciesPopup
{
    public class CurrencyContainer : MonoBehaviour
    {
        [SerializeField] private Image icon; 
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private ShortNumberToStringConverterScriptableObject converter;
        [SerializeField] private Ease tweenAnimationEase = Ease.OutQuad;
        [SerializeField] private float tweenAnimationDuration = 0.5f;

        private int currentValue;
        private Tweener tweener;
        
        public void Set(int value, bool force = false)
        {
            if (!force)
            {
                tweener?.Kill();
                tweener = DOTween.To(() => currentValue, x => currentValue = x, value, tweenAnimationDuration)
                    .OnUpdate(() => text.text = converter.Convert(currentValue))
                    .SetEase(Ease.OutQuad).OnComplete(() => tweener = null);
            }
            else
            {
                currentValue = value;
                text.text = currentValue.ToString();
            }
        }

        private void OnDestroy()
        {
            tweener?.Kill();
        }
    }
}