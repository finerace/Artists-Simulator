using System.Collections.Generic;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Infrastructure.Main.UI
{
    
    public class ItemsProvider : IItemsProvider
    {
        private CharacterShopConfig shopConfig;
        public ItemsProvider()
        {
            shopConfig = ConfigsProxy.CharactersAndShopConfig;
        }
        
        public CharacterItemData[] GetFilteredItems(CharacterItemData[] items, CharacterGender characterGender)
        {
            Logs.Debug($"GetFilteredItems: {characterGender}");
            
            var filteredItems = new List<CharacterItemData>();
            
            foreach (var itemData in items)
            {
                if (itemData.IsHiddenFromShop)
                    continue;
                
                if (itemData.CharacterGender != CharacterGender.All && 
                    itemData.CharacterGender != characterGender)
                    continue;
                
                filteredItems.Add(itemData);
            }
            
            return filteredItems.ToArray();
        }
    }
} 