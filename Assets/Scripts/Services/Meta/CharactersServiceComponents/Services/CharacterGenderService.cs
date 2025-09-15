using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using UnityEngine;
using YG;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{   
    
    public class CharacterGenderService : ICharacterGenderService
    {
        // === DEPENDENCIES ===
        private readonly ICharacterCreationService creationService;
        private readonly ICharacterStorageService storageService;
        
        // === STATE ===
        private readonly Dictionary<string, UniTask<CharacterCustomizationView>> swapOperations = new Dictionary<string, UniTask<CharacterCustomizationView>>();

        // === EVENTS ===
        public event Action<string, CharacterCustomizationView> OnCharacterGenderSwapped;

        // === CONSTRUCTOR ===
        public CharacterGenderService(
            ICharacterCreationService creationService,
            ICharacterStorageService storageService)
        {
            this.creationService = creationService;
            this.storageService = storageService;
        }
        
        // === PUBLIC API ===
        public async UniTask<CharacterCustomizationView> SwapGender(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Logs.Warning("CharacterGenderService: characterId is null or empty");
                return null;
            }

            if (swapOperations.TryGetValue(characterId, out var existingOperation))
            {
                return await existingOperation;
            }
            
            var swapTask = ExecuteGenderSwapPipeline(characterId);
            swapOperations[characterId] = swapTask;

            var result = await swapTask;
            swapOperations.Remove(characterId);
            
            return result;
            
            async UniTask<CharacterCustomizationView> ExecuteGenderSwapPipeline(string characterId)
            {
                var oldCharacter = GetExistingCharacter(characterId);
                CharacterCustomizationView GetExistingCharacter(string characterId)
                {
                    var character = storageService.GetCharacter(characterId);
                    if (character?.CustomizationTemplate == null)
                    {
                        Logs.Warning($"CharacterGenderService: character or template is null for {characterId}");
                        return null;
                    }
                    return character;
                }
                
                if (oldCharacter == null) return null;

                var newGender = DetermineNewGender(oldCharacter);
                CharacterGender DetermineNewGender(CharacterCustomizationView character)
                {
                    var currentGender = character.CustomizationTemplate.CharacterGender;
                    return currentGender == CharacterGender.Male ? CharacterGender.Female : CharacterGender.Male;
                }

                var newTemplate = GetTemplateForGender(newGender);
                CustomizationTemplateSO GetTemplateForGender(CharacterGender gender)
                {
                    var config = ConfigsProxy.CharactersAndShopConfig;
                    if (config == null)
                    {
                        Logs.Error("CharacterGenderService: CharactersAndShopConfig is null");
                        return null;
                    }

                    string templateId = gender == CharacterGender.Male ? config.MaleTemplateId : config.FemaleTemplateId;
                    var template = config.GetTemplateById(templateId);
                    
                    if (template == null)
                    {
                        Logs.Warning($"CharacterGenderService: template not found for {templateId}");
                    }
                    
                    return template;
                }

                if (newTemplate == null) return oldCharacter;
                
                var newCharacter = await CreateNewCharacterWithTemplate(characterId, newTemplate);
                async UniTask<CharacterCustomizationView> CreateNewCharacterWithTemplate(string characterId, 
                    CustomizationTemplateSO template)
                {
                    string tempCharacterId = $"{characterId}_swapping_temp";
                    var newCharacter = await creationService.CreateCharacter(template, tempCharacterId);
                    
                    if (newCharacter == null)
                    {
                        Logs.Error($"CharacterGenderService: failed to create new character for {characterId}");
                    }
                    
                    return newCharacter;
                }

                if (newCharacter == null) return oldCharacter;

                var characterBackup = CreateCharacterBackup(oldCharacter);
                CharacterBackupData CreateCharacterBackup(CharacterCustomizationView character)
                {
                    if (character == null)
                    {
                        Logs.Warning("CharacterGenderService: character is null in CreateCharacterBackup");
                        return null;
                    }
                    
                    var transform = character.transform;
                    return new CharacterBackupData
                    {
                        Slots = character.GetSlotsData(),
                        Parent = transform.parent,
                        LocalPosition = transform.localPosition,
                        LocalRotation = transform.localRotation,
                        LocalScale = transform.localScale,
                        Layer = character.gameObject.layer
                    };
                }
                
                await ReplaceCharacterAtomically(characterId, newCharacter, characterBackup);
                async UniTask ReplaceCharacterAtomically(string characterId, CharacterCustomizationView newCharacter, 
                    CharacterBackupData backup)
                {
                    if (backup == null)
                    {
                        Logs.Error("CharacterGenderService: backup is null in ReplaceCharacterAtomically");
                        return;
                    }

                    string tempCharacterId = $"{characterId}_swapping_temp";
                    
                    storageService.UnregisterCharacter(tempCharacterId);
                    creationService.DestroyCharacter(characterId);
                    storageService.RegisterCharacter(characterId, newCharacter);
                    
                    await RestoreCharacterState(newCharacter, backup);
                    async UniTask RestoreCharacterState(CharacterCustomizationView character, CharacterBackupData backup)
                    {
                        if (character == null)
                        {
                            Logs.Warning("CharacterGenderService: character is null in RestoreCharacterState");
                            return;
                        }

                        if (backup == null)
                        {
                            Logs.Warning("CharacterGenderService: backup is null in RestoreCharacterState");
                            return;
                        }

                        RestoreTransformData(character, backup);
                        void RestoreTransformData(CharacterCustomizationView character, CharacterBackupData backup)
                        {
                            character.transform.SetParent(backup.Parent);
                            character.transform.localPosition = backup.LocalPosition;
                            character.transform.localRotation = backup.LocalRotation;
                            character.transform.localScale = backup.LocalScale;
                            SetLayerRecursively(character.gameObject, backup.Layer);
                        }
                        
                        void SetLayerRecursively(GameObject obj, int layer)
                        {
                            if (obj == null)
                            {
                                Logs.Warning("CharacterGenderService: GameObject is null for layer setting");
                                return;
                            }
                            
                            obj.layer = layer;
                            
                            foreach (Transform child in obj.transform)
                            {
                                SetLayerRecursively(child.gameObject, layer);
                            }
                        }

                        await character.SetSlotsSavedData(backup.Slots);
                    }
                }
                
                OnCharacterGenderSwapped?.Invoke(characterId, newCharacter);
                return newCharacter;
            }
        }

        // === DATA STRUCTURES ===
        private class CharacterBackupData
        {
            public List<SavesYG.CharacterSlotData> Slots;
            public Transform Parent;
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public Vector3 LocalScale;
            public int Layer;
        }
    }
} 