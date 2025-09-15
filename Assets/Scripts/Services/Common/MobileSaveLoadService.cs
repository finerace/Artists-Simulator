using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;
using Game.Services.Meta;
using UnityEngine;
using YG;
using Zenject;
using Cysharp.Threading.Tasks;
using System.IO;

namespace Game.Services.Common
{
    public class MobileSaveLoadService : ISaveLoadService, ITickable
    {
        private readonly ICurrenciesService currenciesService;
        private readonly ICharacterItemsShopService characterItemsShopService;
        private readonly ILocationImprovementsService locationImprovementsService;
        private readonly IPlayerLevelService playerLevelService;
        private readonly SettingsService settingsService;
        private readonly ICharactersServiceFacade charactersService;
        
        private float debounceDelay => ConfigsProxy.AssetsPathsConfig.SaveDebounceDelay;
        private float maxDelay => ConfigsProxy.AssetsPathsConfig.SaveMaxDelay;
        private float debounceTimer;
        private float maxTimer;
        private bool timerActive;

        private readonly string _saveFilePath;

        public MobileSaveLoadService(
            ICurrenciesService currenciesService,
            ICharacterItemsShopService characterItemsShopService,
            ILocationImprovementsService locationImprovementsService,
            IPlayerLevelService playerLevelService,
            SettingsService settingsService,
            ICharactersServiceFacade charactersService)
        {
            this.currenciesService = currenciesService;
            this.characterItemsShopService = characterItemsShopService;
            this.locationImprovementsService = locationImprovementsService;
            this.playerLevelService = playerLevelService;
            this.settingsService = settingsService;
            this.charactersService = charactersService;
            
            _saveFilePath = Path.Combine(Application.persistentDataPath, "player_progress.json");
        }
        
        [LogMethod(LogLevel.Debug, LogLevel.Info)]
        public void Initialize()
        {
            charactersService.OnCharacterGenderSwapped += (characterId, newCharacter) =>
            {
                if (characterId == ConfigsProxy.CharactersAndShopConfig.MainCharacterId && 
                    newCharacter.CustomizationTemplate != null)
                {
                    YandexGame.savesData.SaveCharacterTemplateId(newCharacter.CustomizationTemplate.TemplateId);
                    Save();
                }
            };
            
            characterItemsShopService.OnNewItemUnlocked += item =>
            {
                YandexGame.savesData.AddNewItem(item.ItemId, item.ItemColor);
                Save();
            };
            
            characterItemsShopService.OnItemColorChanged += (itemId, color) =>
            {
                YandexGame.savesData.UpdateItemColor(itemId, color);
                Save();
            };
            
            characterItemsShopService.OnPlayerCharacterAppearanceChanged += slots =>
            {
                YandexGame.savesData.SaveCharacterSlots(slots);
                Save();
            };
            
            currenciesService.OnCoinsChange += coins =>
            {
                YandexGame.savesData.SetCurrencies(coins, currenciesService.Crystals);
                Save();
            };
            
            currenciesService.OnCrystalsChange += crystals =>
            {
                YandexGame.savesData.SetCurrencies(currenciesService.Coins, crystals);
                Save();
            };
            
            locationImprovementsService.OnLocImproveUnlocked += id =>
            {
                YandexGame.savesData.AddNewUnlockedLocation(id);
                Save();
            };
            
            playerLevelService.OnExperienceChanged += experience =>
            {
                YandexGame.savesData.SetPlayerExperience(experience);
                Save();
            };
            
            settingsService.OnSoundEnabledChanged += _ => SaveSettings();
            settingsService.OnMusicEnabledChanged += _ => SaveSettings();
            settingsService.OnGraphicsQualityChanged += _ => SaveSettings();
            settingsService.OnLanguageChanged += language =>
            {
                var isoCode = Game.Infrastructure.Additionals.LanguageUtility.SystemLanguageToIsoCode(language);
                if (!string.IsNullOrEmpty(isoCode))
                {
                    YandexGame.savesData.language = isoCode;
                    Save();
                }
            };
        }
        
        private void SaveSettings()
        {
            YandexGame.savesData.SetSettings(
                settingsService.IsSoundEnabled, 
                settingsService.IsMusicEnabled, 
                settingsService.GraphicsQuality);
            
            Save();
        }
        
        public UniTask Save()
        {
            if (!timerActive)
            {
                debounceTimer = debounceDelay;
                maxTimer = maxDelay;
                timerActive = true;
            }
            else
            {
                debounceTimer = debounceDelay;
            }

            return UniTask.CompletedTask;
        }
        
        public UniTask Load()
        {
            YandexGame.savesData.Initialize();

            if (File.Exists(_saveFilePath))
            {
                string json = File.ReadAllText(_saveFilePath);
                JsonUtility.FromJsonOverwrite(json, YandexGame.savesData);
            }
            
            return UniTask.CompletedTask;
        }
        
        public void Tick()
        {
            if (!timerActive)
                return;
            
            debounceTimer -= Time.deltaTime;
            maxTimer -= Time.deltaTime;

            if (debounceTimer <= 0f || maxTimer <= 0f)
            {
                timerActive = false;
                
                string json = JsonUtility.ToJson(YandexGame.savesData, true);
                File.WriteAllText(_saveFilePath, json);
            }
        }
    }
}