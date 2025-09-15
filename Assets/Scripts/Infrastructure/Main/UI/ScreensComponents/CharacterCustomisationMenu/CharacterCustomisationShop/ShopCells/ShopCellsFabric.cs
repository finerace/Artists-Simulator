using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI
{
    
    public class ShopCellsFabric : IShopCellsFabric
    {
        // === DEPENDENCIES ===
        
        private readonly IAssetsService assetsService;
        
        // === CONSTRUCTOR ===
        
        public ShopCellsFabric(IAssetsService assetsService)
        {
            this.assetsService = assetsService;
        }
        
        // === CELL CREATION ===
        
        public async UniTask<IShopCellView<CharacterItemData>> CreateCell(CharacterItemData itemData, Transform parent)
        {
            var cellView = await assetsService.GetAsset<ShopCellView>
                (ConfigsProxy.AssetsPathsConfig.CustomisationMenuShopCellID);
            
            if (cellView == null)
            {
                Debug.LogError($"Cell prefab doesn't have {nameof(ShopCellView)} component!");
                
                return null;
            }
            
            cellView.transform.SetParent(parent, false);
            await cellView.SetNewItem(itemData);
            
            await SetupCellIcon(cellView, itemData);
            
            return cellView;
        }
        
        // === CELL DESTRUCTION ===
        
        public void DestroyCell(IShopCellView<CharacterItemData> cellView)
        {
            if (cellView?.CellTransform?.gameObject == null)
                return;
                
            assetsService.ReleaseAsset(cellView.CellTransform.gameObject);
        }
        
        // === CELL SETUP ===
        
        private async UniTask SetupCellIcon(ShopCellView cellView, CharacterItemData itemData)
        {
            if (itemData.IsIconObject)
            {
                await Setup3DIcon(cellView, itemData);
            }
            else
            {
                Setup2DIcon(cellView, itemData);
            }
        }
        
        private async UniTask Setup3DIcon(ShopCellView cellView, CharacterItemData itemData)
        {
            if (string.IsNullOrEmpty(itemData.ItemObjectAddressableId))
            {
                Debug.LogWarning($"Item marked as IconObject but has no AddressableId: {itemData.ItemId}");
                return;
            }

            var iconObject = await assetsService.GetAsset<Transform>(itemData.ItemObjectAddressableId);
            
            if (iconObject == null)
            {
                Debug.LogError($"Failed to load 3D icon for item: {itemData.ItemId}");
                return;
            }
            
            cellView.SetIcon3D(iconObject, itemData.IconObjectLocalPos, itemData.IconObjectScale);
        }
        
        private void Setup2DIcon(ShopCellView cellView, CharacterItemData itemData)
        {
            if (itemData.ItemIcon == null)
            {
                Debug.LogWarning($"Item has no 2D icon: {itemData.ItemId}");
                return;
            }
            
            cellView.SetIcon2D(itemData.ItemIcon);
        }
    }
} 