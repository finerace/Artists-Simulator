using System.Collections.Generic;

namespace Game.Services.Meta
{
    public interface ICharacterStorageService
    {
        CharacterCustomizationView GetCharacter(string characterId);
        bool HasCharacter(string characterId);
        void RegisterCharacter(string characterId, CharacterCustomizationView character);
        void UnregisterCharacter(string characterId);
        IReadOnlyDictionary<string, CharacterCustomizationView> GetAllCharacters();
    }
} 