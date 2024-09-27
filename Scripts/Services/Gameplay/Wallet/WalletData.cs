using System;

namespace Services.Gameplay.Wallet
{
    [Serializable]
    public class WalletData
    {
        public int ActorNumber;
        public CurrencyType CurrencyType;
        public int Amount;
    }
}