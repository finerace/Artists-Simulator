using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using UnityEngine;
using YG;
using Random = UnityEngine.Random;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Services.Meta
{
    public class CharacterCustomizationService : ICharacterCustomizationService
    {
        public async UniTask ApplySlotDataToCharacter(CharacterCustomizationView character, List<SavesYG.CharacterSlotData> slotData)
        {
            if (character == null)
            {
                Logs.Warning("CharacterCustomizationService: character is null");
                return;
            }
            
            if (slotData == null)
            {
                Logs.Warning("CharacterCustomizationService: slotData is null");
                return;
            }
                
            await character.SetSlotsSavedData(slotData);
        }

        public async UniTask ApplyRandomItemsToCharacter(CharacterCustomizationView character)
        {
            if (character == null)
            {
                Logs.Warning("CharacterCustomizationService: character is null");
                return;
            }
            
            if (character.CustomizationTemplate == null)
            {
                Logs.Warning("CharacterCustomizationService: character CustomizationTemplate is null");
                return;
            }
            
            var characterConfig = ConfigsProxy.CharactersAndShopConfig;
            if (characterConfig == null)
            {
                Logs.Error("CharacterCustomizationService: CharactersAndShopConfig is null");
                return;
            }
            
            bool useMonochromaticTheme = Random.value < characterConfig.MonochromaticThemeChance;
            bool useRandomColors = Random.value < characterConfig.RandomColorsChance;
            
            Color monochromaticBaseColor = new Color(Random.value, Random.value, Random.value);
            
            foreach (var slot in character.CustomizationTemplate.CustomizationSlots)
            {
                if (slot == null)
                    continue;
                    
                if (slot.IsRemovable && Random.value > characterConfig.SlotItemSpawnChance)
                    continue;
                    
                var availableItems = characterConfig.GetItemsForSlot(slot.SlotId);
                if (availableItems == null || availableItems.Count == 0)
                    continue;
                    
                var randomIndex = Random.Range(0, availableItems.Count);
                var randomItem = availableItems[randomIndex];
                
                if (randomItem.CharacterGender != CharacterGender.All && 
                    randomItem.CharacterGender != character.CustomizationTemplate.CharacterGender)
                    continue;
                    
                await character.ApplyItemToSlot(randomItem);
                
                if (randomItem.IsCanColorize)
                {
                    Color colorToApply;
                    
                    switch (slot.SlotId.ToLower())
                    {
                        case var hairSlot when hairSlot == characterConfig.HairSlotId:
                            colorToApply = characterConfig.GetRandomHairColor();
                            break;
                        case var skinSlot when skinSlot == characterConfig.SkinSlotId:
                            colorToApply = characterConfig.GetRandomSkinColor();
                            break;
                        case var eyeSlot when eyeSlot == characterConfig.EyeSlotId:
                            colorToApply = characterConfig.GetRandomEyeColor();
                            break;
                        default:
                            if (useRandomColors)
                            {
                                colorToApply = new Color(Random.value, Random.value, Random.value);
                            }
                            else if (useMonochromaticTheme)
                            {
                                colorToApply = GetMonochromaticVariation(monochromaticBaseColor);
                            }
                            else
                            {
                                Gradient colorGradient = new Gradient();
                                
                                List<GradientColorKey> colorKeysList = new List<GradientColorKey>();
                                
                                AddColorKeys(colorKeysList, characterConfig.HairColors, characterConfig.HairGradientStart, characterConfig.HairGradientEnd, characterConfig.GradientKeysCount);
                                
                                AddColorKeys(colorKeysList, characterConfig.SkinColors, characterConfig.SkinGradientStart, characterConfig.SkinGradientEnd, characterConfig.GradientKeysCount);
                                
                                AddColorKeys(colorKeysList, characterConfig.EyeColors, characterConfig.EyeGradientStart, characterConfig.EyeGradientEnd, characterConfig.GradientKeysCount);
                                
                                GradientColorKey[] colorKeys = colorKeysList.ToArray();
                                
                                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                                alphaKeys[0] = new GradientAlphaKey(characterConfig.GradientAlphaValue, 0.0f);
                                alphaKeys[1] = new GradientAlphaKey(characterConfig.GradientAlphaValue, 1.0f);
                                
                                colorGradient.SetKeys(colorKeys, alphaKeys);
                                
                                colorToApply = colorGradient.Evaluate(Random.value);
                            }
                            
                            void AddColorKeys(List<GradientColorKey> keysList, ColorWithWeight[] palette, float startPos, float endPos, int count)
                            {
                                if (palette == null || palette.Length == 0)
                                    return;
                                
                                float totalWeight = 0;
                                foreach (var colorWithWeight in palette)
                                    totalWeight += colorWithWeight.weight;
                                
                                for (int i = 0; i < count && i < palette.Length; i++)
                                {
                                    float randomValue = Random.Range(0, totalWeight);
                                    float currentWeight = 0;
                                    int selectedIndex = 0;
                                    
                                    for (int j = 0; j < palette.Length; j++)
                                    {
                                        currentWeight += palette[j].weight;
                                        if (randomValue <= currentWeight)
                                        {
                                            selectedIndex = j;
                                            break;
                                        }
                                    }
                                    
                                    float position = startPos + ((endPos - startPos) * (float)i / count);
                                    
                                    keysList.Add(new GradientColorKey(palette[selectedIndex].color, position));
                                }
                            }
                            break;
                    }
                    
                    character.SetSlotColor(slot.SlotId, colorToApply);
                }
            }
        }

        private Color GetMonochromaticVariation(Color baseColor)
        {
            var characterConfig = ConfigsProxy.CharactersAndShopConfig;
            if (characterConfig == null)
            {
                Logs.Error("CharacterCustomizationService: CharactersAndShopConfig is null in GetMonochromaticVariation");
                return baseColor;
            }
            
            float h, s, v;
            Color.RGBToHSV(baseColor, out h, out s, out v);
            
            s = Mathf.Clamp01(s + Random.Range(-characterConfig.MonochromaticSaturationRange, characterConfig.MonochromaticSaturationRange));
            v = Mathf.Clamp01(v + Random.Range(-characterConfig.MonochromaticBrightnessRange, characterConfig.MonochromaticBrightnessRange));
            
            return Color.HSVToRGB(h, s, v);
        }
    }
} 