using System;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Additionals;
using Game.Services.Common.Logging;
using UnityEngine;
using YG;

namespace Game.Services.Common
{
    
    public class SettingsService
    {
        private readonly ILocalizationService localizationService;
        
        public event Action<bool> OnSoundEnabledChanged;
        public event Action<bool> OnMusicEnabledChanged;
        public event Action<int> OnGraphicsQualityChanged;
        public event Action<SystemLanguage> OnLanguageChanged;
        
        public bool IsSoundEnabled { get; private set; } = true;
        public bool IsMusicEnabled { get; private set; } = true;
        public int GraphicsQuality { get; private set; } = 1; // 0 - низкое, 1 - среднее, 2 - высокое
        
        public SettingsService(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }
        
        [LogMethod(LogLevel.Debug)]
        public void Initialize()
        {
            LoadSettings();
            ApplySettings();
        }
        
        private void LoadSettings()
        {
            try
            {
                var saveData = YandexGame.savesData;
                if (saveData == null)
                {
                    Logs.Warning("SaveData is null, using default settings");
                    return;
                }

                IsSoundEnabled = saveData.IsSoundEnabled;
                IsMusicEnabled = saveData.IsMusicEnabled;
                GraphicsQuality = saveData.GraphicsQuality;

                var lang = LanguageUtility.IsoCodeToSystemLanguage(saveData.language);
                if (lang != SystemLanguage.Unknown)
                {
                    localizationService.SetLanguage(lang);
                }

                Logs.Info($"Settings loaded - Sound: {IsSoundEnabled}, Music: {IsMusicEnabled}, Graphics: {GraphicsQuality}");
            }
            catch
            {
                Logs.Warning("Failed to load settings from YandexGame, using defaults");
            }
        }
        
        private void ApplySettings()
        {
            if (GraphicsQuality >= 0 && GraphicsQuality < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(GraphicsQuality);
                Logs.Debug($"Applied graphics quality: {GraphicsQuality}");
            }
            else
            {
                Logs.Warning($"Invalid graphics quality: {GraphicsQuality}, using default");
                GraphicsQuality = 1;
                QualitySettings.SetQualityLevel(GraphicsQuality);
            }
        }
        
        public void SetSoundEnabled(bool enabled)
        {
            if (IsSoundEnabled == enabled)
                return;
                
            var oldValue = IsSoundEnabled;
            IsSoundEnabled = enabled;
            OnSoundEnabledChanged?.Invoke(enabled);
            
            Logs.Info($"Sound setting changed: {oldValue} → {enabled}");
        }
        
        public void SetMusicEnabled(bool enabled)
        {
            if (IsMusicEnabled == enabled)
                return;
                
            var oldValue = IsMusicEnabled;
            IsMusicEnabled = enabled;
            OnMusicEnabledChanged?.Invoke(enabled);
            
            Logs.Info($"Music setting changed: {oldValue} → {enabled}");
        }
        
        public void SetGraphicsQuality(int quality)
        {
            if (quality < 0 || quality >= QualitySettings.names.Length)
            {
                Logs.Warning($"Invalid graphics quality: {quality}");
                return;
            }
            
            if (GraphicsQuality == quality)
                return;
                
            var oldValue = GraphicsQuality;
            GraphicsQuality = quality;
            QualitySettings.SetQualityLevel(quality);
            OnGraphicsQualityChanged?.Invoke(quality);
            
            Logs.Info($"Graphics quality changed: {oldValue} → {quality}");
        }
        
        public void SetLanguage(SystemLanguage language)
        {
            localizationService.SetLanguage(language);
            OnLanguageChanged?.Invoke(language);
        }
        
        public SystemLanguage[] GetAvailableLanguages()
        {
            return localizationService.GetAvailableLanguages();
        }
    }
} 