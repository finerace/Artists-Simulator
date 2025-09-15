#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Services.Meta.Editor
{
    [CustomEditor(typeof(CustomizationTemplateSO))]
    public class CustomizationTemplateSOEditor : UnityEditor.Editor
    {
        private SerializedProperty templateId;
        private SerializedProperty characterGender;
        private SerializedProperty customizationSlots;
        private SerializedProperty characterModelAddressableId;
        private SerializedProperty defaultItemsList;
        
        private ReorderableList slotsList;
        private Dictionary<string, bool> slotFoldouts = new Dictionary<string, bool>();
        
        private CustomizationTemplateSO templateTarget;
        
        private void OnEnable()
        {
            templateId = serializedObject.FindProperty("templateId");
            characterGender = serializedObject.FindProperty("characterGender");
            customizationSlots = serializedObject.FindProperty("customizationSlots");
            characterModelAddressableId = serializedObject.FindProperty("characterModelAddressableId");
            defaultItemsList = serializedObject.FindProperty("defaultItemsList");
            
            templateTarget = (CustomizationTemplateSO)target;
            
            CreateSlotsList();
            
            Selection.selectionChanged += OnSelectionChanged;
        }
        
        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }
        
        private void OnSelectionChanged()
        {
            if (EditorGUIUtility.GetObjectPickerControlID() != 0 && 
                EditorGUIUtility.GetObjectPickerObject() is CustomizationSlotSO slot)
            {
                OnObjectPickerClosed();
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Character Template Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(templateId, new GUIContent("Template ID"));
            EditorGUILayout.PropertyField(characterGender, new GUIContent("Character Gender"));
            EditorGUILayout.PropertyField(characterModelAddressableId, new GUIContent("Model ID (Addressable)"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Customization Slots", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            slotsList.DoLayoutList();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add New Slot", GUILayout.Height(30)))
            {
                CreateNewSlot();
            }
            
            DrawDefaultItemsSection();
            
            serializedObject.ApplyModifiedProperties();
            
            HandleObjectPickerEvents();
        }
        
        private void DrawDefaultItemsSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Default Items", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (templateTarget == null)
            {
                EditorGUILayout.HelpBox("Failed to access character template.", MessageType.Error);
                return;
            }
            
            var slots = new List<CustomizationSlotSO>();
            for (int i = 0; i < customizationSlots.arraySize; i++)
            {
                var slotProperty = customizationSlots.GetArrayElementAtIndex(i);
                var slot = slotProperty.objectReferenceValue as CustomizationSlotSO;
                if (slot != null)
                {
                    slots.Add(slot);
                }
            }
            
            if (slots.Count == 0)
            {
                EditorGUILayout.HelpBox("No customization slots found. Add slots first.", MessageType.Info);
                return;
            }
            
            foreach (var slot in slots)
            {
                if (slot == null) continue;
                
                string slotKey = slot.SlotId;
                if (!slotFoldouts.ContainsKey(slotKey))
                {
                    slotFoldouts[slotKey] = false;
                }
                
                EditorGUILayout.BeginVertical("box");
                
                slotFoldouts[slotKey] = EditorGUILayout.Foldout(slotFoldouts[slotKey], 
                    $"Slot: {slot.SlotId} ({slot.SlotType})", true);
                
                if (slotFoldouts[slotKey])
                {
                    EditorGUILayout.Space(5);
                    
                    var compatibleItems = FindCompatibleItems(slot);
                    
                    if (compatibleItems.Count > 0)
                    {
                        EditorGUILayout.LabelField($"Compatible Items: {compatibleItems.Count}", EditorStyles.miniLabel);
                        
                        foreach (var item in compatibleItems)
                        {
                            EditorGUILayout.BeginHorizontal();
                            
                            EditorGUILayout.LabelField($"Item ID: {item.ItemId}", EditorStyles.miniLabel);
                            
                            if (GUILayout.Button("Set as Default", GUILayout.Width(100)))
                            {
                                SetDefaultItem(slot, item);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No compatible items found for this slot.", MessageType.Warning);
                    }
                    
                    if (GUILayout.Button("Browse Items", GUILayout.Height(25)))
                    {
                        ShowItemSelector(slot);
                    }
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        private void ShowItemSelector(CustomizationSlotSO slot)
        {
            var compatibleItems = FindCompatibleItems(slot);
            
            if (compatibleItems.Count == 0)
            {
                EditorUtility.DisplayDialog("No Items", 
                    "No compatible items found for this slot.", "OK");
                return;
            }
            
            var menu = new GenericMenu();
            
            foreach (var item in compatibleItems)
            {
                string itemPath = $"Items/{item.ItemId}";
                menu.AddItem(new GUIContent(itemPath), false, () => {
                    SetDefaultItem(slot, item);
                });
            }
            
            menu.ShowAsContext();
        }
        
        private List<CharacterItemData> FindCompatibleItems(CustomizationSlotSO slot)
        {
            var items = new List<CharacterItemData>();
            
            string[] guids = AssetDatabase.FindAssets("t:CharacterItemData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<CharacterItemData>(path);
                
                if (item != null && item.SlotId == slot.SlotId)
                {
                    items.Add(item);
                }
            }
            
            return items;
        }
        
        private void SetDefaultItem(CustomizationSlotSO slot, CharacterItemData item)
        {
            if (templateTarget == null) return;
            
            EditorUtility.SetDirty(templateTarget);
            
            Debug.Log($"Set default item '{item.ItemId}' for slot '{slot.SlotId}'");
        }
        
        private void HandleObjectPickerEvents()
        {
            if (Event.current.type == EventType.ExecuteCommand && 
                Event.current.commandName == "ObjectSelectorClosed")
            {
                OnObjectPickerClosed();
                Event.current.Use();
            }
        }
        
        private void CreateSlotsList()
        {
            slotsList = new ReorderableList(serializedObject, customizationSlots, true, true, true, true);
            
            slotsList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Customization Slots");
            };
            
            slotsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = slotsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
                    element, GUIContent.none);
            };
            
            slotsList.onAddCallback = (ReorderableList list) => {
                CreateNewSlot();
            };
            
            slotsList.onRemoveCallback = (ReorderableList list) => {
                if (EditorUtility.DisplayDialog("Remove Slot", 
                    "Are you sure you want to remove this slot?", "Remove", "Cancel"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }
        
        private void CreateNewSlot()
        {
            var slot = CreateInstance<CustomizationSlotSO>();
            
            string slotName = "New Slot";
            string slotId = "new_slot";
            
            slot.Initialize(slotId, ItemType.Object, true);
            
            string path = EditorUtility.SaveFilePanelInProject("Save Customization Slot", 
                slotName, "asset", "Choose location to save slot");
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(slot, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                AddSlotToList(slot);
                
                Debug.Log($"Created new customization slot: {slotName}");
            }
            else
            {
                DestroyImmediate(slot);
            }
        }
        
        private void AddSlotToList(CustomizationSlotSO slot)
        {
            if (slot == null) return;
            
            int index = customizationSlots.arraySize;
            customizationSlots.arraySize++;
            
            var newSlotProperty = customizationSlots.GetArrayElementAtIndex(index);
            newSlotProperty.objectReferenceValue = slot;
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnCreateNewSlot()
        {
            CreateNewSlot();
        }
        
        private void OnObjectPickerClosed()
        {
            var selectedObject = EditorGUIUtility.GetObjectPickerObject();
            if (selectedObject is CustomizationSlotSO slot)
            {
                AddSlotToList(slot);
            }
        }
    }
}
#endif 