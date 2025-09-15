using System.Collections.Generic;
using Game.Infrastructure.Additionals;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Services.Common
{
    public class LanguagesListGenerator : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown languageDropdown;
        
        private ILocalizationService localizationService;
        
        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
        }
        
        private void Start()
        {
            PopulateLanguagesList();
            
            if (languageDropdown != null)
                languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
        
        private void OnDestroy()
        {
            if (languageDropdown != null)
                languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
        
        private void PopulateLanguagesList()
        {
            if (languageDropdown == null)
                return;
                
            var allLanguages = LanguageUtility.GetAllDisplayNames();
            var currentLang = localizationService.CurrentLanguage;
            
            languageDropdown.ClearOptions();
            
            var options = new List<string>();
            int currentIndex = 0;
            int index = 0;
            
            foreach (var language in allLanguages)
            {
                options.Add(language.Value);
                if (language.Key == currentLang)
                    currentIndex = index;
                    
                index++;
            }
            
            languageDropdown.AddOptions(options);
            languageDropdown.value = currentIndex;
        }
        
        private void OnLanguageChanged(int value)
        {
            var allLanguages = LanguageUtility.GetAllDisplayNames();
            var languages = new List<SystemLanguage>();
            
            foreach (var language in allLanguages)
            {
                languages.Add(language.Key);
            }
            
            var selectedLanguage = languages[value];
            localizationService.SetLanguage(selectedLanguage);
        }
    }
}