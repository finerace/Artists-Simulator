using System.Collections.Generic;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    public class CharacterStorageService : ICharacterStorageService
    {
        private readonly Dictionary<string, CharacterCustomizationView> characterList = new Dictionary<string, CharacterCustomizationView>();

        public CharacterCustomizationView GetCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterStorageService: characterId is null or empty in GetCharacter");
                return null;
            }
            
            if (!characterList.ContainsKey(characterId))
            {
                Logs.Warning($"CharacterStorageService: character not found: {characterId}");
                return null;
            }
                
            return characterList[characterId];
        }
        
        public bool HasCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;
                
            return characterList.ContainsKey(characterId);
        }

        public void RegisterCharacter(string characterId, CharacterCustomizationView character)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterStorageService: characterId is null or empty in RegisterCharacter");
                return;
            }
            
            if (character == null)
            {
                Logs.Warning($"CharacterStorageService: character is null for id: {characterId}");
                return;
            }

            if (characterList.ContainsKey(characterId))
            {
                Logs.Warning($"CharacterStorageService: overwriting existing character: {characterId}");
            }
                
            characterList[characterId] = character;
        }

        public void UnregisterCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterStorageService: characterId is null or empty in UnregisterCharacter");
                return;
            }

            if (!characterList.ContainsKey(characterId))
            {
                Logs.Warning($"CharacterStorageService: character not found for removal: {characterId}");
                return;
            }
                
            characterList.Remove(characterId);
        }

        public IReadOnlyDictionary<string, CharacterCustomizationView> GetAllCharacters()
        {
            return characterList;
        }
    }
} 