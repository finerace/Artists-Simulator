using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using YG;
using Zenject;

namespace Game.Services.Meta
{
    
    public class CharactersServiceFacade : ICharactersServiceFacade
    {
        private readonly ICharacterStorageService storageService;
        private readonly ICharacterCreationService creationService;
        private readonly ICharacterCustomizationService customizationService;
        private readonly ICharacterGenderService genderService;

        public event Action<string, CharacterCustomizationView> OnCharacterGenderSwapped
        {
            add => genderService.OnCharacterGenderSwapped += value;
            remove => genderService.OnCharacterGenderSwapped -= value;
        }
        
        [Inject]
        public CharactersServiceFacade(
            ICharacterStorageService storageService,
            ICharacterCreationService creationService,
            ICharacterCustomizationService customizationService,
            ICharacterGenderService genderService)
        {
            this.storageService = storageService;
            this.creationService = creationService;
            this.customizationService = customizationService;
            this.genderService = genderService;
        }

        public async UniTask<CharacterCustomizationView> CreateCharacter(CustomizationTemplateSO template, string characterId)
        {
            Logs.Info($"Creating character {characterId} with template {template?.name}");
            return await creationService.CreateCharacter(template, characterId);
        }
        
        public CharacterCustomizationView GetCharacter(string characterId)
        {
            return storageService.GetCharacter(characterId);
        }
        
        public async UniTask DestroyCharacter(string characterId)
        {
            Logs.Info($"Destroying character {characterId}");
            await creationService.DestroyCharacter(characterId);
        }
        
        public async UniTask ApplySlotDataToCharacter(CharacterCustomizationView character, List<SavesYG.CharacterSlotData> slotData)
        {
            Logs.Info($"Applying slot data to character {character?.name}");
            await customizationService.ApplySlotDataToCharacter(character, slotData);
        }
        
        public CustomizationTemplateSO GetRandomTemplate(CharacterGender gender)
        {
            return creationService.GetRandomTemplate(gender);
        }
        
        public async UniTask ApplyRandomItemsToCharacter(CharacterCustomizationView character)
        {
            Logs.Info($"Applying random items to character {character?.name}");
            await customizationService.ApplyRandomItemsToCharacter(character);
        }
        
        public async UniTask<CharacterCustomizationView> SwapGender(string characterId)
        {
            Logs.Info($"Swapping gender for character {characterId}");
            return await genderService.SwapGender(characterId);
        }
    }
}