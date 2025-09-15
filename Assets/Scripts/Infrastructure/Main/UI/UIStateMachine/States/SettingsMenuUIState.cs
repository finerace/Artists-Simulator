using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Additionals;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using UnityEngine;
using TMPro;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class SettingsMenuUIState : UniversalMenuUIState
    {
        private readonly SettingsService settingsService;
        private readonly ILocalizationService localizationService;
        
        private SettingsMenuView settingsMenuView;
        
        public SettingsMenuUIState(
            IAssetsService assetsService, 
            MainScreenProxy mainScreenProxy,
            SettingsService settingsService,
            ILocalizationService localizationService) 
            : base(assetsService, mainScreenProxy)
        {
            this.settingsService = settingsService;
            this.localizationService = localizationService;
        }
        
        public override async UniTask Enter()
        {
            universalMenuView = await assetsService.GetAsset<SettingsMenuView>(ConfigsProxy.AssetsPathsConfig.SettingsMenuID);
            settingsMenuView = (SettingsMenuView)universalMenuView;
            
            InitializeSettings();
            SetupListeners();

            await base.Enter();
        }
        
        public override UniTask Exit()
        {
            RemoveListeners();
            return base.Exit();
        }
        
        private void InitializeSettings()
        {
            settingsMenuView.SoundToggle.isOn = settingsService.IsSoundEnabled;
            settingsMenuView.MusicToggle.isOn = settingsService.IsMusicEnabled;
            
            settingsMenuView.GraphicsDropdown.value = settingsService.GraphicsQuality;
            
            InitializeLanguageDropdown();
        }
        
        private void InitializeLanguageDropdown()
        {
            var languages = settingsService.GetAvailableLanguages();
            var options = new List<TMP_Dropdown.OptionData>();
            
            var languageMapping = new Dictionary<int, SystemLanguage>();
            int currentLanguageIndex = 0;
            int index = 0;
            
            foreach (var language in languages)
            {
                string displayName = LanguageUtility.GetNativeName(language);
                options.Add(new TMP_Dropdown.OptionData(displayName));
                languageMapping[index] = language;
                
                if (language == localizationService.CurrentLanguage)
                {
                    currentLanguageIndex = index;
                }
                
                index++;
            }
            
            settingsMenuView.LanguageDropdown.ClearOptions();
            settingsMenuView.LanguageDropdown.AddOptions(options);
            settingsMenuView.LanguageDropdown.value = currentLanguageIndex;
            
            settingsMenuView.LanguageDropdown.gameObject.AddComponent<LanguageDropdownMapping>().Initialize(languageMapping);
        }
        
        private void SetupListeners()
        {
            settingsMenuView.SoundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            settingsMenuView.MusicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            settingsMenuView.GraphicsDropdown.onValueChanged.AddListener(OnGraphicsDropdownChanged);
            settingsMenuView.LanguageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        }
        
        private void RemoveListeners()
        {
            settingsMenuView.SoundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
            settingsMenuView.MusicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            settingsMenuView.GraphicsDropdown.onValueChanged.RemoveListener(OnGraphicsDropdownChanged);
            settingsMenuView.LanguageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
        }
        
        private void OnSoundToggleChanged(bool isOn)
        {
            settingsService.SetSoundEnabled(isOn);
        }
        
        private void OnMusicToggleChanged(bool isOn)
        {
            settingsService.SetMusicEnabled(isOn);
        }
        
        private void OnGraphicsDropdownChanged(int value)
        {
            settingsService.SetGraphicsQuality(value);
        }
        
        private void OnLanguageDropdownChanged(int value)
        {
            var mapping = settingsMenuView.LanguageDropdown.GetComponent<LanguageDropdownMapping>();
            if (mapping != null && mapping.LanguageMapping.TryGetValue(value, out SystemLanguage language))
            {
                settingsService.SetLanguage(language);
            }
        }
        
        private class LanguageDropdownMapping : MonoBehaviour
        {
            public Dictionary<int, SystemLanguage> LanguageMapping { get; private set; }
            
            public void Initialize(Dictionary<int, SystemLanguage> mapping)
            {
                LanguageMapping = mapping;
            }
        }
    }
}