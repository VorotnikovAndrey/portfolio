using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Gameplay.Player.Effects;
using PlayVibe;
using UnityEngine;

namespace Gameplay
{
    public class StaminaHandler
    {
        private readonly Balance balance;
        private readonly AbstractCharacterView view;
        private readonly EffectsSettings effectsSettings;

        public bool IsExhausted { get; private set; }
        public float CurrentSpeed { get; private set; }
        public float CurrentStamina { get; private set; }
        public bool IsSprinting { get; private set; }

        public StaminaHandler(
            Balance balance,
            PopupService popupService,
            AbstractCharacterView view,
            EffectsSettings effectsSettings)
        {
            this.effectsSettings = effectsSettings;
            this.view = view;
            this.balance = balance;
            
            CurrentStamina = balance.Movement.MaxStamina;
            CurrentSpeed = balance.Movement.MoveSpeed;

            popupService.ShowPopup(new PopupOptions(Constants.Popups.StaminaPopup)).Forget();
        }

        public void Update(bool isMoved)
        {
            if (Input.GetKey(KeyCode.LeftShift) && CurrentStamina > 0 && !IsExhausted && isMoved)
            {
                IsSprinting = true;
                CurrentStamina -= balance.Movement.StaminaDrainRate * GetStaminaDrainModify() * Time.deltaTime;
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, balance.Movement.MaxSpeed * GetAccelerationModify(), balance.Movement.SpeedAccelerationFactor * Time.deltaTime);

                if (CurrentStamina < 0.1f)
                {
                    IsExhausted = true;
                }
            }
            else
            {
                IsSprinting = false;
                CurrentStamina += balance.Movement.StaminaRegenRate * Time.deltaTime;
                CurrentStamina = Mathf.Clamp(CurrentStamina, 0, balance.Movement.MaxStamina);
                CurrentSpeed = Mathf.Lerp(CurrentSpeed, balance.Movement.MoveSpeed, balance.Movement.SpeedSlowingDownFactor * Time.deltaTime);

                if (CurrentStamina / balance.Movement.MaxStamina > balance.Movement.StaminaExhausted)
                {
                    IsExhausted = false;
                }
            }
        }

        private float GetStaminaDrainModify()
        {
            view.EffectsHandler.Data.TryGetValue(EffectType.StaminaPotion, out var effectModel);
            
            if (effectModel == null)
            {
                return 1f;
            }

            var settings = effectsSettings.Get(EffectType.StaminaPotion) as StaminaPotionEffectData;

            if (settings == null)
            {
                return 1;
            }

            return settings.StaminaDrainReductionMultiplier;
        }
        
        private float GetAccelerationModify()
        {
            view.EffectsHandler.Data.TryGetValue(EffectType.SpeedPotion, out var effectModel);
            
            if (effectModel == null)
            {
                return 1f;
            }

            var settings = effectsSettings.Get(EffectType.SpeedPotion) as SpeedPotionEffectData;

            if (settings == null)
            {
                return 1;
            }

            return settings.AccelerationModifier;
        }
    }
}