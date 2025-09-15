using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Random = UnityEngine.Random;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    public class CharacterCreationService : ICharacterCreationService
    {
        private readonly IAssetsService assetsService;
        private readonly ICharacterStorageService storageService;
        
        public CharacterCreationService(IAssetsService assetsService, ICharacterStorageService storageService)
        {
            this.assetsService = assetsService;
            this.storageService = storageService;
        }

        public async UniTask<CharacterCustomizationView> CreateCharacter(CustomizationTemplateSO template, string characterId)
        {
            if (template == null)
            {
                Logs.Warning("CharacterCreationService: template is null");
                return null;
            }
            
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterCreationService: characterId is null or empty");
                return null;
            }
                
            if (storageService.HasCharacter(characterId))
                DestroyCharacter(characterId);
            
            string assetPath = template.CharacterModelAddressableId;
            if (string.IsNullOrEmpty(assetPath))
            {
                Logs.Warning($"CharacterCreationService: empty asset path for template {template.name}");
                return null;
            }
                
            var character = await assetsService.GetAsset<CharacterCustomizationView>(assetPath);
            
            if (character == null)
            {
                Logs.Error($"CharacterCreationService: failed to load asset {assetPath}");
                return null;
            }
            
            storageService.RegisterCharacter(characterId, character);
            
            await character.ApplyDefaultItems();
            
            return character;
        }

        public UniTask DestroyCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterCreationService: characterId is null or empty in DestroyCharacter");
                return UniTask.CompletedTask;
            }
                
            var character = storageService.GetCharacter(characterId);
            if (character != null)
            {
                assetsService.ReleaseAsset(character.gameObject);
            }
            
            storageService.UnregisterCharacter(characterId);
            
            return UniTask.CompletedTask;
        }

        public CustomizationTemplateSO GetRandomTemplate(CharacterGender gender)
        {
            var config = ConfigsProxy.CharactersAndShopConfig;
            if (config?.CharacterTemplates == null)
            {
                Logs.Error("CharacterCreationService: CharacterTemplates config is null");
                return null;
            }

            var templates = config.CharacterTemplates;
            if (templates.Length == 0)
            {
                Logs.Warning("CharacterCreationService: no character templates found");
                return null;
            }
                
            var matchingTemplates = new List<CustomizationTemplateSO>();
            
            foreach (var template in templates)
            {
                if (template != null && (template.CharacterGender == gender || template.CharacterGender == CharacterGender.All))
                {
                    matchingTemplates.Add(template);
                }
            }
            
            if (matchingTemplates.Count == 0)
            {
                Logs.Warning($"CharacterCreationService: no templates found for gender {gender}");
                return null;
            }
                
            return matchingTemplates[Random.Range(0, matchingTemplates.Count)];
        }
    }
} 