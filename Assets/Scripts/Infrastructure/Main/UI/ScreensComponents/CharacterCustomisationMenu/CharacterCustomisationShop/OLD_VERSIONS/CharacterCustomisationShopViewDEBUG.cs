/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Game.Services.Common;
using UnityEngine;
using YG;
using Zenject;
using Game.Services.Common.Logging;

namespace Game.Infrastructure.Main.UI
{
    
    public class CharacterCustomisationShopView : MonoBehaviour, IAsyncDisposable
    {
        // === FIELDS & PROPERTIES ===
        
        private CharacterShopConfig shopConfig;
        
        private IAssetsService assetsService;
        private CharactersService charactersService;
        private ICharacterItemsShopService itemsShopService;
    
        [SerializeField] private ColorSelector colorSelector;
        [SerializeField] private CharacterCustomisationShopPanelManager panelManager;
        
        [Header("Shop Frame And Cells")] 
        
        [SerializeField] private RectTransform mainShopFrame;
        [SerializeField] private float cellYPosMultiplier = 1;
        
        private CharacterItemData currentSelectedItem;
        private CharacterCustomizationView targetCharacterCustomization;
        
        private List<ShopCellView> shopCellsCurrent = new List<ShopCellView>();
        private Stack<CharacterItemData> selectedItemPacks = new Stack<CharacterItemData>();
        private CharacterItemData firstSelectedItemPack;
        
        private List<SavesYG.CharacterSlotData> actualCharacterItems = new();
        
        private bool isShopAlreadyRebuild;
        private bool isShopDisposed;
        
        // === DEPENDENCY INJECTION ===

        [Inject]
        private void Construct(IAssetsService assetsService, 
            CharactersService charactersService, 
            ICharacterItemsShopService characterItemsShopService)
        {
            this.assetsService = assetsService;
            this.charactersService = charactersService;
            this.itemsShopService = characterItemsShopService;
        }
        
        // === INITIALIZATION ===
        
        public void Initialize()
        {
            shopConfig = ConfigsProxy.CharactersAndShopConfig;
            
            targetCharacterCustomization = charactersService.GetCharacter(shopConfig.MainCharacterId);
            charactersService.OnCharacterGenderSwapped += OnCharacterGenderSwapped;
            
            actualCharacterItems = targetCharacterCustomization.GetSlotsData();
            
            colorSelector.onColorUpdate += OnColorUpdate;
            colorSelector.onColorBuy += OnColorBuy;
            
            firstSelectedItemPack = ScriptableObject.CreateInstance<CharacterItemData>();
            firstSelectedItemPack.SetPack(shopConfig.ItemsData);
            selectedItemPacks.Push(firstSelectedItemPack);

            panelManager.GetBackButton().onClick.AddListener(ShopBack);
            panelManager.GetBuyButton().onClick.AddListener(UnlockItem);
            
            BuildItemCells(firstSelectedItemPack.ItemsPack).Forget();
        }
        
        // === SHOP CELLS MANAGEMENT ===

        private async UniTask BuildItemCells(CharacterItemData[] items)
        {
            int skippedCells = 0;
            
            for (int index = 0; index < items.Length; index++)
            {
                if(isShopDisposed)
                    return;

                var itemData = items[index];
                
                if(itemData.IsHiddenFromShop)
                {
                    skippedCells++;
                    continue;
                }
                
                if(itemData.CharacterGender != CharacterGender.All && 
                   targetCharacterCustomization != null && 
                   targetCharacterCustomization.CustomizationTemplate.CharacterGender != itemData.CharacterGender)
                {
                    skippedCells++;
                    continue;
                }
                
                var cellCreateCooldownTask = UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                
                var cellView = await CreateCell(itemData, index - skippedCells);
                shopCellsCurrent.Add(cellView);
                
                await cellCreateCooldownTask;
            }
            
            UpdateCellsState();
        }
        
        private async UniTask<ShopCellView> CreateCell(CharacterItemData itemData, int index)
        {
            var cellView = await assetsService.GetAsset<ShopCellView>
                (ConfigsProxy.AssetsPathsConfig.CustomisationMenuShopCellID);
            
            var cellT = cellView.CellT;
            cellT.SetParent(mainShopFrame, false);
            
            int maxCellsOnWidth = Mathf.Max(1, Mathf.FloorToInt(mainShopFrame.sizeDelta.x / cellT.sizeDelta.x));
            float cellsMoveStep = mainShopFrame.sizeDelta.x / maxCellsOnWidth;
            
            const int cellStartPosSmooth = 2;
            var remains = (index % maxCellsOnWidth); 
            var xCellPos = (cellsMoveStep / cellStartPosSmooth) + remains * cellsMoveStep;
            var yCellPos = (cellsMoveStep / cellStartPosSmooth) + index / maxCellsOnWidth * (cellsMoveStep * cellYPosMultiplier);
            
            cellT.anchoredPosition = new Vector2(xCellPos, -yCellPos);
            
            await cellView.SetNewItem(itemData);
            UpdateCellState(cellView);
            
            cellView.StartShowAnimation().Forget();

            cellView.onClick += () => SelectItem(itemData).Forget();
            
            return cellView;
        }
        
        private async UniTask RebuildItemCells(CharacterItemData[] items)
        {
            if(isShopAlreadyRebuild)
                return;
            
            isShopAlreadyRebuild = true;
            
            await DestroyCurrentCells();
            
            await BuildItemCells(items);
                
            isShopAlreadyRebuild = false;
        }
        
        private async UniTask DestroyCurrentCells()
        {
            if(shopCellsCurrent.Count == 0)
                return;
            
            for (int i = shopCellsCurrent.Count-1; i >= 0; i--)
            {
                if (shopCellsCurrent[i] != null && !isShopDisposed)
                {
                    shopCellsCurrent[i].StartHideAnimation();
                    await UniTask.Delay(ConfigsProxy.AnimationsConfig.CellsCooldown);
                }
            }
            
            foreach (var cell in shopCellsCurrent)
            {
                if (cell != null)
                {
                    assetsService.ReleaseAsset(cell.gameObject);
                }
            }
            
            shopCellsCurrent.Clear();
        }
        
        // === ITEM SELECTION & APPLICATION ===

        private async UniTask SelectItem(CharacterItemData itemData)
        {
            if(itemData == null)
                return;
            
            colorSelector.ColorRollback();
            currentSelectedItem = itemData;
            
            if (itemData.IsItemsPack)
            {
                selectedItemPacks.Push(itemData);
                
                await RebuildItemCells(itemData.ItemsPack);

                panelManager.SetBackButtonShow(true);
                
                return;
            }
            
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
            
            panelManager.SetDescPanelShow(true);
            panelManager.UpdateDescPanel(itemData);
            
            if (itemsShopService.IsItemUnlocked(itemData))
            {
                panelManager.SetPricePanelShow(false);
                panelManager.SetBuyButtonShow(false);
                
                if(itemData.IsCanColorize)
                {
                    colorSelector.SetCurrentItemData(itemData);
                    colorSelector.SetColorSelectorPanelShow(true);
                }
                else
                {
                    colorSelector.SetColorSelectorPanelShow(false);
                }
            }
            else
            {
                panelManager.UpdatePricePanel(itemData);
                panelManager.SetBuyButtonShow(true);
                colorSelector.SetColorSelectorPanelShow(false);
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
                if(string.IsNullOrEmpty(item.ItemId))
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
                panelManager.SetPricePanelShow(false);
                panelManager.SetBuyButtonShow(false);
                
                if(currentSelectedItem.IsCanColorize)
                {
                    colorSelector.SetCurrentItemData(currentSelectedItem);
                    colorSelector.SetColorSelectorPanelShow(true);
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

        private void HideAllPanels()
        {
            panelManager.HideAllPanels();
            colorSelector.SetColorSelectorPanelShow(false);
        }

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
        
        private void UpdateCellState(ShopCellView cell)
        {
            if (cell == null || cell.ItemCurrent == null)
                return;
                
            var cellItem = cell.ItemCurrent;
            
            cell.SetCellSelectedColorActive(currentSelectedItem != null && cellItem.ItemId == currentSelectedItem.ItemId);
            cell.SetCellLockObjectActive(!itemsShopService.IsItemUnlocked(cellItem));
            
            bool isCanColorize = cellItem.IsCanColorize && !cellItem.IsItemsPack;
            cell.SetCellColorChangeIconActive(isCanColorize);
            
            if(isCanColorize && itemsShopService.IsItemUnlocked(cellItem))
            {
                Color resultColor = itemsShopService.GetItemColor(cellItem.ItemId);
                cell.SetCellColorizeColor(resultColor);
                cell.SetCellColorChangeIconColor(resultColor);
            }
            else if (isCanColorize)
            {
                cell.SetCellColorizeColor(cellItem.DefaultColor);
                cell.SetCellColorChangeIconColor(cellItem.DefaultColor);
            }
            
            if (cellItem.IconColor != Color.white && !cellItem.IsIconObject)
                cell.SetCellColorizeColor(cellItem.IconColor);
            
            if(cellItem.IsItemsPack)
                return;
            
            bool isItemEquipped = targetCharacterCustomization.IsItemEquipped(cellItem.ItemId);
            bool isRemovable = targetCharacterCustomization.IsSlotRemovable(cellItem.SlotId);
            
            cell.SetCellFrameState(isItemEquipped? (isRemovable ? ShopCellFrameState.SelectedRemoved : ShopCellFrameState.Selected)
                : ShopCellFrameState.Idle);
        }
        
        // === EVENT HANDLERS ===

        private void ShopBack()
        {
            const int minPacksCount = 2;

            if(isShopAlreadyRebuild)
                return;
            
            colorSelector.ColorRollback();
            RestoreActualCharacterState().Forget();
            
            if (selectedItemPacks.Count >= minPacksCount)
                selectedItemPacks.Pop();
            
            RebuildItemCells(selectedItemPacks.Peek().ItemsPack).Forget();
            panelManager.SetBackButtonShow(selectedItemPacks.Count >= minPacksCount);
            
            currentSelectedItem = null;
            HideAllPanels();
        }

        private void OnColorUpdate(Color newColor)
        {
            if (currentSelectedItem == null || !currentSelectedItem.IsCanColorize || targetCharacterCustomization == null)
                return;

            targetCharacterCustomization.SetSlotColor(currentSelectedItem.SlotId,newColor);
        }
        
        private void OnColorBuy(Color newColor)
        {
            UpdateCellsState();
            TryStoreActualCharacterState();
        }
        
        private void OnCharacterGenderSwapped(string characterId, CharacterCustomizationView newCharacter)
        {
            if (characterId != shopConfig.MainCharacterId) 
                return;
            
            targetCharacterCustomization = newCharacter;
                
            actualCharacterItems = targetCharacterCustomization.GetSlotsData();
                
            ResetShop().Forget();
        }

        // === UTILITY METHODS ===

        private async UniTask ResetShop()
        {
            panelManager.SetBackButtonShow(false);
            HideAllPanels();
            
            selectedItemPacks.Clear();
            selectedItemPacks.Push(firstSelectedItemPack);

            currentSelectedItem = null;
            await RebuildItemCells(selectedItemPacks.Peek().ItemsPack);
        }

        // === CLEANUP ===
        
        public async ValueTask DisposeAsync()
        {
            isShopDisposed = true;

            if (colorSelector != null)
            {
                colorSelector.ColorRollback();
                colorSelector.onColorUpdate -= OnColorUpdate;
                colorSelector.onColorBuy -= OnColorBuy;
            }
            
            if (charactersService != null)
            {
                charactersService.OnCharacterGenderSwapped -= OnCharacterGenderSwapped;
            }
            
            RestoreActualCharacterState().Forget();
            
            await UniTask.WaitUntil(() => isShopAlreadyRebuild == false);
            await DestroyCurrentCells();
        }
    }
}
*/