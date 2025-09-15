using System;
using System.Collections.Generic;
using Game.Infrastructure.Configs;
using UnityEngine;
using Color = UnityEngine.Color;

namespace YG
{
    public static class YandexGame
    {
        private static SavesYG _savesData;
        private static EnvironmentData _environmentData;
        
        public static SavesYG savesData
        {
            get
            {
                if (_savesData == null)
                {
                    _savesData = new SavesYG();
                }
                return _savesData;
            }
        }
        
        public static EnvironmentData EnvironmentData
        {
            get
            {
                if (_environmentData == null)
                {
                    _environmentData = new EnvironmentData();
                }
                return _environmentData;
            }
        }
        
        public static bool SDKEnabled => true;
        
        public static void SaveProgress()
        {
            // Implementation for cloud save if needed
            Debug.Log("SaveProgress called");
        }
    }
    
    [Serializable]
    public class EnvironmentData
    {
        public string language = "ru";
    }

    [Serializable]
    public class SavesYG
    {
        [SerializeField] public bool isFirstSession = true;
        [SerializeField] public string language = "en";
        
        // Настройки игры
        [SerializeField] private bool isSoundEnabled = true;
        [SerializeField] private bool isMusicEnabled = true;
        [SerializeField] private int graphicsQuality = 1; // 0 - низкое, 1 - среднее, 2 - высокое

        [SerializeField] private int coins;
        [SerializeField] private int crystals;
        [SerializeField] private int playerExperience;
        
        [SerializeField] private string currentCharacterTemplateId;
        
        [SerializeField] private List<ItemSaveData> unlockedItemsData;
        
        [SerializeField] private List<CharacterSlotData> characterSlots = new List<CharacterSlotData>();
        
        [SerializeField] private List<string> unlockedLocationImprovements;

        public int Coins => coins;
        public int Crystals => crystals;
        public int PlayerExperience => playerExperience;
        
        public bool IsSoundEnabled => isSoundEnabled;
        public bool IsMusicEnabled => isMusicEnabled;
        public int GraphicsQuality => graphicsQuality;

        public string CurrentCharacterTemplateId => currentCharacterTemplateId;

        public List<ItemSaveData> UnlockedItemsData => unlockedItemsData;
        public List<CharacterSlotData> CharacterSlots => characterSlots;
        public List<string> UnlockedLocationImprovements => unlockedLocationImprovements;

        public SavesYG()
        {
            unlockedItemsData = new List<ItemSaveData>();
            characterSlots = new List<CharacterSlotData>();
            unlockedLocationImprovements = new List<string>();
            isFirstSession = true;
            isSoundEnabled = true;
            isMusicEnabled = true;
            graphicsQuality = 1;
        }
        
        public void Initialize()
        {
            if(isFirstSession)
            {
                coins = ConfigsProxy.EconomicConfig.StartMoney;
                crystals = ConfigsProxy.EconomicConfig.StartCrystals;
                playerExperience = 0;
                currentCharacterTemplateId = ConfigsProxy.CharactersAndShopConfig.MaleTemplateId;
                isFirstSession = false;
            }
        }

        public void SetCurrencies(int coins,int crystals)
        {
            this.coins = coins;
            this.crystals = crystals;
        }
        
        public void SetPlayerExperience(int experience)
        {
            this.playerExperience = experience;
        }
        
        public void SetSettings(bool isSoundEnabled, bool isMusicEnabled, int graphicsQuality)
        {
            this.isSoundEnabled = isSoundEnabled;
            this.isMusicEnabled = isMusicEnabled;
            this.graphicsQuality = graphicsQuality;
        }
        
        public void AddNewItem(string itemId, Color itemColor = default)
        {
            if (string.IsNullOrEmpty(itemId))
                return;
                
            foreach (var item in unlockedItemsData)
            {
                if (item.ItemId == itemId)
                    return;
            }
            
            var newItemSaveData = new ItemSaveData(itemId, itemColor);
            unlockedItemsData.Add(newItemSaveData);
        }
        
        public void UpdateItemColor(string itemId, Color color)
        {
            if (string.IsNullOrEmpty(itemId))
                return;
                
            foreach (var item in unlockedItemsData)
            {
                if (item.ItemId == itemId)
                {
                    item.SetColor(color);
                    return;
                }
            }
            
            var itemData = Game.Infrastructure.Configs.ConfigsProxy.CharactersAndShopConfig.GetItemById(itemId);
            if (itemData != null && itemData.ItemIsAlwaysUnlocked)
            {
                AddNewItem(itemId, color);
            }
        }
        
        public Color GetItemColor(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return Color.white;
                
            foreach (var item in unlockedItemsData)
            {
                if (item.ItemId == itemId)
                    return item.ItemColor;
            }
            
            return Color.white;
        }

        public void AddNewUnlockedLocation(string locationId)
        {
            unlockedLocationImprovements.Add(locationId);
        }
        
        public void SaveCharacterSlots(List<CharacterSlotData> slots)
        {
            characterSlots.Clear();

            characterSlots = slots;
        }

        public void SaveCharacterTemplateId(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
                return;
                
            currentCharacterTemplateId = templateId;
        }
        
        [Serializable]
        public class ItemSaveData
        {
            [SerializeField] private string itemId;
            [SerializeField] private Color itemColor = Color.white;

            public string ItemId => itemId;
            public Color ItemColor => itemColor;

            public ItemSaveData(string itemId, Color color = default)
            {
                this.itemId = itemId;
                this.itemColor = color == default ? Color.white : color;
            }
            
            public void SetColor(Color color)
            {
                itemColor = color;
            }
        }
        
        [Serializable]
        public class CharacterSlotData
        {
            [SerializeField] private string slotId;
            [SerializeField] private string itemId;
            [SerializeField] private Color itemColor = Color.white;
            
            public string SlotId => slotId;
            public string ItemId => itemId;
            public Color ItemColor => itemColor;
            
            public CharacterSlotData() { }
            
            public CharacterSlotData(string slotId, string itemId)
            {
                this.slotId = slotId;
                this.itemId = itemId;
                this.itemColor = Color.white;
            }
            
            public CharacterSlotData(string slotId, string itemId, Color itemColor)
            {
                this.slotId = slotId;
                this.itemId = itemId;
                this.itemColor = itemColor;
            }
            
            public void SetColor(Color color)
            {
                itemColor = color;
            }
        }
    }
}
