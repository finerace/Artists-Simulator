using System;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.Locations;
using Game.Infrastructure.Main.UI;
using Game.Infrastructure.Main.UI.States;
using Game.Services.Meta;
using Game.Services.Core;
using Game.Services.Common;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using Game.Audio;
using UnityEngine.SceneManagement;
using YG;

namespace Game.Infrastructure.Main
{
    
    public class BootState : IEnterableState, IExitableState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly IScenesService scenesService;
        private readonly LoadingScreenService loadingScreenService;
        private readonly MainLocationProxy mainLocationProxy;
        private readonly CamerasService camerasService;
        private readonly ICharactersServiceFacade charactersService;
        private readonly UIStateMachine uiStateMachine;
        private readonly PaintingService paintingService;
        private readonly ICurrenciesService currenciesService;
        private readonly ICharacterItemsShopService characterItemsShopService;
        private readonly ISaveLoadService saveService;
        private readonly ILocationImprovementsService locationImprovementsService;
        private readonly PaintGameplayGenerationService paintGameplayGenerationService;
        private readonly IPlayerLevelService playerLevelService;
        private readonly SettingsService settingsService;
        private readonly ILocalizationService localizationService;
        private readonly IAudioPoolService audioPoolService;
        
        internal BootState(GameStateMachine gameStateMachine,
            IScenesService scenesService,
            LoadingScreenService loadingScreenService,
            MainLocationProxy mainLocationProxy,
            CamerasService camerasService,
            ICharactersServiceFacade charactersService,
            UIStateMachine uiStateMachine,
            PaintingService paintingService,
            ICurrenciesService currenciesService,
            ICharacterItemsShopService characterItemsShopService,
            ISaveLoadService saveService,
            ILocationImprovementsService locationImprovementsService,
            PaintGameplayGenerationService paintGameplayGenerationService,
            IPlayerLevelService playerLevelService,
            SettingsService settingsService,
            ILocalizationService localizationService,
            IAudioPoolService audioPoolService)
        {
            this.gameStateMachine = gameStateMachine;
            this.scenesService = scenesService;
            this.loadingScreenService = loadingScreenService;
            this.mainLocationProxy = mainLocationProxy;
            this.camerasService = camerasService;
            this.charactersService = charactersService;
            this.uiStateMachine = uiStateMachine;
            this.paintingService = paintingService;
            this.currenciesService = currenciesService;
            this.characterItemsShopService = characterItemsShopService;
            this.saveService = saveService;
            this.locationImprovementsService = locationImprovementsService;
            this.paintGameplayGenerationService = paintGameplayGenerationService;
            this.playerLevelService = playerLevelService;
            this.settingsService = settingsService;
            this.localizationService = localizationService;
            this.audioPoolService = audioPoolService;
        }
        
        public async UniTask Enter()
        {
            await InitializeScenes();
            await InitializeBaseSystems();
            await InitializeSaveDataDependentSystems();
            await InitializeUI();
        }
        
        public UniTask Exit()
        {
            loadingScreenService.Hide().Forget();
            return default;
        }

        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeScenes()
        {
            await MainSceneLoad();
            await BootSceneUnload();
        }
        
        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeBaseSystems()
        {
            localizationService.Initialize();
            camerasService.Initialize();
            audioPoolService.Initialize();
            
            await loadingScreenService.Initialize();
            await mainLocationProxy.Initialize();
            
            paintingService.Initialize();
            paintGameplayGenerationService.Initialize(mainLocationProxy);
        }
        
        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeSaveDataDependentSystems()
        {
            await saveService.Load();
            
            var saveData = YandexGame.savesData;
            saveData.Initialize();
            
            await InitializeCoreServices(saveData);
            await InitializeCharacterSystems(saveData);
            await InitializeLocationSystems(saveData);
            
            saveService.Initialize();
        }
        
        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeCoreServices(SavesYG saveData)
        {
            currenciesService.Initialize(saveData.Coins, saveData.Crystals);
            playerLevelService.Initialize(saveData.PlayerExperience);
            settingsService.Initialize();
            
            await UniTask.CompletedTask;
        }
        
        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeCharacterSystems(SavesYG saveData)
        {
            string templateId = !string.IsNullOrEmpty(saveData.CurrentCharacterTemplateId) 
                ? saveData.CurrentCharacterTemplateId 
                : ConfigsProxy.CharactersAndShopConfig.MaleTemplateId;
                
            var characterTemplate = ConfigsProxy.CharactersAndShopConfig.GetTemplateById(templateId);
            if (characterTemplate == null)
            {
                Logs.Warning($"Character template not found: {templateId}, using default");
                characterTemplate = ConfigsProxy.CharactersAndShopConfig.CharacterTemplates[0];
            }
            
            await charactersService.CreateCharacter(characterTemplate, 
                ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            
            var playerCharacter = charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            characterItemsShopService.Initialize(saveData.UnlockedItemsData);
            characterItemsShopService.SetPlayerCharacter(playerCharacter);
            
            await playerCharacter.SetSlotsSavedData(saveData.CharacterSlots);
        }
        
        private async UniTask InitializeLocationSystems(SavesYG saveData)
        {
            await locationImprovementsService.Initialize(saveData.UnlockedLocationImprovements);
        }
        
        [LogMethod(LogLevel.Debug)]
        private async UniTask InitializeUI()
        {
            uiStateMachine.Initialize();
            await uiStateMachine.EnterState<BootUIState>();
            
            gameStateMachine.EnterState<MainMenuState>().Forget();
        }

        private UniTask MainSceneLoad()
        {
            var asyncOperation = scenesService.LoadSceneAsync(ConfigsProxy.AssetsPathsConfig.MainSceneID, LoadSceneMode.Additive);
            return asyncOperation.ToUniTask();
        }

        private UniTask BootSceneUnload()
        {
            var asyncOperation = scenesService.UnloadSceneAsync(ConfigsProxy.AssetsPathsConfig.BootSceneID);
            return asyncOperation.ToUniTask();
        }
    }
}