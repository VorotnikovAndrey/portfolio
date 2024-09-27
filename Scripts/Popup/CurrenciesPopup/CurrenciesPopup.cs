using System;
using Cysharp.Threading.Tasks;
using Services.Gameplay.Wallet;
using UniRx;
using UnityEngine;
using Zenject;

namespace PlayVibe.CurrenciesPopup
{
    public class CurrenciesPopup : AbstractBasePopup
    {
        [SerializeField] private CurrencyContainer softContainer;

        [Inject] private GameplayStage gameplayStage;
        
        protected override UniTask OnShow(object data = null)
        {
            Subscribes();

            softContainer.Set(gameplayStage.LocalGameplayData.Wallet.GetAmount(CurrencyType.Soft), true);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            UnSubscribes();
            
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
        
        protected void Subscribes()
        {
            gameplayStage.LocalGameplayData.Wallet.HasChanged.Subscribe(OnHasChanged).AddTo(CompositeDisposable);
        }

        protected void UnSubscribes()
        {
            
        }
        
        private void OnHasChanged((CurrencyType type, int amount) value)
        {
            switch (value.type)
            {
                case CurrencyType.Soft:
                    softContainer.Set(value.amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}