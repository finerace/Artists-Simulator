using Game.Services.Meta;

namespace Game.Infrastructure.Main.UI
{
    public interface IItemsProvider
    {
        CharacterItemData[] GetFilteredItems(CharacterItemData[] items, CharacterGender characterGender);
    }
} 