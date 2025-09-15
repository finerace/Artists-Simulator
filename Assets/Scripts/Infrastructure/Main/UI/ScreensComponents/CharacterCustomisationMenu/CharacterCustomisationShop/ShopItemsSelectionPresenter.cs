using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Services.Common.Logging;
using Game.Services.Meta;
using UnityEngine;
using YG;

namespace Game.Infrastructure.Main.UI
{
    public class ShopItemsSelectionPresenter : IShopItemsSelectionPresenter<CharacterItemData, CharacterCustomizationView>, IShopItemUnlockNotifier
    {
        // === EVENTS ===
        
        public event Action<CharacterItemData[]> OnItemPackSelected;
        public event Action OnItemUnlocked;
        
        // === DEPENDENCIES ===
        
        private readonly ICharacterItemsShopView shopView;
        private readonly IColorSelectWidget colorSelector;
        private readonly ICharacterItemsShopService itemsShopService;
        
        // === FIELDS & PROPERTIES ===
        
        private CharacterCustomizationView targetCharacterCustomization;
        private CharacterItemData currentSelectedItem;
        private List<SavesYG.CharacterSlotData> actualCharacterItems = new();
        
        // === CONSTRUCTOR ===
        
        public ShopItemsSelectionPresenter(
            ICharacterItemsShopView shopView,
            IColorSelectWidget colorSelector,
            ICharacterItemsShopService itemsShopService)
        {
            this.shopView = shopView;
            this.colorSelector = colorSelector;
            this.itemsShopService = itemsShopService;
        }
        
        // === TARGET CHARACTER MANAGEMENT ===
        
        public void UpdateTargetCharacter(CharacterCustomizationView targetCharacterCustomization)
        {
            this.targetCharacterCustomization = targetCharacterCustomization;
            
            ColorRollback();
            TryStoreActualCharacterState();
        }
        
        public CharacterItemData GetCurrentSelectedItem()
        {
            return currentSelectedItem;
        }
        
        public CharacterCustomizationView GetTargetCharacter()
        {
            return targetCharacterCustomization;
        }
        
        // === ITEM SELECTION ===
        
        public async UniTask SelectItem(CharacterItemData itemData)
        {
            if (itemData == null)
                return;
            
            ColorRollback();
            
            if (itemData.IsItemsPack)
            {
                OnItemPackSelected?.Invoke(itemData.ItemsPack);
                return;
            }

            currentSelectedItem = itemData;
            
            if (IsEquippedAndRemovable(itemData))
            {
                targetCharacterCustomization.ClearSlot(itemData.SlotId);
                TryStoreActualCharacterState();
                return;
            }
            
            await ApplyItemToCharacter(itemData);
            UpdateUIForSelectedItem(itemData);
            TryStoreActualCharacterState();
            
            bool IsEquippedAndRemovable(CharacterItemData itemData)
            {
                return targetCharacterCustomization.IsItemEquipped(itemData.ItemId) && 
                    targetCharacterCustomization.IsSlotRemovable(itemData.SlotId);
            }
        }
        
        // === ITEM UNLOCKING ===
        
        public void UnlockItem()
        {
            if (currentSelectedItem == null || itemsShopService.IsItemUnlocked(currentSelectedItem))
                return;
            
            if (itemsShopService.TryToUnlockItem(currentSelectedItem))
            {
                UpdateUIAfterUnlock();
                void UpdateUIAfterUnlock()
                {
                    shopView.SetPricePanelShow(false);
                    shopView.SetBuyButtonShow(false);
                    
                    if (currentSelectedItem.IsCanColorize)
                    {
                        var savedColor = itemsShopService.GetItemColor(currentSelectedItem.ItemId);
                        colorSelector.SetCurrentColor(savedColor);
                        colorSelector.SetPanelShow(true);
                    }
                }

                TryStoreActualCharacterState();
                OnItemUnlocked?.Invoke();
            }
        }
        
        // === COLOR MANAGEMENT ===
        
        public void HandleColorUpdate(Color newColor)
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize || targetCharacterCustomization == null)
                return;
            
            colorSelector.SetBuyColorPanelsShow(true);
            targetCharacterCustomization.SetSlotColor(currentSelectedItem.SlotId, newColor);
        }
        
        public void HandleColorPurchase(Color newColor)
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize)
                return;
            
            if (itemsShopService.TryToSetColor(currentSelectedItem, newColor))
            {
                TryStoreActualCharacterState();
                colorSelector.SetBuyColorPanelsShow(false);
            }
        }
        
        public void ColorRollback()
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize)
                return;
            
            if (itemsShopService.IsItemUnlocked(currentSelectedItem))
            {
                var savedColor = itemsShopService.GetItemColor(currentSelectedItem.ItemId);
                colorSelector.SetCurrentColor(savedColor);
                HandleColorUpdate(savedColor);
            }
        }
        
        // === CHARACTER STATE MANAGEMENT ===
        
        public async UniTask RestoreActualCharacterState()
        {
            if (targetCharacterCustomization == null)
            {
                Logs.Error("Target character customization is null");
                return;
            }
            
            await targetCharacterCustomization.SetSlotsSavedData(actualCharacterItems);
            
            Logs.Info($"Actual character state restored with {actualCharacterItems.Count} items!");
        }
        
        private void TryStoreActualCharacterState()
        {
            var checkItems = targetCharacterCustomization.GetSlotsData();
            
            if (!ValidateCharacterItems(checkItems))
                return;
            
            Logs.Info($"Actual character state stored with {checkItems.Count} items!");
            
            actualCharacterItems.Clear();
            actualCharacterItems = checkItems;
            
            bool ValidateCharacterItems(List<SavesYG.CharacterSlotData> checkItems)
            {
                foreach (var item in checkItems)
                {
                    if (string.IsNullOrEmpty(item.ItemId))
                    {
                        Logs.Warning($"Item id is null {item.ItemId}");
                        continue;
                    }
                    
                    if (!itemsShopService.IsItemUnlocked(item.ItemId))
                    {
                        Logs.Warning($"Item id is unlocked {item.ItemId}");
                        return false;
                    }
                }
                return true;
            }
        }
        
        // === ITEM APPLICATION ===
        
        private async UniTask ApplyItemToCharacter(CharacterItemData itemData)
        {
            if (targetCharacterCustomization == null || itemData == null)
                return;
            
            await targetCharacterCustomization.ApplyItemToSlot(itemData);
            
            if (itemData.IsCanColorize && itemsShopService.IsItemUnlocked(itemData))
            {
                Color savedColor = itemsShopService.GetItemColor(itemData.ItemId);
                targetCharacterCustomization.SetSlotColor(itemData.SlotId, savedColor);
                colorSelector.SetCurrentColor(savedColor);
            }
        }
        
        // === UI MANAGEMENT ===
        
        private void UpdateUIForSelectedItem(CharacterItemData itemData)
        {
            shopView.SetItemName(itemData.ItemNameId.ToString());
            
            if (itemsShopService.IsItemUnlocked(itemData))
            {
                UpdateUIForUnlockedItem(itemData);
            }
            else
            {
                UpdateUIForLockedItem(itemData);
            }
            
            void UpdateUIForUnlockedItem(CharacterItemData itemData)
            {
                shopView.SetPricePanelShow(false);
                shopView.SetBuyButtonShow(false);
                
                if (itemData.IsCanColorize)
                {
                    var savedColor = itemsShopService.GetItemColor(itemData.ItemId);
                    colorSelector.SetCurrentColor(savedColor);
                    
                    colorSelector.SetPanelShow(true);
                    colorSelector.SetColorPrice(itemData.ColorizeCoinsPrice);
                    colorSelector.SetBuyColorPanelsShow(false);
                }
                else
                {
                    colorSelector.SetPanelShow(false);
                }
            }
            
            void UpdateUIForLockedItem(CharacterItemData itemData)
            {
                shopView.SetPricePanelShow(true);
                shopView.SetCoinsPrice(itemData.CoinsPrice);
                shopView.SetCrystalsPrice(itemData.CrystalsPrice);
                
                shopView.SetBuyButtonShow(true);
                colorSelector.SetPanelShow(false);
            }
        }
    }
} 