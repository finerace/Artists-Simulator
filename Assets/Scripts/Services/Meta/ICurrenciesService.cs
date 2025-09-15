using System;

namespace Game.Services.Meta
{
    public interface ICurrenciesService
    {
        event Action<int> OnCoinsChange;
        event Action<int> OnCrystalsChange;

        int Coins { get; }
        int Crystals { get; }

        void Initialize(int coins, int crystals);
        void AddCoins(int coins);
        void AddCrystals(int crystals);
        bool TrySpendCoins(int coinsToSpend);
        bool TrySpendCrystals(int crystalsToSpend);
        bool TrySpendCoinsAndCrystals(int coinsValue, int crystalsValue);
    }
} 