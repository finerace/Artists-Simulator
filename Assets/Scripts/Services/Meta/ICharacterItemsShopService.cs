using System;
using System.Collections.Generic;
using UnityEngine;
using YG;

namespace Game.Services.Meta
{
    public interface ICharacterItemsShopService
    {
        event Action<SavesYG.ItemSaveData> OnNewItemUnlocked;
        event Action<string, Color> OnItemColorChanged;
        event Action<List<SavesYG.CharacterSlotData>> OnPlayerCharacterAppearanceChanged;
        
        void Initialize(List<SavesYG.ItemSaveData> unlockedItems);
        void SetPlayerCharacter(CharacterCustomizationView playerCharacter);
        bool TryToUnlockItem(CharacterItemData characterItemData);
        bool TryToSetColor(CharacterItemData characterItemData, Color newColor);
        bool IsItemUnlocked(CharacterItemData itemData);
        bool IsItemUnlocked(string itemId);
        void SetItemColor(string itemId, Color newItemColor);
        Color GetItemColor(string itemId);
    }
} 