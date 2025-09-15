#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Services.Meta.Editor
{
    [CustomEditor(typeof(CharacterItemData))]
    public class CharacterItemDataCustomEditor : UnityEditor.Editor
    {
        private SerializedProperty itemId;
        private SerializedProperty itemNameId;
        private SerializedProperty slotId;
        private SerializedProperty itemType;
        private SerializedProperty characterGender;
        private SerializedProperty isCanColorize;
        private SerializedProperty itemObjectAddressableId;
        private SerializedProperty itemIcon;
        
        private void OnEnable()
        {
            itemId = serializedObject.FindProperty("itemId");
            itemNameId = serializedObject.FindProperty("itemNameId");
            slotId = serializedObject.FindProperty("slotId");
            itemType = serializedObject.FindProperty("itemType");
            characterGender = serializedObject.FindProperty("characterGender");
            isCanColorize = serializedObject.FindProperty("isCanColorize");
            itemObjectAddressableId = serializedObject.FindProperty("itemObjectAddressableId");
            itemIcon = serializedObject.FindProperty("itemIcon");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Character Item Data", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(itemId, new GUIContent("Item ID"));
            EditorGUILayout.PropertyField(itemNameId, new GUIContent("Item Name ID"));
            EditorGUILayout.PropertyField(slotId, new GUIContent("Slot ID"));
            EditorGUILayout.PropertyField(itemType, new GUIContent("Item Type"));
            EditorGUILayout.PropertyField(characterGender, new GUIContent("Character Gender"));
            EditorGUILayout.PropertyField(isCanColorize, new GUIContent("Can Colorize"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(itemObjectAddressableId, new GUIContent("Addressable ID"));
            EditorGUILayout.PropertyField(itemIcon, new GUIContent("Item Icon"));
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Validate Item", GUILayout.Height(30)))
            {
                ValidateItem();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void ValidateItem()
        {
            var item = target as CharacterItemData;
            if (item == null) return;
            
            bool isValid = true;
            string errorMessage = "";
            
            if (string.IsNullOrEmpty(item.ItemId))
            {
                isValid = false;
                errorMessage += "Item ID is required.\n";
            }
            
            if (string.IsNullOrEmpty(item.SlotId))
            {
                isValid = false;
                errorMessage += "Slot ID is required.\n";
            }
            
            if (string.IsNullOrEmpty(item.ItemObjectAddressableId))
            {
                isValid = false;
                errorMessage += "Addressable ID is required.\n";
            }
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", "Item is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Error", errorMessage, "OK");
            }
        }
    }
}
#endif
