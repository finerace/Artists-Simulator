using System;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;

namespace Game.Services.Meta
{
    
    public class CurrenciesService : ICurrenciesService
    {
        private int coins;
        private int crystals;

        public event Action<int> OnCoinsChange;
        public event Action<int> OnCrystalsChange;

        public int Coins => coins;
        public int Crystals => crystals;

        public void Initialize(int coins, int crystals)
        {
            this.coins = coins;
            this.crystals = crystals;
            
            Logs.Info($"Currencies initialized - Coins: {coins}, Crystals: {crystals}");
        }

        /*public void SetCoins(int coins)
        {
            if (coins < 0)
            {
                Logs.Error($"Invalid coins value: {coins}. Cannot be negative.");
                return;
            }

            var oldValue = this.coins;
            this.coins = coins;
            
            Logs.Info($"Coins set from {oldValue} to {coins}");
            OnCoinsChange?.Invoke(this.coins);
        }

        public void SetCrystals(int crystals)
        {
            if (crystals < 0)
            {
                Logs.Error($"Invalid crystals value: {crystals}. Cannot be negative.");
                return;
            }

            var oldValue = this.crystals;
            this.crystals = crystals;
            
            Logs.Info($"Crystals set from {oldValue} to {crystals}");
            OnCrystalsChange?.Invoke(this.crystals);
        }
        */
        
        public void AddCoins(int coins)
        {
            if (coins <= 0)
            {
                Logs.Error($"Invalid coins amount to add: {coins}. Must be positive.");
                return;
            }
            
            var oldValue = this.coins;
            this.coins += coins;
            
            Logs.Info($"Added {coins} coins. Balance: {oldValue} → {this.coins}");
            OnCoinsChange?.Invoke(this.coins);
        }
        
        public void AddCrystals(int crystals) 
        {
            if (crystals <= 0)
            {
                Logs.Error($"Invalid crystals amount to add: {crystals}. Must be positive.");
                return;
            }

            var oldValue = this.crystals;
            this.crystals += crystals;

            Logs.Info($"Added {crystals} crystals. Balance: {oldValue} → {this.crystals}");
            OnCrystalsChange?.Invoke(this.crystals);
        }
        
        public bool TrySpendCoins(int coinsToSpend)
        {
            if (coinsToSpend <= 0)
            {
                Logs.Error($"Invalid coins amount: {coinsToSpend}. Must be positive.");
                return false;
            }
                
            if (this.coins < coinsToSpend)
            {
                Logs.Warning($"Insufficient coins. Current: {this.coins}, Required: {coinsToSpend}");
                return false;
            }

            var oldValue = this.coins;
            this.coins -= coinsToSpend;
            
            Logs.Info($"Spent {coinsToSpend} coins. Balance: {oldValue} → {this.coins}");
            OnCoinsChange?.Invoke(this.coins);
            return true;
        }
        
        public bool TrySpendCrystals(int crystalsToSpend)
        {
            if (crystalsToSpend <= 0)
            {
                Logs.Error($"Invalid crystals amount: {crystalsToSpend}. Must be positive.");
                return false;
            }
                
            if (this.crystals < crystalsToSpend)
            {
                Logs.Warning($"Insufficient crystals. Current: {this.crystals}, Required: {crystalsToSpend}");
                return false;
            }

            var oldValue = this.crystals;
            this.crystals -= crystalsToSpend;
            
            Logs.Info($"Spent {crystalsToSpend} crystals. Balance: {oldValue} → {this.crystals}");
            OnCrystalsChange?.Invoke(this.crystals);
            return true;
        }
        
        public bool TrySpendCoinsAndCrystals(int coinsValue, int crystalsValue)
        {
            if (coins < coinsValue || crystals < crystalsValue)
            {
                Logs.Warning($"Insufficient funds for combined purchase. Coins: {coins}/{coinsValue}, Crystals: {crystals}/{crystalsValue}");
                return false;
            }
            
            var oldCoins = coins;
            var oldCrystals = crystals;
            
            coins -= coinsValue;
            crystals -= crystalsValue;
            
            Logs.Info($"Combined purchase successful. Coins: {oldCoins} → {coins}, Crystals: {oldCrystals} → {crystals}");
            
            OnCoinsChange?.Invoke(coins);
            OnCrystalsChange?.Invoke(crystals);

            return true;
        }
    }
}