using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YG;

namespace Game.Services.Meta
{
    public interface ICharactersServiceFacade
    {
        event Action<string, CharacterCustomizationView> OnCharacterGenderSwapped;
        
        UniTask<CharacterCustomizationView> CreateCharacter(CustomizationTemplateSO template, string characterId);
        CharacterCustomizationView GetCharacter(string characterId);
        UniTask DestroyCharacter(string characterId);
        UniTask ApplySlotDataToCharacter(CharacterCustomizationView character, List<SavesYG.CharacterSlotData> slotData);
        CustomizationTemplateSO GetRandomTemplate(CharacterGender gender);
        UniTask ApplyRandomItemsToCharacter(CharacterCustomizationView character);
        UniTask<CharacterCustomizationView> SwapGender(string characterId);
    }
} 