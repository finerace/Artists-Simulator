/*using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Game.Services.Common;
using UnityEngine;
using YG;
using Game.Services.Common.Logging;

namespace Game.Infrastructure.Main.UI
{
    
    public class ShopPresenter : IShopPresenter
    {
        // === DEPENDENCIES ===
        
        private readonly ICharacterItemsShopView shopView;
        private readonly IColorSelectWidget colorSelector;
        private readonly IShopCellsFabric cellsFabric;
        private readonly ILayoutManager layoutManager;
        private readonly IItemsProvider itemsProvider;
        private readonly IAssetsService assetsService;
        private readonly CharactersService charactersService;
        private readonly ICharacterItemsShopService itemsShopService;
        
        // === FIELDS & PROPERTIES ===
        
        private CharacterShopConfig shopConfig;
        private CharacterItemData currentSelectedItem;
        private CharacterCustomizationView targetCharacterCustomization;
        
        private List<IShopCellView> shopCellsCurrent = new List<IShopCellView>();
        private Stack<CharacterItemData> selectedItemPacks = new Stack<CharacterItemData>();
        private CharacterItemData firstSelectedItemPack;
        
        private List<SavesYG.CharacterSlotData> actualCharacterItems = new();
        
        private bool isShopAlreadyRebuild;
        private bool isShopDisposed;
        
        // === CONSTRUCTOR ===
        
        public ShopPresenter(
            ICharacterItemsShopView shopView,
            IColorSelectWidget colorSelector,
            IShopCellsFabric cellsFabric,
            ILayoutManager layoutManager,
            IItemsProvider itemsProvider,
            CharactersService charactersService,
            ICharacterItemsShopService itemsShopService,
            IAssetsService assetsService)
        {
            this.shopView = shopView;
            this.colorSelector = colorSelector;
            this.cellsFabric = cellsFabric;
            this.layoutManager = layoutManager;
            this.itemsProvider = itemsProvider;
            this.charactersService = charactersService;
            this.itemsShopService = itemsShopService;
            this.assetsService = assetsService;
        }
        
        // === INITIALIZATION ===
        
        public async UniTask Initialize()
        {
            shopConfig = ConfigsProxy.CharactersAndShopConfig;
            
            targetCharacterCustomization = charactersService.GetCharacter(shopConfig.MainCharacterId);
            actualCharacterItems = targetCharacterCustomization.GetSlotsData();
            
            SubscribeToEvents();
            
            firstSelectedItemPack = ScriptableObject.CreateInstance<CharacterItemData>();
            
            firstSelectedItemPack.SetPack(shopConfig.ItemsData);
            selectedItemPacks.Push(firstSelectedItemPack);
            
            await BuildItemCells(firstSelectedItemPack.ItemsPack);
        }
        
        private void SubscribeToEvents()
        {
            charactersService.OnCharacterGenderSwapped += OnCharacterGenderSwapped;
            colorSelector.onColorUpdate += OnColorUpdate;
            colorSelector.onColorBuy += OnColorBuy;
            shopView.onBuyButtonClick += UnlockItem;
            shopView.onBackButtonClick += ShopBack;
        }
        
        // === SHOP CELLS MANAGEMENT ===
        
        private async UniTask BuildItemCells(CharacterItemData[] items)
        {
            items = itemsProvider.GetFilteredItems(items,
                targetCharacterCustomization.CustomizationTemplate.CharacterGender);
            
            for (int index = 0; index < items.Length; index++)
            {
                if (isShopDisposed)
                    return;
                
                var itemData = items[index];
                
                var delay = UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                var cellView = await CreateCell(itemData);
                await delay;
                
                SetCellPosition();
                void SetCellPosition()
                {
                    var cellPosition = layoutManager.CalculateItemPosition
                        (index, shopView.ShopContainer, cellView.CellTransform);
                    
                    cellView.CellTransform.anchoredPosition = cellPosition;
                }
                
                shopCellsCurrent.Add(cellView);
                cellView.StartShowAnimation().Forget();
            }
            
            UpdateCellsState();
        }
        
        private async UniTask<IShopCellView> CreateCell(CharacterItemData itemData)
        {
            var cellView = await cellsFabric.CreateCell(itemData, shopView.ShopContainer);
            
            if (cellView == null)
                return null;
            
            cellView.onClick += () => SelectItem(itemData).Forget();
            UpdateCellState(cellView);
            
            return cellView;
        }
        
        private async UniTask RebuildItemCells(CharacterItemData[] items)
        {
            if (isShopAlreadyRebuild)
                return;
            
            isShopAlreadyRebuild = true;
            
            await DestroyCurrentCells();
            await BuildItemCells(items);
            
            isShopAlreadyRebuild = false;
        }
        
        private async UniTask DestroyCurrentCells()
        {
            if (shopCellsCurrent.Count == 0)
                return;
            
            for (int i = shopCellsCurrent.Count - 1; i >= 0; i--)
            {
                if (shopCellsCurrent[i] != null && !isShopDisposed)
                {
                    shopCellsCurrent[i].StartHideAnimation().Forget();
                    await UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                }
                
                assetsService.ReleaseAsset(shopCellsCurrent[i].CellTransform.gameObject);
            }
            
            shopCellsCurrent.Clear();
        }
        
        // === ITEM SELECTION & APPLICATION ===
        
        private async UniTask SelectItem(CharacterItemData itemData)
        {
            if (itemData == null)
                return;
            
            ColorRollback();
            
            if (itemData.IsItemsPack)
            {
                selectedItemPacks.Push(itemData);
                await RebuildItemCells(itemData.ItemsPack);

                shopView.SetBackButtonShow(true);

                return;
            }

            currentSelectedItem = itemData;
            
            bool isItemEquipped = targetCharacterCustomization.IsItemEquipped(itemData.ItemId);
            bool isSlotRemovable = targetCharacterCustomization.IsSlotRemovable(itemData.SlotId);
            
            if (isItemEquipped && isSlotRemovable)
            {
                targetCharacterCustomization.ClearSlot(itemData.SlotId);
                UpdateCellsState();
                TryStoreActualCharacterState();
                return;
            }
            
            await ApplyItemToCharacter(itemData);
            
            shopView.SetItemName(itemData.ItemNameId.ToString());
            
            if (itemsShopService.IsItemUnlocked(itemData))
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
                    colorSelector.SetPanelShow(false);
                
            }
            else
            {
                shopView.SetPricePanelShow(true);
                shopView.SetCoinsPrice(itemData.CoinsPrice);
                shopView.SetCrystalsPrice(itemData.CrystalsPrice);
                
                shopView.SetBuyButtonShow(true);
                colorSelector.SetPanelShow(false);
            }
            
            UpdateCellsState();
            TryStoreActualCharacterState();
        }
        
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
        
        private void TryStoreActualCharacterState()
        {
            var checkItems = targetCharacterCustomization.GetSlotsData();
            
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
                    return;
                }
            }
            
            Logs.Info($"Actual character state stored with {checkItems.Count} items!");
            
            actualCharacterItems.Clear();
            actualCharacterItems = checkItems;
        }
        
        private void UnlockItem()
        {
            if (currentSelectedItem == null || itemsShopService.IsItemUnlocked(currentSelectedItem))
                return;
            
            if (itemsShopService.TryToUnlockItem(currentSelectedItem))
            {
                shopView.SetPricePanelShow(false);
                shopView.SetBuyButtonShow(false);
                
                if (currentSelectedItem.IsCanColorize)
                {
                    var savedColor = itemsShopService.GetItemColor(currentSelectedItem.ItemId);
                    colorSelector.SetCurrentColor(savedColor);
                    colorSelector.SetPanelShow(true);
                }
                
                UpdateCellsState();
                TryStoreActualCharacterState();
            }
        }
        
        private async UniTask RestoreActualCharacterState()
        {
            if (targetCharacterCustomization == null)
                return;
            
            await targetCharacterCustomization.SetSlotsSavedData(actualCharacterItems);
        }
        
        // === UI STATE MANAGEMENT ===
        
        private void UpdateCellsState()
        {
            foreach (var cell in shopCellsCurrent)
            {
                if (cell != null)
                {
                    UpdateCellState(cell);
                }
            }
        }
        
        private void UpdateCellState(IShopCellView cell)
        {
            if (cell == null || cell.CurrentItem == null)
                return;
            
            var cellItem = cell.CurrentItem;
            bool isItemEquipped = targetCharacterCustomization.IsItemEquipped(cellItem.ItemId);
            bool isRemovable = !cellItem.IsItemsPack && targetCharacterCustomization.IsSlotRemovable(cellItem.SlotId);
            bool isLocked = !itemsShopService.IsItemUnlocked(cellItem);
            bool isSelected = currentSelectedItem != null && cellItem.ItemId == currentSelectedItem.ItemId;
            
            ShopCellFrameState frameState = ShopCellFrameState.Idle;
            
            if (isLocked)
            {
                frameState = isSelected ? ShopCellFrameState.SelectedLocked : ShopCellFrameState.Locked;
            }
            else if (isItemEquipped)
            {
                frameState = isRemovable ? ShopCellFrameState.SelectedRemoved : ShopCellFrameState.Selected;
            }
            else if (isSelected)
            {
                frameState = ShopCellFrameState.Selected;
            }
            
            cell.SetCellState(frameState);
            
            if (cell is IColorizedShopCellView colorizedCell)
            {
                if (cellItem.IsCanColorize && !cellItem.IsItemsPack)
                {
                    Color resultColor = itemsShopService.IsItemUnlocked(cellItem)
                        ? itemsShopService.GetItemColor(cellItem.ItemId)
                        : cellItem.DefaultColor;
                    
                    colorizedCell.SetCellColorizeColor(resultColor);
                    colorizedCell.SetColorizeIconActive(true);
                }
                else if (cellItem.IconColor != Color.white && !cellItem.IsIconObject)
                    colorizedCell.SetCellColorizeColor(cellItem.IconColor);
                else
                    colorizedCell.SetColorizeIconActive(false);
            }
        }
        
        // === EVENT HANDLERS ===
        
        private void ShopBack()
        {
            const int minPacksCount = 2;
            
            if (isShopAlreadyRebuild)
                return;
            
            ColorRollback();
            RestoreActualCharacterState().Forget();
            
            if (selectedItemPacks.Count >= minPacksCount)
                selectedItemPacks.Pop();
            
            RebuildItemCells(selectedItemPacks.Peek().ItemsPack).Forget();
            shopView.SetBackButtonShow(selectedItemPacks.Count >= minPacksCount);
            
            currentSelectedItem = null;
            shopView.HideAllPanels();
            colorSelector.SetPanelShow(false);
        }
        
        private void OnColorUpdate(Color newColor)
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize || targetCharacterCustomization == null)
                return;
            
            colorSelector.SetBuyColorPanelsShow(true);

            targetCharacterCustomization.SetSlotColor(currentSelectedItem.SlotId, newColor);
        }
        
        private void OnColorBuy(Color newColor)
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize)
                return;
            
            if (itemsShopService.TryToSetColor(currentSelectedItem, newColor))
            {
                UpdateCellsState();
                TryStoreActualCharacterState();
            
                colorSelector.SetBuyColorPanelsShow(false);
            }
        }
        
        private void OnCharacterGenderSwapped(string characterId, CharacterCustomizationView newCharacter)
        {
            if (characterId != shopConfig.MainCharacterId)
                return;
            
            targetCharacterCustomization = newCharacter;
            actualCharacterItems = targetCharacterCustomization.GetSlotsData();
            
            ResetShop().Forget();
        }
        
        private void ColorRollback()
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize)
                return;
            
            if (itemsShopService.IsItemUnlocked(currentSelectedItem))
            {
                var savedColor = itemsShopService.GetItemColor(currentSelectedItem.ItemId);
                colorSelector.SetCurrentColor(savedColor);
                OnColorUpdate(savedColor);
            }
        }
        
        // === UTILITY METHODS ===
        
        public async UniTask ResetShop()
        {
            shopView.SetBackButtonShow(false);
            shopView.HideAllPanels();
            
            selectedItemPacks.Clear();
            selectedItemPacks.Push(firstSelectedItemPack);
            
            currentSelectedItem = null;
            await RebuildItemCells(selectedItemPacks.Peek().ItemsPack);
        }
        
        // === CLEANUP ===
        
        public async ValueTask DisposeAsync()
        {
            isShopDisposed = true;
            
            UnsubscribeFromEvents();
            ColorRollback();
            RestoreActualCharacterState().Forget();
            
            await UniTask.WaitUntil(() => isShopAlreadyRebuild == false);
            await DestroyCurrentCells();
        }
        
        private void UnsubscribeFromEvents()
        {
            if (charactersService != null)
                charactersService.OnCharacterGenderSwapped -= OnCharacterGenderSwapped;
            
            if (colorSelector != null)
            {
                colorSelector.onColorUpdate -= OnColorUpdate;
                colorSelector.onColorBuy -= OnColorBuy;
            }
            
            if (shopView != null)
            {
                shopView.onBuyButtonClick -= UnlockItem;
                shopView.onBackButtonClick -= ShopBack;
            }
        }
    }
}*/