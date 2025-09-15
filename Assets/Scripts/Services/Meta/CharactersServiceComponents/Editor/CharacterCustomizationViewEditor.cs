#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Services.Meta.Editor
{
    [CustomEditor(typeof(CharacterCustomizationView))]
    public class CharacterCustomizationViewEditor : UnityEditor.Editor
    {
        private SerializedProperty customizationTemplate;
        private SerializedProperty allSlotReferences;
        
        private ReorderableList slotsList;
        
        private void OnEnable()
        {
            customizationTemplate = serializedObject.FindProperty("customizationTemplate");
            allSlotReferences = serializedObject.FindProperty("allSlotReferences");
            
            if (allSlotReferences == null)
            {
                Debug.LogError("CharacterCustomizationViewEditor: allSlotReferences property not found! Make sure the field exists in CharacterCustomizationView.");
                return;
            }
            
            CreateSlotsList();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Character Customization Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(customizationTemplate, new GUIContent("Character Template"));
            
            EditorGUILayout.Space();
            
            if (allSlotReferences != null)
                EditorGUILayout.LabelField($"Total Slots: {allSlotReferences.arraySize}", EditorStyles.miniLabel);
            
            if (customizationTemplate.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Please select a character template first", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Generate Slots", GUILayout.Height(30)))
                    GenerateSlots();
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Customization Slots", EditorStyles.boldLabel);
                
                if (slotsList != null)
                    slotsList.DoLayoutList();
                else
                    EditorGUILayout.HelpBox("Error: ReorderableList not created", MessageType.Error);
                
                EditorGUILayout.Space(10);
            }
            
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
        
        private void CreateSlotsList()
        {
            slotsList = new ReorderableList(serializedObject, allSlotReferences, true, true, true, true);
            
            // Включаем множественный выбор!
            slotsList.multiSelect = true;
            
            slotsList.drawHeaderCallback = (rect) => 
            {
                EditorGUI.LabelField(rect, "Customization Slots");
            };
            
            slotsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index < 0 || index >= allSlotReferences.arraySize)
                    return;
                
                var element = allSlotReferences.GetArrayElementAtIndex(index);
                if (element == null)
                    return;
                
                var headerRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                var contentRect = new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, 
                    rect.width, rect.height - EditorGUIUtility.singleLineHeight - 2);
                
                string slotType = GetSlotTypeName(element);
                EditorGUI.LabelField(headerRect, $"[{slotType}]", EditorStyles.boldLabel);
                
                EditorGUI.PropertyField(contentRect, element, true);
            };
            
            slotsList.elementHeightCallback = (index) => 
            {
                if (index < 0 || index >= allSlotReferences.arraySize)
                    return EditorGUIUtility.singleLineHeight;
                
                var element = allSlotReferences.GetArrayElementAtIndex(index);
                if (element == null)
                    return EditorGUIUtility.singleLineHeight;
                
                return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.singleLineHeight + 2;
            };
            
            slotsList.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();
                
                foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
                {
                    var slotTypeName = SlotTypeRegistry.GetSlotTypeName(itemType);
                    var displayName = SlotTypeRegistry.GetDisplayName(itemType);
                    menu.AddItem(new GUIContent(displayName), false, () => AddSlot(slotTypeName));
                }
                
                menu.ShowAsContext();
            };
            
            slotsList.onCanRemoveCallback = (list) => list.count > 0;
            
            slotsList.onRemoveCallback = (list) =>
            {
                var selectedIndices = list.selectedIndices;
                
                if (selectedIndices.Count > 1)
                {
                    if (EditorUtility.DisplayDialog("Delete Multiple Slots", 
                        $"Are you sure you want to delete {selectedIndices.Count} selected slots?", 
                        "Delete", "Cancel"))
                    {
                        var sortedIndices = new List<int>(selectedIndices);
                        sortedIndices.Sort((a, b) => b.CompareTo(a));
                        
                        foreach (int index in sortedIndices)
                        {
                            if (index >= 0 && index < list.serializedProperty.arraySize)
                                list.serializedProperty.DeleteArrayElementAtIndex(index);
                        }
                        
                        list.ClearSelection();
                    }
                }
                else
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }
        
        private string GetSlotTypeName(SerializedProperty element)
        {
            const string slotLabel = "slot";
            
            if (element.managedReferenceValue != null)
            {
                var slotType = element.managedReferenceValue.GetType();
                var attribute = slotType.GetCustomAttribute<SlotTypeAttribute>();
                if (attribute != null)
                    return $"{attribute.ItemType.ToString()} {slotLabel}";
            }
            
            return $"Unknown {slotLabel}";
        }
        
        private void AddSlot(string slotType)
        {
            int index = allSlotReferences.arraySize;
            allSlotReferences.arraySize++;
            
            var element = allSlotReferences.GetArrayElementAtIndex(index);
            if (element != null)
            {
                element.managedReferenceValue = SlotTypeRegistry.CreateSlotInstance(slotType);
                
                var slotIdProp = element.FindPropertyRelative("slotId");
                if (slotIdProp != null) 
                    slotIdProp.stringValue = $"New{slotType}";
                
                InitializeSlotDefaults(element, slotType);
            }
            
            serializedObject.ApplyModifiedProperties();
            slotsList.index = index;
        }
        
        private void InitializeSlotDefaults(SerializedProperty element, string expectedSlotType)
        {
            var posProp = element.FindPropertyRelative("positionOffset");
            var rotProp = element.FindPropertyRelative("rotationOffset");
            var scaleProp = element.FindPropertyRelative("scaleOffset");
            var materialIndexProp = element.FindPropertyRelative("materialIndex");
            var renderersProp = element.FindPropertyRelative("renderers");
            
            if (posProp != null && rotProp != null && scaleProp != null)
            {
                posProp.vector3Value = Vector3.zero;
                rotProp.vector3Value = Vector3.zero;
                scaleProp.vector3Value = Vector3.one;
            }
            
            if (materialIndexProp != null && renderersProp != null)
            {
                materialIndexProp.intValue = 0;
                renderersProp.arraySize = 0;
            }
        }
        
        private void GenerateSlots()
        {
            GenerateSlotsFromTemplate();
            
            void GenerateSlotsFromTemplate()
            {
                Dictionary<string, int> existingSlots = new Dictionary<string, int>();
                bool anyChanges = false;
                
                var customizationView = (CharacterCustomizationView)target;
                if (customizationView == null)
                    return;
                    
                var template = customizationView.CustomizationTemplate;
                if (template == null)
                    return;
                
                CreateExistingSlotsMap();
                AddMissingSlots();
                ApplyChangesIfNeeded();
                
                void CreateExistingSlotsMap()
                {
                    existingSlots.Clear();
                    
                    for (int i = 0; i < allSlotReferences.arraySize; i++)
                    {
                        var element = allSlotReferences.GetArrayElementAtIndex(i);
                        if (element == null)
                            continue;
                            
                        var slotIdProp = element.FindPropertyRelative("slotId");
                        if (slotIdProp == null)
                            continue;
                            
                        var slotId = slotIdProp.stringValue;
                        if (!string.IsNullOrEmpty(slotId))
                            existingSlots[slotId] = i;
                    }
                }
                
                void AddMissingSlots()
                {
                    if (template.CustomizationSlots == null)
                        return;
                        
                    foreach (var slot in template.CustomizationSlots)
                    {
                        if (slot == null) 
                            continue;
                        
                        if (!existingSlots.ContainsKey(slot.SlotId))
                            CreateSlotFromTemplate(slot);
                    }
                }
                
                void CreateSlotFromTemplate(CustomizationSlotSO slot)
                {
                    string slotType = DetermineSlotType(slot.SlotType);
                    
                    int index = allSlotReferences.arraySize;
                    allSlotReferences.arraySize++;
                    
                    var element = allSlotReferences.GetArrayElementAtIndex(index);
                    if (element != null)
                    {
                        element.managedReferenceValue = SlotTypeRegistry.CreateSlotInstance(slotType);
                        
                        var slotIdProp = element.FindPropertyRelative("slotId");
                        if (slotIdProp != null) 
                            slotIdProp.stringValue = slot.SlotId;
                        
                        InitializeSlotDefaults(element, slotType);
                        anyChanges = true;
                    }
                }
                
                string DetermineSlotType(ItemType itemType)
                {
                    return SlotTypeRegistry.GetSlotTypeName(itemType);
                }
                
                void ApplyChangesIfNeeded()
                {
                    if (anyChanges)
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                    
                    EditorUtility.DisplayDialog("Generate Slots", 
                        "Slots generated successfully. Now you need to assign corresponding transforms and renderers.", "OK");
                }
            }
        }
    }
}
#endif 
 