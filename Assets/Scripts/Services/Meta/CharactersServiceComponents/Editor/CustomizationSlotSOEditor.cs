#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.Services.Meta.Editor
{
    [CustomEditor(typeof(CustomizationSlotSO))]
    public class CustomizationSlotSOEditor : UnityEditor.Editor
    {
        private SerializedProperty slotId;
        private SerializedProperty slotType;
        private SerializedProperty isRemovable;
        
        private void OnEnable()
        {
            slotId = serializedObject.FindProperty("slotId");
            slotType = serializedObject.FindProperty("slotType");
            isRemovable = serializedObject.FindProperty("isRemovable");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Customization Slot Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(slotId, new GUIContent("Slot ID"));
            EditorGUILayout.PropertyField(slotType, new GUIContent("Slot Type"));
            EditorGUILayout.PropertyField(isRemovable, new GUIContent("Is Removable"));
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Validate Slot", GUILayout.Height(30)))
            {
                ValidateSlot();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void ValidateSlot()
        {
            var slot = target as CustomizationSlotSO;
            if (slot == null) return;
            
            bool isValid = true;
            string errorMessage = "";
            
            if (string.IsNullOrEmpty(slot.SlotId))
            {
                isValid = false;
                errorMessage += "Slot ID is required.\n";
            }
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", "Slot is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Error", errorMessage, "OK");
            }
        }
    }
}
#endif 