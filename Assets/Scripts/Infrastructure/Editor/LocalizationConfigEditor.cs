#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Editor;

namespace Game.Cosmetic.EditorUI
{
    [CustomEditor(typeof(LocalizationConfig))]
    public class LocalizationConfigEditor : Editor
    {
        private SerializedProperty keysFileProperty;
        private SerializedProperty languageFilesProperty;
        private SerializedProperty languagesProperty;
        
        private void OnEnable()
        {
            keysFileProperty = serializedObject.FindProperty("keysFile");
            languageFilesProperty = serializedObject.FindProperty("languageFiles");
            languagesProperty = serializedObject.FindProperty("languages");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Localization Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(keysFileProperty, new GUIContent("Keys File"));
            EditorGUILayout.PropertyField(languageFilesProperty, new GUIContent("Language Files"), true);
            EditorGUILayout.PropertyField(languagesProperty, new GUIContent("Supported Languages"), true);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Generate Keys", GUILayout.Height(30)))
            {
                GenerateLocalizationKeys();
            }
            
            if (GUILayout.Button("Validate Config", GUILayout.Height(30)))
            {
                ValidateConfig();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void GenerateLocalizationKeys()
        {
            var config = target as LocalizationConfig;
            if (config == null) return;
            
            LocalizationKeysGenerator.ShowWindow();
            EditorUtility.SetDirty(config);
            
            Debug.Log("Localization keys generated successfully!");
        }
        
        private void ValidateConfig()
        {
            var config = target as LocalizationConfig;
            if (config == null) return;
            
            bool isValid = true;
            string errorMessage = "";
            
            if (keysFileProperty.objectReferenceValue == null)
            {
                isValid = false;
                errorMessage += "Keys file is not set.\n";
            }
            
            if (languageFilesProperty.arraySize == 0)
            {
                isValid = false;
                errorMessage += "No language files defined.\n";
            }
            
            if (languagesProperty.arraySize == 0)
            {
                isValid = false;
                errorMessage += "No languages defined.\n";
            }
            
            if (isValid)
            {
                EditorUtility.DisplayDialog("Validation", "Configuration is valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Error", errorMessage, "OK");
            }
        }
    }
}
#endif 