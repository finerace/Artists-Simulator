using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using YG;

namespace Game.Services.Meta
{
    public interface ICharacterCustomizationService
    {
        UniTask ApplySlotDataToCharacter(CharacterCustomizationView character, List<SavesYG.CharacterSlotData> slotData);
        UniTask ApplyRandomItemsToCharacter(CharacterCustomizationView character);
    }
} 