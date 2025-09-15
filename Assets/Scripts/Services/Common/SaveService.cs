using Game.Infrastructure.Configs;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;
using Game.Services.Meta;
using UnityEngine;
using YG;
using Zenject;
using Cysharp.Threading.Tasks;

namespace Game.Services.Common
{
    /*public class SaveLoadService : ISaveLoadService, ITickable
    {
        private readonly ICurrenciesService currenciesService;
        private readonly ICharacterItemsShopService characterItemsShopService;
        private readonly ILocationImprovementsService locationImprovementsService;
        private readonly IPlayerLevelService playerLevelService;
        private readonly SettingsService settingsService;
        private readonly ICharactersServiceFacade charactersService;
        private float debounceDelay => Game.Infrastructure.Configs.ConfigsProxy.AssetsPathsConfig.SaveDebounceDelay;
        private float maxDelay => Game.Infrastructure.Configs.ConfigsProxy.AssetsPathsConfig.SaveMaxDelay;
        private float debounceTimer;
        private float maxTimer;
        private bool timerActive;
        
        public SaveLoadService(
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
        
        public async UniTask Load()
        {
            await UniTask.WaitUntil(() => YandexGame.SDKEnabled);
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
                
                Logs.Info("SaveService: Save");
                
                YandexGame.SaveProgress();
            }
        }
    }*/
}