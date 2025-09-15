using System;
using System.Collections.Generic;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Cosmetic.UI
{
    
    public class DropdownLocalizer : MonoBehaviour
    {
        [Serializable]
        public class LocalizedDropdownItem
        {
            public string textID;
            public string prefix;
            public string suffix;
        }
        
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private LocalizedDropdownItem[] dropdownItems;
        
        private ILocalizationService localizationService;
        
        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
            localizationService.OnLanguageChanged += UpdateDropdownItems;
            
            UpdateDropdownItems();
        }
        
        private void OnDestroy()
        {
            if (localizationService != null)
                localizationService.OnLanguageChanged -= UpdateDropdownItems;
        }
        
        private void UpdateDropdownItems()
        {
            if (dropdown == null || dropdownItems == null || dropdownItems.Length == 0)
                return;
                
            var options = new List<TMP_Dropdown.OptionData>();
            
            foreach (var item in dropdownItems)
            {
                if (!string.IsNullOrEmpty(item.textID))
                {
                    string translatedText = localizationService.GetText(item.textID);
                    string optionText = $"{item.prefix}{translatedText}{item.suffix}";
                    options.Add(new TMP_Dropdown.OptionData(optionText));
                }
            }
            
            dropdown.options = options;
        }
    }
} 