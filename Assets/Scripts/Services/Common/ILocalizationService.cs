using System;
using UnityEngine;

namespace Game.Services.Common
{
    public interface ILocalizationService
    {
        event Action OnLanguageChanged;
        SystemLanguage CurrentLanguage { get; }
        
        void Initialize();
        string GetText(string key);
        void SetLanguage(SystemLanguage language);
        SystemLanguage[] GetAvailableLanguages();
    }
} 