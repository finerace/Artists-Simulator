using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Additionals;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Infrastructure.Main.UI
{
    
    public class ShopCellsPresenter : IShopCellsPresenter<CharacterItemData>, ISpecificDataSetter<CharacterItemData>,ISpecificDataSetter<CharacterCustomizationView>
    {
        // === EVENTS ===
        
        public event Action<CharacterItemData> OnCellClicked;
        
        // === DEPENDENCIES ===
        
        private readonly ICharacterItemsShopView shopView;
        private readonly IShopCellsFabric cellsFabric;
        private readonly ILayoutManager layoutManager;
        private readonly IItemsProvider itemsProvider;
        private readonly ICharacterItemsShopService itemsShopService;
        
        // === FIELDS & PROPERTIES ===
        
        private List<IShopCellView<CharacterItemData>> shopCellsCurrent = new List<IShopCellView<CharacterItemData>>();
        private CharacterItemData currentSelectedItem;
        private CharacterCustomizationView targetCharacter;

        private bool isRebuildInProgress;
        private bool isDisposed;
        
        // === CONSTRUCTOR ===
        
        public ShopCellsPresenter(
            ICharacterItemsShopView shopView,
            IShopCellsFabric cellsFabric,
            ILayoutManager layoutManager,
            IItemsProvider itemsProvider,
            ICharacterItemsShopService itemsShopService)
        {
            this.shopView = shopView;
            this.cellsFabric = cellsFabric;
            this.layoutManager = layoutManager;
            this.itemsProvider = itemsProvider;
            this.itemsShopService = itemsShopService;
        }
        
        // === CELLS BUILDING ===
        
        private async UniTask<IShopCellView<CharacterItemData>> CreateCell(CharacterItemData itemData)
        {
            var cellView = await cellsFabric.CreateCell(itemData, shopView.ShopContainer);
            UpdateCellState(cellView);
            
            if (cellView == null)
                return null;
            
            cellView.onClick += () => 
            {
                OnCellClicked?.Invoke(itemData);
            };
            
            return cellView;
        }
        
        public async UniTask BuildCells(CharacterItemData[] items)
        {
            var filteredItems = itemsProvider.GetFilteredItems(items, targetCharacter.CustomizationTemplate.CharacterGender);
            
            for (int index = 0; index < filteredItems.Length; index++)
            {
                if (isDisposed)
                    return;
                
                var itemData = filteredItems[index];
                
                var delay = UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                var cellView = await CreateCell(itemData);
                
                if (cellView == null)
                    continue;
                    
                await delay;
                
                SetupCell(index, cellView);
                void SetupCell(int index, IShopCellView<CharacterItemData> cellView)
                {
                    SetCellPosition(index, cellView);
                    shopCellsCurrent.Add(cellView);
                    cellView.StartShowAnimation().Forget();
                }
            }
        }
        
        public async UniTask RebuildCells(CharacterItemData[] items)
        {
            if (isRebuildInProgress)
                return;
            
            isRebuildInProgress = true;
            
            await DestroyCells();
            await BuildCells(items);
            
            isRebuildInProgress = false;
        }
        
        public async UniTask DestroyCells()
        {
            if (shopCellsCurrent.Count == 0)
                return;
            
            for (int i = shopCellsCurrent.Count - 1; i >= 0; i--)
            {
                if (shopCellsCurrent[i] != null && !isDisposed)
                {
                    shopCellsCurrent[i].StartHideAnimation().Forget();
                    await UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                }
                
                cellsFabric.DestroyCell(shopCellsCurrent[i]);
            }
            
            shopCellsCurrent.Clear();
        }
        
        // === CELLS STATE MANAGEMENT ===
        
        public void UpdateCellsState()
        {
            foreach (var cell in shopCellsCurrent)
            {
                if (cell != null)
                {
                    UpdateCellState(cell);
                }
            }
        }
        
        public void UpdateCellState(IShopCellView<CharacterItemData> cell)
        {
            if (cell?.CurrentItem == null)
                return;
            
            var cellItem = cell.CurrentItem;
            var cellState = CalculateCellState(cellItem);
            
            cell.SetCellState(cellState);
            UpdateCellColorization(cell, cellItem, itemsShopService);
            
            ShopCellFrameState CalculateCellState(CharacterItemData cellItem)
            {
                bool isItemEquipped = targetCharacter.IsItemEquipped(cellItem.ItemId);
                bool isRemovable = !cellItem.IsItemsPack && targetCharacter.IsSlotRemovable(cellItem.SlotId);
                bool isLocked = !itemsShopService.IsItemUnlocked(cellItem);
                bool isSelected = currentSelectedItem != null && cellItem.ItemId == currentSelectedItem.ItemId;
                
                if (isLocked)
                {
                    return isSelected ? ShopCellFrameState.SelectedLocked : ShopCellFrameState.Locked;
                }
                
                if (isItemEquipped)
                {
                    return isRemovable ? ShopCellFrameState.SelectedRemoved : ShopCellFrameState.Selected;
                }
                
                return isSelected ? ShopCellFrameState.Selected : ShopCellFrameState.Idle;
            }
        }
        
        // === DATA SETTERS ===
        
        public void SetData(CharacterItemData data)
        {
            currentSelectedItem = data;
        }

        public void SetData(CharacterCustomizationView data)
        {
            targetCharacter = data;
        }
        
        // === UTILITY METHODS ===
        
        private void SetCellPosition(int index, IShopCellView<CharacterItemData> cellView)
        {
            var cellPosition = layoutManager.CalculateItemPosition(
                index, shopView.ShopContainer, cellView.CellTransform);
            
            cellView.CellTransform.anchoredPosition = cellPosition;
        }
        
        private void UpdateCellColorization(IShopCellView<CharacterItemData> cell, CharacterItemData cellItem, ICharacterItemsShopService itemsShopService)
        {
            if (cell is not IColorizedShopCellView colorizedCell)
                return;
            
            if (cellItem.IsCanColorize && !cellItem.IsItemsPack)
            {
                Color resultColor = itemsShopService.IsItemUnlocked(cellItem)
                    ? itemsShopService.GetItemColor(cellItem.ItemId)
                    : cellItem.DefaultColor;
                
                colorizedCell.SetCellColorizeColor(resultColor);
                colorizedCell.SetColorizeIconActive(true);
            }
            else if (cellItem.IconColor != Color.white && !cellItem.IsIconObject)
            {
                colorizedCell.SetCellColorizeColor(cellItem.IconColor);
            }
            else
            {
                colorizedCell.SetColorizeIconActive(false);
            }
        }
        
        // === CLEANUP ===
        
        public ValueTask DisposeAsync()
        {
            isDisposed = true;
            return DestroyCells().AsValueTask();
        }
    
    }
} 