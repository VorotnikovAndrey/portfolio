using System.Globalization;
using Cysharp.Threading.Tasks;
using Gameplay.Character;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class StaminaPopup : AbstractBasePopup
    {
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private GameObject exhaustedImage;
        [SerializeField] private Color exhaustedColor;
        [SerializeField] private Color nonExhaustedColor;
        [SerializeField] private CanvasGroup fillCanvasGroup;
        [SerializeField] private float colorLerpSpeed = 4;
        [SerializeField] private float alphaLerpSpeed = 8;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;
        
        private CharacterView characterView;
        
        protected override UniTask OnShow(object data = null)
        {
            fillCanvasGroup.alpha = 0;
            characterView = gameplayStage.LocalGameplayData.CharacterView as CharacterView;

            if (characterView == null)
            {
                Hide(true).Forget();
                return UniTask.CompletedTask;
            }
            
            Observable.EveryUpdate().Subscribe(_ => UpdateInfo()).AddTo(CompositeDisposable);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        private void UpdateInfo()
        {
            if (characterView == null)
            {
                Hide(true).Forget();
                return;
            }

            var movement = characterView.Movement;
            var staminaHandler = characterView.Movement.StaminaHandler;
            
            exhaustedImage.SetActive(staminaHandler.IsExhausted);
            speedText.text = movement.LastSpeed.ToString("F1", CultureInfo.InvariantCulture);
            fill.fillAmount = staminaHandler.CurrentStamina / balance.Movement.MaxStamina;
            fill.color = Color.Lerp(fill.color, staminaHandler.IsExhausted ? exhaustedColor : nonExhaustedColor, colorLerpSpeed * Time.deltaTime);
            fillCanvasGroup.alpha = Mathf.Lerp(fillCanvasGroup.alpha,(staminaHandler.CurrentStamina < balance.Movement.MaxStamina) ? 1 : 0, alphaLerpSpeed * Time.deltaTime);
        }
    }
}