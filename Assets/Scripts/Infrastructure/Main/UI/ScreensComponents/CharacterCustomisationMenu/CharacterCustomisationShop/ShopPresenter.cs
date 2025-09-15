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
    
    public class ShopPresenter : IShopPresenter
    {
        // === DEPENDENCIES ===
        
        private readonly ICharacterItemsShopView shopView;
        private readonly IColorSelectWidget colorSelector;
        private readonly IShopCellsPresenter<CharacterItemData> cellsPresenter;
        private readonly IShopItemsSelectionPresenter<CharacterItemData, CharacterCustomizationView> itemsSelectionPresenter;
        private readonly ICharactersServiceFacade charactersService;
        
        // === FIELDS & PROPERTIES ===
        
        private CharacterShopConfig shopConfig;
        private CharacterCustomizationView targetCharacterCustomization;
        
        private Stack<CharacterItemData> selectedItemPacks = new Stack<CharacterItemData>();
        private CharacterItemData firstSelectedItemPack;
        
        private bool isShopAlreadyRebuild;
        
        // === CONSTRUCTOR ===
        
        public ShopPresenter(
            ICharacterItemsShopView shopView,
            IColorSelectWidget colorSelector,
            IShopCellsPresenter<CharacterItemData> cellsPresenter,
            IShopItemsSelectionPresenter<CharacterItemData, CharacterCustomizationView> itemsSelectionPresenter,
            ICharactersServiceFacade charactersService)
        {
            this.shopView = shopView;
            this.colorSelector = colorSelector;
            this.cellsPresenter = cellsPresenter;
            this.itemsSelectionPresenter = itemsSelectionPresenter;
            this.charactersService = charactersService;
        }
        
        // === INITIALIZATION ===
        
        public UniTask Initialize()
        {
            shopConfig = ConfigsProxy.CharactersAndShopConfig;
            
            targetCharacterCustomization = charactersService.GetCharacter(shopConfig.MainCharacterId);
            
            SubscribeToEvents();
            
            InitializeItemPacks();
            void InitializeItemPacks()
            {
                firstSelectedItemPack = ScriptableObject.CreateInstance<CharacterItemData>();
                firstSelectedItemPack.SetPack(shopConfig.ItemsData);
                selectedItemPacks.Push(firstSelectedItemPack);
            }
            
            itemsSelectionPresenter.UpdateTargetCharacter(targetCharacterCustomization);
            
            InitializeCells();
            void InitializeCells()
            {
                UpdateCellsPresenterState();
                
                cellsPresenter.BuildCells(firstSelectedItemPack.ItemsPack).Forget();
            }

            return UniTask.CompletedTask;
        }
        
        // === EVENT SUBSCRIPTION ===
        
        private void SubscribeToEvents()
        {
            charactersService.OnCharacterGenderSwapped += OnCharacterGenderSwapped;
            colorSelector.onColorUpdate += itemsSelectionPresenter.HandleColorUpdate;
            colorSelector.onColorBuy += itemsSelectionPresenter.HandleColorPurchase;
            shopView.onBuyButtonClick += itemsSelectionPresenter.UnlockItem;
            shopView.onBackButtonClick += ShopBack;
            
            cellsPresenter.OnCellClicked += OnCellClicked;
            itemsSelectionPresenter.OnItemPackSelected += OnItemPackSelected;
            
            if (itemsSelectionPresenter is IShopItemUnlockNotifier unlockNotifier)
                unlockNotifier.OnItemUnlocked += OnItemUnlocked;
        }
        
        // === EVENT HANDLERS ===
        
        private async void OnCellClicked(CharacterItemData itemData)
        {
            await itemsSelectionPresenter.SelectItem(itemData);
            
            UpdateCellsPresenterState();
            UpdateCellsSystemState();
        }
        
        private async void OnItemPackSelected(CharacterItemData[] itemsPack)
        {
            var itemData = ScriptableObject.CreateInstance<CharacterItemData>();
            itemData.SetPack(itemsPack);
            
            selectedItemPacks.Push(itemData);
            await RebuildItemCells(itemsPack);
            shopView.SetBackButtonShow(true);
        }
        
        private void OnItemUnlocked()
        {
            UpdateCellsSystemState();
        }
        
        private void ShopBack()
        {
            const int minPacksCount = 2;
            
            if (isShopAlreadyRebuild)
                return;
            
            itemsSelectionPresenter.ColorRollback();
            itemsSelectionPresenter.RestoreActualCharacterState().Forget();
            
            if (selectedItemPacks.Count >= minPacksCount)
                selectedItemPacks.Pop();
            
            RebuildItemCells(selectedItemPacks.Peek().ItemsPack).Forget();
            shopView.SetBackButtonShow(selectedItemPacks.Count >= minPacksCount);
            
            shopView.HideAllPanels();
            colorSelector.SetPanelShow(false);
        }
        
        private void OnCharacterGenderSwapped(string characterId, CharacterCustomizationView newCharacter)
        {
            if (characterId != shopConfig.MainCharacterId)
                return;
            
            targetCharacterCustomization = newCharacter;
            itemsSelectionPresenter.UpdateTargetCharacter(newCharacter);
            
            UpdateCellsPresenterState();
            
            ResetShop().Forget();
        }
        
        // === CELLS MANAGEMENT ===
        
        private async UniTask RebuildItemCells(CharacterItemData[] items)
        {
            if (isShopAlreadyRebuild)
                return;
            
            isShopAlreadyRebuild = true;
            
            await cellsPresenter.RebuildCells(items);
            
            UpdateCellsSystemState();
            
            isShopAlreadyRebuild = false;
        }
        
        private void UpdateCellsSystemState()
        {
            cellsPresenter.UpdateCellsState();
        }
        
        // === STATE MANAGEMENT ===
        
        private void UpdateCellsPresenterState()
        {
            if(cellsPresenter is ISpecificDataSetter<CharacterItemData> selectedItemSetter)
                selectedItemSetter.SetData(itemsSelectionPresenter.GetCurrentSelectedItem());
        
            if(cellsPresenter is ISpecificDataSetter<CharacterCustomizationView> targetCharacterSetter)
                targetCharacterSetter.SetData(itemsSelectionPresenter.GetTargetCharacter());
        }
        
        // === UTILITY METHODS ===
        
        public async UniTask ResetShop()
        {
            shopView.SetBackButtonShow(false);
            shopView.HideAllPanels();
            colorSelector.SetPanelShow(false);
            
            selectedItemPacks.Clear();
            selectedItemPacks.Push(firstSelectedItemPack);
            
            await RebuildItemCells(selectedItemPacks.Peek().ItemsPack);
        }
        
        // === CLEANUP ===
        
        public async ValueTask DisposeAsync()
        {
            UnsubscribeFromEvents();
            itemsSelectionPresenter.ColorRollback();

            await itemsSelectionPresenter.RestoreActualCharacterState();
            await cellsPresenter.DisposeAsync();

            await UniTask.WaitUntil(() => isShopAlreadyRebuild == false);
        }
        
        private void UnsubscribeFromEvents()
        {
            if (charactersService != null)
                charactersService.OnCharacterGenderSwapped -= OnCharacterGenderSwapped;
            
            if (colorSelector != null)
            {
                colorSelector.onColorUpdate -= itemsSelectionPresenter.HandleColorUpdate;
                colorSelector.onColorBuy -= itemsSelectionPresenter.HandleColorPurchase;
            }
            
            if (shopView != null)
            {
                shopView.onBuyButtonClick -= itemsSelectionPresenter.UnlockItem;
                shopView.onBackButtonClick -= ShopBack;
            }
            
            if (cellsPresenter != null)
                cellsPresenter.OnCellClicked -= OnCellClicked;
                
            if (itemsSelectionPresenter != null)
            {
                itemsSelectionPresenter.OnItemPackSelected -= OnItemPackSelected;
                
                if (itemsSelectionPresenter is IShopItemUnlockNotifier unlockNotifier)
                    unlockNotifier.OnItemUnlocked -= OnItemUnlocked;
            }
        }
    }
} 