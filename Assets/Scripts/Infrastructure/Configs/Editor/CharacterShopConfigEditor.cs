using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Game.Services.Meta;
using Game.Services.Meta.Editor;

namespace Game.Infrastructure.Configs.Editor
{
    [CustomEditor(typeof(CharacterShopConfig))]
    public class CharacterShopConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty priceMultiplier;
        private SerializedProperty itemsData;
        private SerializedProperty characterTemplates;
        private SerializedProperty availableSlotIds;
        private SerializedProperty slotTypeDefinitions;
        private SerializedProperty mainCharacterId;
        private SerializedProperty maleTemplateId;
        private SerializedProperty femaleTemplateId;
        
        // Settings for color generation
        private SerializedProperty randomColorsChance;
        private SerializedProperty monochromaticThemeChance;
        private SerializedProperty slotItemSpawnChance;
        private SerializedProperty hairColors;
        private SerializedProperty skinColors;
        private SerializedProperty eyeColors;
        
        // Settings for monochromatic colors
        private SerializedProperty monochromaticSaturationRange;
        private SerializedProperty monochromaticBrightnessRange;
        
        // Settings for gradient colors
        private SerializedProperty hairGradientStart;
        private SerializedProperty hairGradientEnd;
        private SerializedProperty skinGradientStart;
        private SerializedProperty skinGradientEnd;
        private SerializedProperty eyeGradientStart;
        private SerializedProperty eyeGradientEnd;
        private SerializedProperty gradientKeysCount;
        
        // Constants for gradient and slots
        private SerializedProperty gradientAlphaValue;
        private SerializedProperty hairSlotId;
        private SerializedProperty skinSlotId;
        private SerializedProperty eyeSlotId;
        
        private ReorderableList[] slotTypeDefinitionLists;
        private ReorderableList hairColorsList;
        private ReorderableList skinColorsList;
        private ReorderableList eyeColorsList;
        
        private void OnEnable()
        {
            priceMultiplier = serializedObject.FindProperty("priceMultiplier");
            itemsData = serializedObject.FindProperty("itemsData");
            characterTemplates = serializedObject.FindProperty("characterTemplates");
            availableSlotIds = serializedObject.FindProperty("availableSlotIds");
            slotTypeDefinitions = serializedObject.FindProperty("slotTypeDefinitions");
            mainCharacterId = serializedObject.FindProperty("mainCharacterId");
            maleTemplateId = serializedObject.FindProperty("maleTemplateId");
            femaleTemplateId = serializedObject.FindProperty("femaleTemplateId");
            
            // Find properties for color generation settings
            randomColorsChance = serializedObject.FindProperty("randomColorsChance");
            monochromaticThemeChance = serializedObject.FindProperty("monochromaticThemeChance");
            slotItemSpawnChance = serializedObject.FindProperty("slotItemSpawnChance");
            hairColors = serializedObject.FindProperty("hairColors");
            skinColors = serializedObject.FindProperty("skinColors");
            eyeColors = serializedObject.FindProperty("eyeColors");
            
            // Find properties for monochromatic color settings
            monochromaticSaturationRange = serializedObject.FindProperty("monochromaticSaturationRange");
            monochromaticBrightnessRange = serializedObject.FindProperty("monochromaticBrightnessRange");
            
            // Find properties for gradient color settings
            hairGradientStart = serializedObject.FindProperty("hairGradientStart");
            hairGradientEnd = serializedObject.FindProperty("hairGradientEnd");
            skinGradientStart = serializedObject.FindProperty("skinGradientStart");
            skinGradientEnd = serializedObject.FindProperty("skinGradientEnd");
            eyeGradientStart = serializedObject.FindProperty("eyeGradientStart");
            eyeGradientEnd = serializedObject.FindProperty("eyeGradientEnd");
            gradientKeysCount = serializedObject.FindProperty("gradientKeysCount");
            
            // Find constants for gradient and slots
            gradientAlphaValue = serializedObject.FindProperty("gradientAlphaValue");
            hairSlotId = serializedObject.FindProperty("hairSlotId");
            skinSlotId = serializedObject.FindProperty("skinSlotId");
            eyeSlotId = serializedObject.FindProperty("eyeSlotId");
            
            CreateSlotTypeDefinitionLists();
            CreateColorLists();
            
            SlotIdEditorUtility.ClearCache();
        }
        
        private void CreateColorLists()
        {
            hairColorsList = new ReorderableList(serializedObject, hairColors, true, true, true, true);
            hairColorsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Hair colors");
            hairColorsList.drawElementCallback = (rect, index, isActive, isFocused) => DrawColorWithWeightElement(rect, hairColors.GetArrayElementAtIndex(index));
            
            skinColorsList = new ReorderableList(serializedObject, skinColors, true, true, true, true);
            skinColorsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Skin colors");
            skinColorsList.drawElementCallback = (rect, index, isActive, isFocused) => DrawColorWithWeightElement(rect, skinColors.GetArrayElementAtIndex(index));
            
            eyeColorsList = new ReorderableList(serializedObject, eyeColors, true, true, true, true);
            eyeColorsList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Eye colors");
            eyeColorsList.drawElementCallback = (rect, index, isActive, isFocused) => DrawColorWithWeightElement(rect, eyeColors.GetArrayElementAtIndex(index));
        }
        
        private void DrawColorWithWeightElement(Rect rect, SerializedProperty element)
        {
            SerializedProperty colorProp = element.FindPropertyRelative("color");
            SerializedProperty weightProp = element.FindPropertyRelative("weight");
            
            float colorWidth = rect.width * 0.7f;
            float weightWidth = rect.width * 0.25f;
            float spacing = rect.width * 0.05f;
            
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, colorWidth, EditorGUIUtility.singleLineHeight),
                colorProp,
                GUIContent.none
            );
            
            EditorGUI.PropertyField(
                new Rect(rect.x + colorWidth + spacing, rect.y, weightWidth, EditorGUIUtility.singleLineHeight),
                weightProp,
                GUIContent.none
            );
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Main settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(priceMultiplier, new GUIContent("Price multiplier"));
            EditorGUILayout.PropertyField(mainCharacterId, new GUIContent("Main character ID"));
            EditorGUILayout.PropertyField(maleTemplateId, new GUIContent("Male template ID"));
            EditorGUILayout.PropertyField(femaleTemplateId, new GUIContent("Female template ID"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Items and templates", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(itemsData, new GUIContent("Items"), true);
            EditorGUILayout.PropertyField(characterTemplates, new GUIContent("Character templates"), true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings for slot types", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Configure slot IDs for each item type. These IDs will be available in dropdown lists when creating items and customization slots.", MessageType.Info);
            EditorGUILayout.Space();
            
            if (slotTypeDefinitionLists != null)
            {
                for (int i = 0; i < slotTypeDefinitionLists.Length; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    SerializedProperty slotTypeDefinition = slotTypeDefinitions.GetArrayElementAtIndex(i);
                    SerializedProperty slotTypeProperty = slotTypeDefinition.FindPropertyRelative("slotType");
                    ItemType itemType = (ItemType)slotTypeProperty.enumValueIndex;
                    
                    EditorGUILayout.LabelField(itemType.ToString() + " slots", EditorStyles.boldLabel);
                    
                    GUIContent deleteContent = new GUIContent("X", "Delete slot type");
                    GUIStyle deleteButtonStyle = new GUIStyle(GUI.skin.button);
                    deleteButtonStyle.normal.textColor = Color.red;
                    deleteButtonStyle.fontStyle = FontStyle.Bold;
                    
                    if (GUILayout.Button(deleteContent, deleteButtonStyle, GUILayout.Width(25), GUILayout.Height(18)))
                    {
                        DeleteSlotType(i);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    slotTypeDefinitionLists[i].DoLayoutList();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }
            
            if (GUILayout.Button("Add new slot type", GUILayout.Height(30)))
            {
                AddNewSlotType();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings for character generation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.Slider(randomColorsChance, 0f, 1f, new GUIContent("Chance of random colors", "Chance of generating fully random colors (0-1)"));
            EditorGUILayout.Slider(monochromaticThemeChance, 0f, 1f, new GUIContent("Chance of monochromatic theme", "Chance of generating a monochromatic theme (0-1)"));
            EditorGUILayout.Slider(slotItemSpawnChance, 0f, 1f, new GUIContent("Chance of item spawn", "Chance of item appearing in a removable slot (0-1)"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings for monochromatic colors", EditorStyles.boldLabel);
            EditorGUILayout.Slider(monochromaticSaturationRange, 0f, 0.5f, new GUIContent("Saturation range", "Range of saturation for monochromatic colors"));
            EditorGUILayout.Slider(monochromaticBrightnessRange, 0f, 0.5f, new GUIContent("Brightness range", "Range of brightness for monochromatic colors"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings for gradient colors", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(hairGradientStart, 0f, 1f, new GUIContent("Hair start", "Position of the hair start in the gradient"));
            EditorGUILayout.Slider(hairGradientEnd, 0f, 1f, new GUIContent("Hair end", "Position of the hair end in the gradient"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(skinGradientStart, 0f, 1f, new GUIContent("Skin start", "Position of the skin start in the gradient"));
            EditorGUILayout.Slider(skinGradientEnd, 0f, 1f, new GUIContent("Skin end", "Position of the skin end in the gradient"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(eyeGradientStart, 0f, 1f, new GUIContent("Eyes start", "Position of the eyes start in the gradient"));
            EditorGUILayout.Slider(eyeGradientEnd, 0f, 1f, new GUIContent("Eyes end", "Position of the eyes end in the gradient"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.IntSlider(gradientKeysCount, 1, 5, new GUIContent("Number of keys", "Number of color keys for each category"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Constants", EditorStyles.boldLabel);
            EditorGUILayout.Slider(gradientAlphaValue, 0f, 1f, new GUIContent("Gradient alpha", "Value of the gradient alpha"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Constants", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(hairSlotId, new GUIContent("Hair slot", "Name of the hair slot"));
            EditorGUILayout.PropertyField(skinSlotId, new GUIContent("Skin slot", "Name of the skin slot"));
            EditorGUILayout.PropertyField(eyeSlotId, new GUIContent("Eyes slot", "Name of the eyes slot"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Natural colors - Hair", EditorStyles.boldLabel);
            hairColorsList.DoLayoutList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Natural colors - Skin", EditorStyles.boldLabel);
            skinColorsList.DoLayoutList();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Natural colors - Eyes", EditorStyles.boldLabel);
            eyeColorsList.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                SlotIdEditorUtility.ClearCache();
            }
        }
        
        private void AddNewSlotType()
        {
            slotTypeDefinitions.arraySize++;
            int newIndex = slotTypeDefinitions.arraySize - 1;
            SerializedProperty newSlotType = slotTypeDefinitions.GetArrayElementAtIndex(newIndex);
            
            SerializedProperty typeProperty = newSlotType.FindPropertyRelative("slotType");
            typeProperty.enumValueIndex = 0; 
            
            SerializedProperty slotIdsProperty = newSlotType.FindPropertyRelative("slotIds");
            slotIdsProperty.arraySize = 1;
            slotIdsProperty.GetArrayElementAtIndex(0).stringValue = "NewSlot";
            
            CreateSlotTypeDefinitionLists();
            
            SlotIdEditorUtility.ClearCache();
        }
        
        private void DeleteSlotType(int index)
        {
            if (index < 0 || index >= slotTypeDefinitions.arraySize)
                return;
                
            slotTypeDefinitions.DeleteArrayElementAtIndex(index);
            
            CreateSlotTypeDefinitionLists();
            
            SlotIdEditorUtility.ClearCache();
            
            EditorUtility.SetDirty(target);
        }
        
        private void CreateSlotTypeDefinitionLists()
        {
            
            slotTypeDefinitionLists = new ReorderableList[slotTypeDefinitions.arraySize];
            
            for (int i = 0; i < slotTypeDefinitions.arraySize; i++)
            {
                int currentIndex = i; 
                SerializedProperty slotTypeDefinition = slotTypeDefinitions.GetArrayElementAtIndex(currentIndex);
                SerializedProperty slotTypeProperty = slotTypeDefinition.FindPropertyRelative("slotType");
                SerializedProperty slotIdsProperty = slotTypeDefinition.FindPropertyRelative("slotIds");
                
                ItemType itemType = (ItemType)slotTypeProperty.enumValueIndex;
                string headerText = itemType.ToString() + " slots";
                
                slotTypeDefinitionLists[i] = new ReorderableList(serializedObject, slotIdsProperty, true, true, true, true)
                {
                    drawHeaderCallback = (rect) => 
                    {
                        float enumWidth = 130;
                        float labelWidth = rect.width - enumWidth - 10;
                        
                        EditorGUI.LabelField(
                            new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight),
                            "Slots for type:"
                        );
                        
                        int currentType = slotTypeProperty.enumValueIndex;
                        int newType = EditorGUI.EnumPopup(
                            new Rect(rect.x + labelWidth + 10, rect.y, enumWidth, EditorGUIUtility.singleLineHeight),
                            (ItemType)currentType
                        ).GetHashCode();
                        
                        if (newType != currentType)
                        {
                            slotTypeProperty.enumValueIndex = newType;
                            SlotIdEditorUtility.ClearCache();
                        }
                    },
                    
                    drawElementCallback = (rect, elementIndex, isActive, isFocused) =>
                    {
                        var element = slotIdsProperty.GetArrayElementAtIndex(elementIndex);
                        
                        if (isActive)
                        {
                            EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), 
                                new Color(0.3f, 0.6f, 0.9f, 0.2f));
                        }
                        else if (elementIndex % 2 == 0)
                        {
                            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.05f));
                        }
                        
                        EditorGUI.PropertyField(
                            new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            element,
                            GUIContent.none
                        );
                    },
                    
                    onAddCallback = (list) =>
                    {
                        int index = list.serializedProperty.arraySize;
                        list.serializedProperty.arraySize++;
                        list.index = index;
                        var element = list.serializedProperty.GetArrayElementAtIndex(index);
                        element.stringValue = "NewSlotId";
                        
                        SlotIdEditorUtility.ClearCache();
                    },
                    
                    onRemoveCallback = (list) =>
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                        
                        SlotIdEditorUtility.ClearCache();
                    }
                };
            }
        }
    }
} 