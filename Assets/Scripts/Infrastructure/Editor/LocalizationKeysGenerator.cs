using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using Game.Infrastructure.Configs;

namespace Game.Infrastructure.Editor
{
    public class LocalizationKeysGenerator : EditorWindow
    {
        private LocalizationConfig localizationConfig;
        private string outputPath = "Assets/Scripts/Services/Common/LocalizationKeys.cs";
        private string namespaceName = "Game.Services.Common";
        private string className = "LocalizationKeys";
        
        [MenuItem("Tools/Localization/Generate Localization Keys")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationKeysGenerator>("Localization Keys Generator");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Localization Keys Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            localizationConfig = EditorGUILayout.ObjectField("Localization Config", localizationConfig, typeof(LocalizationConfig), false) as LocalizationConfig;
            
            EditorGUILayout.Space(5);
            
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            namespaceName = EditorGUILayout.TextField("Namespace", namespaceName);
            className = EditorGUILayout.TextField("Class Name", className);
            
            EditorGUILayout.Space(10);
            
            GUI.enabled = localizationConfig != null;
            
            if (GUILayout.Button("Generate Keys Class"))
            {
                GenerateKeysClass();
            }
            
            GUI.enabled = true;
        }
        
        private void GenerateKeysClass()
        {
            if (localizationConfig == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a Localization Config", "OK");
                return;
            }
            
            var keys = localizationConfig.GetAllKeys();
            if (keys.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No keys found in the config", "OK");
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("// This file is auto-generated. Do not modify.");
            sb.AppendLine();
            
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public static class {className}");
            sb.AppendLine("    {");
            
            foreach (var key in keys)
            {
                sb.AppendLine($"        public const string {key} = \"{key}\";");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            string directoryPath = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            File.WriteAllText(outputPath, sb.ToString());
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", "Localization keys generated successfully!", "OK");
        }
    }
} 