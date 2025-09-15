using System;
using Cysharp.Threading.Tasks;

namespace Game.Services.Meta
{
    public interface ICharacterGenderService
    {
        UniTask<CharacterCustomizationView> SwapGender(string characterId);
        event Action<string, CharacterCustomizationView> OnCharacterGenderSwapped;
    }
} 