using System;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Additionals;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using UnityEngine;
using YG;

namespace Game.Services.Common
{
    
    public class LocalizationService : ILocalizationService
    {
        private LocalizationConfig config;
        
        public event Action OnLanguageChanged;
        public SystemLanguage CurrentLanguage { get; private set; }
        
        [LogMethod(LogLevel.Debug)]
        public void Initialize()
        {
            config = ConfigsProxy.LocalizationConfig;
            
            CurrentLanguage = DetectLanguage();
            SetLanguage(CurrentLanguage);
        }
        
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logs.Warning("GetText called with null or empty key");
                return key ?? "";
            }

            if (config == null)
            {
                Logs.Warning("LocalizationConfig not initialized");
                return key;
            }
            
            return config.GetTextByKey(key, CurrentLanguage);
        }
        
        public void SetLanguage(SystemLanguage language)
        {
            if (CurrentLanguage == language)
                return;

            var availableLanguages = GetAvailableLanguages();
            if (availableLanguages != null && Array.IndexOf(availableLanguages, language) == -1)
            {
                Logs.Warning($"Language {language} not supported, using current: {CurrentLanguage}");
                return;
            }
                
            var oldLanguage = CurrentLanguage;
            CurrentLanguage = language;
            OnLanguageChanged?.Invoke();
            
            Logs.Info($"Language changed: {oldLanguage} → {CurrentLanguage}");
        }

        public SystemLanguage[] GetAvailableLanguages()
        {
            if (config == null)
            {
                Logs.Warning("LocalizationConfig not initialized");
                return new SystemLanguage[] { SystemLanguage.English };
            }

            return config.GetSupportedLanguages();
        }
        
        private SystemLanguage DetectLanguage()
        {
            try
            {
                return LanguageUtility.IsoCodeToSystemLanguage(YandexGame.EnvironmentData.language);
            }
            catch
            {
                Logs.Warning("Failed to detect language from YandexGame, using English");
                return SystemLanguage.English;
            }
        }
    }
} 