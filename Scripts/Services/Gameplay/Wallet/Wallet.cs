using System;
using System.Collections.Generic;
using UniRx;

namespace Services.Gameplay.Wallet
{
    [Serializable]
    public class Wallet
    {
        private readonly Subject<(CurrencyType, int)> hasChanged = new();
        private readonly Dictionary<CurrencyType, int> currencies;
        
        public IObservable<(CurrencyType, int)> HasChanged => hasChanged;

        public Wallet(Dictionary<CurrencyType, int> data)
        {
            currencies = data;
        }

        public int GetAmount(CurrencyType type)
        {
            return currencies[type];
        }

        public void Modify(CurrencyType type, int value)
        {
            currencies[type] = Math.Clamp(currencies[type] + value, 0, int.MaxValue);
            hasChanged.OnNext((type, currencies[type]));
        }

        public void Clear(CurrencyType type)
        {
            currencies[type] = 0;
            
            hasChanged.OnNext((type, currencies[type]));
        }
        
        public void ClearAll()
        {
            foreach (var key in new List<CurrencyType>(currencies.Keys))
            {
                currencies[key] = 0;
                hasChanged.OnNext((key, currencies[key]));
            }
        }

        public bool Has(CurrencyType type, int value)
        {
            return GetAmount(type) >= value;
        }
    }
}