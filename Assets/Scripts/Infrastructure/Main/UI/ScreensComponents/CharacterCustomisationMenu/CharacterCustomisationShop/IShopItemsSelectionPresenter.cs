using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    public interface IShopItemsSelectionPresenter<TItem,TCharacter>
    {
        event Action<TItem[]> OnItemPackSelected;
        
        UniTask SelectItem(TItem itemData);
        void UnlockItem();
        UniTask RestoreActualCharacterState();
        
        void HandleColorUpdate(Color newColor);
        void HandleColorPurchase(Color newColor);
        void ColorRollback();
        
        void UpdateTargetCharacter(TCharacter targetCharacterCustomization);
        
        TItem GetCurrentSelectedItem();
        TCharacter GetTargetCharacter();
    }
    
    public interface IShopItemUnlockNotifier
    {
        event Action OnItemUnlocked;
    }
} 