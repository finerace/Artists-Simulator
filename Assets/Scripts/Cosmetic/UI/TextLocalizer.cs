using System;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Cosmetic.UI
{
    
    public class TextLocalizer : MonoBehaviour
    {
        [Serializable]
        public class LocalizedTextItem
        {
            public TextMeshProUGUI text;
            public string textID;
            public string prefix;
            public string suffix;
        }
        
        [SerializeField] private LocalizedTextItem[] textItems;
        
        private ILocalizationService localizationService;
        
        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            this.localizationService = localizationService;
            localizationService.OnLanguageChanged += UpdateAllTexts;
            
            UpdateAllTexts();
        }
        
        private void OnDestroy()
        {
            if (localizationService != null)
                localizationService.OnLanguageChanged -= UpdateAllTexts;
        }
        
        private void UpdateAllTexts()
        {
            foreach (var item in textItems)
            {
                if (item.text != null && !string.IsNullOrEmpty(item.textID))
                {
                    string translatedText = localizationService.GetText(item.textID);
                    item.text.text = $"{item.prefix}{translatedText}{item.suffix}";
                }
            }
        }
    }
} 