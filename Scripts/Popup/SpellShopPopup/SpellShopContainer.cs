using System;
using System.Linq;
using Gameplay.Player.Spells;
using PlayVibe.RolePopup;
using Services.Gameplay.Wallet;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PlayVibe
{
    public class SpellShopContainer : PoolView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image icon;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image priceIcon;
        [SerializeField] private TextMeshProUGUI priceText;

        [Inject] private GameplayStage gameplayStage;
        [Inject] private Balance balance;

        private readonly CompositeDisposable compositeDisposable = new();
        private readonly Subject<SpellShopContainer> onClick = new();

        public IObservable<SpellShopContainer> OnClick => onClick;
        public SpellData CurrentSpellData { get; private set; }

        private void Start()
        {
            buyButton.OnClickAsObservable().Subscribe(_ => onClick.OnNext(this)).AddTo(compositeDisposable);
        }

        private void OnDestroy()
        {
            compositeDisposable?.Dispose();
        }
        
        public void Setup(SpellData data)
        {
            CurrentSpellData = data;

            Refresh();
        }

        public void Refresh()
        {
            var localData = gameplayStage.LocalGameplayData;
            var limit = localData.RoleType == RoleType.Prisoner
                ? balance.Spells.PrisonerSpellsLimit
                : balance.Spells.SecuritySpellsLimit;
                
            title.text = CurrentSpellData.SpellType.ToString();
            icon.sprite = CurrentSpellData.Icon;
            priceText.text = $"{CurrentSpellData.Price}$";
            buyButton.interactable =
                localData.Wallet.Has(CurrencyType.Soft, CurrentSpellData.Price) &&
                localData.SpellHandlers.All(x => x.SpellType != CurrentSpellData.SpellType) &&
                localData.SpellHandlers.Count < limit;
        }
    }
}