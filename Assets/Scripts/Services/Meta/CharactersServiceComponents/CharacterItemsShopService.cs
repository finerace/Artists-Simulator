using System;
using System.Collections.Generic;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using UnityEngine;
using YG;

namespace Game.Services.Meta
{
    
    public class CharacterItemsShopService : ICharacterItemsShopService
    {
        private readonly ICurrenciesService currenciesService;
        private readonly ICharactersServiceFacade charactersService;

        private List<SavesYG.ItemSaveData> unlockedItems;
        private CharacterCustomizationView targetPlayerCharacter;
        
        public event Action<SavesYG.ItemSaveData> OnNewItemUnlocked;
        public event Action<string, Color> OnItemColorChanged;
        public event Action<List<SavesYG.CharacterSlotData>> OnPlayerCharacterAppearanceChanged;
        
        public CharacterItemsShopService(ICurrenciesService currenciesService, ICharactersServiceFacade charactersService)
        {
            this.currenciesService = currenciesService;
            this.charactersService = charactersService;
            
            this.charactersService.OnCharacterGenderSwapped += OnCharacterGenderSwapped;
        }

        public void Initialize(List<SavesYG.ItemSaveData> unlockedItems)
        {
            this.unlockedItems = unlockedItems;
            InitializeAlwaysUnlockedItems();
            Logs.Debug($"CharacterItemsShop initialized with {unlockedItems.Count} items");
        }

        private void InitializeAlwaysUnlockedItems()
        {
            var allItems = ConfigsProxy.CharactersAndShopConfig.ItemsDatabase.Values;
            
            foreach (var item in allItems)
            {
                if (!item.ItemIsAlwaysUnlocked || item == null)
                    continue;
                    
                var existingItem = unlockedItems.Find(i => i.ItemId == item.ItemId);
                if (existingItem == null)
                {
                    unlockedItems.Add(new SavesYG.ItemSaveData(item.ItemId, item.DefaultColor));
                }
            }
        }
        
        public void SetPlayerCharacter(CharacterCustomizationView playerCharacter)
        {
            if (targetPlayerCharacter == playerCharacter)
                return;
                
            if (targetPlayerCharacter != null)
            {
                targetPlayerCharacter.OnSlotItemChanged -= OnPlayerCharacterSlotChanged;
            }
            
            targetPlayerCharacter = playerCharacter;
            
            if (targetPlayerCharacter != null)
            {
                targetPlayerCharacter.OnSlotItemChanged += OnPlayerCharacterSlotChanged;
            }
            
            Logs.Debug($"Set player character: {playerCharacter?.name}");
        }
        
        private void OnPlayerCharacterSlotChanged(string slotId, string itemId, Color itemColor)
        {
            var slots = targetPlayerCharacter.GetSlotsData();
            bool isValidChange = true;
            
            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(slot.ItemId))
                    continue;
                
                var itemData = ConfigsProxy.CharactersAndShopConfig.GetItemById(slot.ItemId);
                
                if (!IsItemUnlocked(itemData))
                {
                    isValidChange = false;
                    break;
                }
            }
            
            if (isValidChange)
            {
                OnPlayerCharacterAppearanceChanged?.Invoke(slots);
            }
        }
        
        private void OnCharacterGenderSwapped(string characterId, CharacterCustomizationView newCharacter)
        {
            if (characterId == ConfigsProxy.CharactersAndShopConfig.MainCharacterId)
            {
                SetPlayerCharacter(newCharacter);
            }
        }
        
        public bool TryToUnlockItem(CharacterItemData characterItemData)
        {
            if (characterItemData == null)
            {
                Logs.Info("Cannot unlock null item");
                return false;
            }
                
            if (IsItemUnlocked(characterItemData))
            {
                Logs.Info($"Item {characterItemData.ItemId} already unlocked");
                return true;
            }
                
            var spendResult = currenciesService.TrySpendCoinsAndCrystals(
                characterItemData.CoinsPrice, 
                characterItemData.CrystalsPrice
            );

            if (!spendResult)
            {
                Logs.Info($"Insufficient funds for item {characterItemData.ItemId}");
                return false;
            }
            
            var itemSaveData = new SavesYG.ItemSaveData(characterItemData.ItemId, characterItemData.DefaultColor);
            
            unlockedItems.Add(itemSaveData);
            
            Logs.Info($"Unlocked item {characterItemData.ItemId} for {characterItemData.CoinsPrice}c/{characterItemData.CrystalsPrice}g");
            OnNewItemUnlocked?.Invoke(itemSaveData);
            
            return true;
        }
        
        public bool TryToSetColor(CharacterItemData characterItemData, Color newColor)
        {
            if (characterItemData == null || !characterItemData.IsCanColorize)
            {
                Logs.Info($"Cannot colorize item {characterItemData?.ItemId}");
                return false;
            }
                
            if (!IsItemUnlocked(characterItemData))
            {
                Logs.Info($"Item {characterItemData.ItemId} not unlocked for coloring");
                return false;
            }
                
            var spendResult = currenciesService.TrySpendCoins(characterItemData.ColorizeCoinsPrice);
            
            if (!spendResult)
            {
                Logs.Info($"Insufficient coins for coloring {characterItemData.ItemId}");
                return false;
            }
            
            SetItemColor(characterItemData.ItemId, newColor);
            
            Logs.Info($"Colored item {characterItemData.ItemId} for {characterItemData.ColorizeCoinsPrice}c");
            OnItemColorChanged?.Invoke(characterItemData.ItemId, newColor);
            
            return true;
        }
        
        public bool IsItemUnlocked(CharacterItemData itemData)
        {
            if (itemData == null)
                return false;
                
            if (itemData.ItemIsAlwaysUnlocked || itemData.IsItemsPack)
                return true;
                
            foreach (var item in unlockedItems)
            {
                if (item.ItemId == itemData.ItemId)
                    return true;
            }
            
            return false;
        }
        
        public bool IsItemUnlocked(string itemId)
        {
            var itemData = ConfigsProxy.CharactersAndShopConfig.GetItemById(itemId);
            
            if (itemData == null)
                return false;
                
            if (itemData.ItemIsAlwaysUnlocked || itemData.IsItemsPack)
                return true;
                
            foreach (var item in unlockedItems)
            {
                if (item.ItemId == itemData.ItemId)
                    return true;
            }
            
            return false;
        }
        
        public void SetItemColor(string itemId, Color newItemColor)
        {
            if (string.IsNullOrEmpty(itemId))
                return;
            
            foreach (var item in unlockedItems)
            {
                if (item.ItemId == itemId)
                {
                    item.SetColor(newItemColor);
                    break;
                }
            }
        }

        public Color GetItemColor(string itemId)
        {
            foreach (var item in unlockedItems)
            {
                if (item.ItemId == itemId)
                    return item.ItemColor;
            }
            
            throw new ArgumentException($"Item with id {itemId} is not found!");
        }
    }
}