using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Configs/LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        [SerializeField] private TextAsset keysFile;
        [SerializeField] private TextAsset[] languageFiles;
        [SerializeField] private SystemLanguage[] languages;
        
        private List<string> keys = new List<string>();
        private Dictionary<SystemLanguage, List<string>> translations = new Dictionary<SystemLanguage, List<string>>();
        
        private bool initialized = false;
        
        public void Initialize()
        {
            if (keysFile == null)
            {
                Debug.LogError("LocalizationConfig: keys file is missing!");
                return;
            }
            LoadKeys();
            LoadTranslations();
        }
        
        private void LoadKeys()
        {
            keys.Clear();
            string[] lines = keysFile.text.Split('\n');
            
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                // Пропускаем комментарии и пустые строки
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//"))
                    continue;
                    
                keys.Add(trimmed);
            }
        }
        
        private void LoadTranslations()
        {
            translations.Clear();
            
            for (int i = 0; i < languages.Length; i++)
            {
                if (i >= languageFiles.Length)
                    break;
                    
                TextAsset langFile = languageFiles[i];
                if (langFile == null)
                    continue;
                    
                var langValues = new List<string>();
                
                string[] lines = langFile.text.Split('\n');
                
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    // Пропускаем комментарии и пустые строки
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//"))
                        continue;
                        
                    langValues.Add(trimmed);
                }
                
                translations[languages[i]] = langValues;
            }
        }
        
        public string GetTextByKey(string key, SystemLanguage language)
        {
            int index = keys.IndexOf(key);
            if (index < 0)
                return $"[Unknown key: {key}]";
                
            if (!translations.TryGetValue(language, out var values))
            {
                if (language != SystemLanguage.English && 
                    translations.TryGetValue(SystemLanguage.English, out var englishValues))
                {
                    if (index < englishValues.Count)
                        return englishValues[index];
                }
                
                return $"[Missing translation: {key}]";
            }
            
            if (index >= values.Count)
                return $"[Index out of range: {key}]";
                
            return values[index];
        }
        
        public IReadOnlyList<string> GetAllKeys()
        {
            if (!initialized)
                Initialize();
                
            return keys;
        }
        
        public TextAsset GetKeysFile()
        {
            return keysFile;
        }

        public TextAsset[] GetLanguageFiles()
        {
            return languageFiles;
        }

        public SystemLanguage[] GetSupportedLanguages()
        {
            return languages;
        }

        public bool AddLanguage(SystemLanguage language, TextAsset languageAsset)
        {
            // Проверяем, есть ли уже такой язык
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i] == language)
                {
                    // Язык уже есть, просто обновляем файл
                    if (i < languageFiles.Length)
                    {
                        languageFiles[i] = languageAsset;
                        Initialize(); // Перезагружаем данные
                        return false; // Язык не был добавлен, а обновлен
                    }
                }
            }
            
            // Создаем новые массивы с дополнительным элементом
            var newLanguages = new SystemLanguage[languages.Length + 1];
            var newLanguageFiles = new TextAsset[languageFiles.Length + 1];
            
            // Копируем существующие данные
            for (int i = 0; i < languages.Length; i++)
            {
                newLanguages[i] = languages[i];
            }
            
            for (int i = 0; i < languageFiles.Length; i++)
            {
                newLanguageFiles[i] = languageFiles[i];
            }
            
            // Добавляем новый язык
            newLanguages[languages.Length] = language;
            newLanguageFiles[languageFiles.Length] = languageAsset;
            
            // Обновляем ссылки
            languages = newLanguages;
            languageFiles = newLanguageFiles;
            
            // Перезагружаем данные
            initialized = false;
            Initialize();
            
            return true;
        }
    }
} 