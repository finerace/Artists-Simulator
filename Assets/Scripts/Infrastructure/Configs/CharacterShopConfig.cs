using System.Collections.Generic;
using Game.Services.Meta;
using UnityEngine;
using YG;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "CharacterShopConfig", menuName = "Configs/CharacterShopConfig", order = 1)]
    public class CharacterShopConfig : ScriptableObject
    {

        [SerializeField] private float priceMultiplier;
        [SerializeField] private CharacterItemData[] itemsData;
        [SerializeField] private CustomizationTemplateSO[] characterTemplates;

        [Header("Customization Slots")]
        [SerializeField] private SlotTypeDefinition[] slotTypeDefinitions;
        
        [Header("Character Generation Settings")]
        [Tooltip("Chance of generating completely random colors (0-1)")]
        [Range(0, 1)]
        [SerializeField] private float randomColorsChance = 0.05f;
        
        [Tooltip("Chance of generating monochromatic theme (0-1)")]
        [Range(0, 1)]
        [SerializeField] private float monochromaticThemeChance = 0.1f;
        
        [Tooltip("Chance of item appearing in removable slot (0-1)")]
        [Range(0, 1)]
        [SerializeField] private float slotItemSpawnChance = 0.5f;
        
        [Tooltip("Gradient for generating item colors")]
        [SerializeField] private Gradient itemsColorGradient;
        
        [Header("Monochromatic Colors Settings")]
        [Tooltip("Saturation change range for monochromatic colors")]
        [Range(0, 0.5f)]
        [SerializeField] private float monochromaticSaturationRange = 0.2f;
        
        [Tooltip("Brightness change range for monochromatic colors")]
        [Range(0, 0.5f)]
        [SerializeField] private float monochromaticBrightnessRange = 0.3f;
        
        [Header("Color Gradient Settings")]
        [Tooltip("Hair start position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float hairGradientStart = 0.0f;
        
        [Tooltip("Hair end position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float hairGradientEnd = 0.33f;
        
        [Tooltip("Skin start position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float skinGradientStart = 0.33f;
        
        [Tooltip("Skin end position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float skinGradientEnd = 0.66f;
        
        [Tooltip("Eyes start position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float eyeGradientStart = 0.66f;
        
        [Tooltip("Eyes end position in gradient")]
        [Range(0, 1)]
        [SerializeField] private float eyeGradientEnd = 1.0f;
        
        [Tooltip("Number of color keys for each category")]
        [Range(1, 5)]
        [SerializeField] private int gradientKeysCount = 2;
        
        [Header("Gradient Constants")]
        [Tooltip("Alpha value for gradient (usually 1.0)")]
        [Range(0, 1)]
        [SerializeField] private float gradientAlphaValue = 1.0f;
        
        [Header("Slot Constants")]
        [Tooltip("Hair slot name")]
        [SerializeField] private string hairSlotId = "hair";
        
        [Tooltip("Skin slot name")]
        [SerializeField] private string skinSlotId = "skin";
        
        [Tooltip("Eyes slot name")]
        [SerializeField] private string eyeSlotId = "eye";
        
        [Header("Natural Colors - Hair")]
        [SerializeField] private ColorWithWeight[] hairColors = new ColorWithWeight[]
        {
            new ColorWithWeight { color = new Color(0.05f, 0.05f, 0.05f), weight = 15 },  // Black
            new ColorWithWeight { color = new Color(0.2f, 0.1f, 0.05f), weight = 20 },    // Dark brown
            new ColorWithWeight { color = new Color(0.3f, 0.2f, 0.1f), weight = 25 },     // Brown
            new ColorWithWeight { color = new Color(0.6f, 0.4f, 0.2f), weight = 20 },     // Light brown
            new ColorWithWeight { color = new Color(0.8f, 0.7f, 0.4f), weight = 15 },     // Blonde
            new ColorWithWeight { color = new Color(0.7f, 0.3f, 0.2f), weight = 3 },      // Red
            new ColorWithWeight { color = new Color(0.5f, 0.5f, 0.5f), weight = 2 }       // Gray
        };
        
        [Header("Natural Colors - Skin")]
        [SerializeField] private ColorWithWeight[] skinColors = new ColorWithWeight[]
        {
            new ColorWithWeight { color = new Color(1.0f, 0.87f, 0.73f), weight = 10 },    // Light
            new ColorWithWeight { color = new Color(0.94f, 0.78f, 0.66f), weight = 10 },   // Medium light
            new ColorWithWeight { color = new Color(0.8f, 0.64f, 0.52f), weight = 10 },    // Olive
            new ColorWithWeight { color = new Color(0.65f, 0.48f, 0.35f), weight = 10 },   // Medium dark
            new ColorWithWeight { color = new Color(0.4f, 0.26f, 0.13f), weight = 10 }     // Dark
        };
        
        [Header("Natural Colors - Eyes")]
        [SerializeField] private ColorWithWeight[] eyeColors = new ColorWithWeight[]
        {
            new ColorWithWeight { color = new Color(0.3f, 0.2f, 0.1f), weight = 35 },      // Brown
            new ColorWithWeight { color = new Color(0.5f, 0.35f, 0.2f), weight = 25 },     // Light brown
            new ColorWithWeight { color = new Color(0.3f, 0.5f, 0.7f), weight = 20 },      // Blue
            new ColorWithWeight { color = new Color(0.2f, 0.4f, 0.2f), weight = 15 },      // Green
            new ColorWithWeight { color = new Color(0.4f, 0.3f, 0.5f), weight = 5 }        // Purple/unusual
        };

        [Space] 
        
        [SerializeField] private string mainCharacterId = "Main";

        [Header("Main Character Templates")]
        [SerializeField] private string maleTemplateId;
        [SerializeField] private string femaleTemplateId;
        public string MaleTemplateId => maleTemplateId;
        public string FemaleTemplateId => femaleTemplateId;
        
        private Dictionary<string, CharacterItemData> itemsDatabase;
        private Dictionary<ItemType, List<string>> slotIdsByType;

        public IReadOnlyDictionary<string, CharacterItemData> ItemsDatabase => itemsDatabase;

        public CharacterItemData[] ItemsData => itemsData;
        public CustomizationTemplateSO[] CharacterTemplates => characterTemplates;

        public string MainCharacterId => mainCharacterId;
        public float RandomColorsChance => randomColorsChance;
        public float MonochromaticThemeChance => monochromaticThemeChance;
        public float SlotItemSpawnChance => slotItemSpawnChance;
        public Gradient ItemsColorGradient => itemsColorGradient;
        public ColorWithWeight[] HairColors => hairColors;
        public ColorWithWeight[] SkinColors => skinColors;
        public ColorWithWeight[] EyeColors => eyeColors;
        
        public float MonochromaticSaturationRange => monochromaticSaturationRange;
        public float MonochromaticBrightnessRange => monochromaticBrightnessRange;
        public float HairGradientStart => hairGradientStart;
        public float HairGradientEnd => hairGradientEnd;
        public float SkinGradientStart => skinGradientStart;
        public float SkinGradientEnd => skinGradientEnd;
        public float EyeGradientStart => eyeGradientStart;
        public float EyeGradientEnd => eyeGradientEnd;
        public int GradientKeysCount => gradientKeysCount;
        
        public float GradientAlphaValue => gradientAlphaValue;
        
        public string HairSlotId => hairSlotId;
        public string SkinSlotId => skinSlotId;
        public string EyeSlotId => eyeSlotId;
        
        public Color GetRandomHairColor()
        {
            return GetRandomWeightedColor(hairColors);
        }
        
        public Color GetRandomSkinColor()
        {
            return GetRandomWeightedColor(skinColors);
        }
        
        public Color GetRandomEyeColor()
        {
            return GetRandomWeightedColor(eyeColors);
        }
        
        private Color GetRandomWeightedColor(ColorWithWeight[] colorPalette)
        {
            if (colorPalette == null || colorPalette.Length == 0)
                return Color.white;
                
            if (colorPalette.Length == 1)
                return colorPalette[0].color;
                
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[colorPalette.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
            
            float totalWeight = 0;
            foreach (var colorWithWeight in colorPalette)
                totalWeight += colorWithWeight.weight;
                
            float currentPosition = 0;
            for (int i = 0; i < colorPalette.Length; i++)
            {
                float normalizedWeight = colorPalette[i].weight / totalWeight;
                
                float colorPosition = currentPosition + (normalizedWeight / 2);
                
                colorPosition = Mathf.Clamp01(colorPosition);
                
                colorKeys[i] = new GradientColorKey(colorPalette[i].color, colorPosition);
                
                currentPosition += normalizedWeight;
            }
            
            gradient.SetKeys(colorKeys, alphaKeys);
            
            float randomValue = Random.value;
            
            return gradient.Evaluate(randomValue);
        }
        
        public void InitializeItemsDatabase()
        {
            itemsDatabase = new Dictionary<string, CharacterItemData>();
            
            FillItemsDatabase(itemsData);
            void FillItemsDatabase(CharacterItemData[] characterItemData)
            {
                var itemsPacks = new List<CharacterItemData>();
                
                foreach (var item in characterItemData)
                {
                    if (item == null || string.IsNullOrEmpty(item.ItemId))
                        continue;
                        
                    if (!item.IsItemsPack)
                    {
                        itemsDatabase[item.ItemId] = item;
                    }
                    else
                    {
                        itemsPacks.Add(item);
                    }
                }

                foreach (var pack in itemsPacks)
                {
                    if (pack.ItemsPack != null)
                    {
                        FillItemsDatabase(pack.ItemsPack);
                    }
                }
            }
        }
        
        public CharacterItemData[] GetAllItemsArray()
        {
            var resultItems = new List<CharacterItemData>();
            
            SearchItems(itemsData);
            void SearchItems(CharacterItemData[] characterItemData)
            {
                var itemsPacks = new List<CharacterItemData>();
                
                foreach (var item in characterItemData)
                {
                    if (!item.IsItemsPack)
                        resultItems.Add(item);
                    else
                        itemsPacks.Add(item);
                }

                foreach (var item in itemsPacks)
                    SearchItems(item.ItemsPack);
            }

            return resultItems.ToArray();
        }
        
        public List<CharacterItemData> GetItemsForSlot(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
                return new List<CharacterItemData>();
                
            var result = new List<CharacterItemData>();
            var allItems = GetAllItemsArray();
            
            foreach (var item in allItems)
            {
                if (item != null && item.SlotId == slotId && !item.IsItemsPack)
                {
                    result.Add(item);
                }
            }
            
            return result;
        }
        
        public CustomizationTemplateSO GetTemplateById(string templateId)
        {
            if (string.IsNullOrEmpty(templateId) || characterTemplates == null)
                return null;
            foreach (var template in characterTemplates)
            {
                if (template != null && template.TemplateId == templateId)
                    return template;
            }
            return null;
        }
        
        public CharacterItemData GetItemById(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;
                
            return itemsDatabase.TryGetValue(itemId, out var item) ? item : null;
        }
        
        public string[] GetSlotIdsByType(ItemType slotType)
        {
            if (slotTypeDefinitions == null || slotTypeDefinitions.Length == 0)
                return new string[0];

            foreach (var definition in slotTypeDefinitions)
            {
                if (definition.SlotType == slotType && definition.SlotIds != null)
                {
                    return definition.SlotIds;
                }
            }
            
            return new string[0];
        }


    }

    [System.Serializable]
    public class SlotTypeDefinition
    {
        [SerializeField] private ItemType slotType;
        [SerializeField] private string[] slotIds;

        public ItemType SlotType { get => slotType; set => slotType = value; }
        public string[] SlotIds { get => slotIds; set => slotIds = value; }
    }
    
    [System.Serializable]
    public class ColorWithWeight
    {
        public Color color;
        public int weight = 10;
    }
}
