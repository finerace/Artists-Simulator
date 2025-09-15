using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Core;
using Game.Services.Common;
using Game.Audio;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Installers.Global
{
    public class GlobalServicesInstaller : MonoInstaller
    {
        [SerializeField] private CoroutineRunner coroutineRunner;
        
        public override void InstallBindings()
        {
            BindCommonServices();
            void BindCommonServices()
            {
                Container.Bind<IScenesService>().To<ScenesService>().AsSingle();
                Container.Bind<ICoroutineRunner>().FromInstance(coroutineRunner).AsSingle();
                
                Container.BindInterfacesAndSelfTo<CamerasService>().AsSingle();
                Container.Bind<ICurrenciesService>().To<CurrenciesService>().AsSingle();
                Container.Bind<IPlayerLevelService>().To<PlayerLevelService>().AsSingle();
                
                Container.BindInterfacesAndSelfTo<MobileSaveLoadService>().AsSingle();
                
                Container.Bind<ICharacterItemsShopService>().To<CharacterItemsShopService>().AsSingle();
                Container.Bind<ILocationImprovementsService>().To<LocationImprovementsService>().AsSingle();
                
                Container.BindInterfacesAndSelfTo<PaintingService>().AsSingle();
                Container.BindInterfacesAndSelfTo<PaintAccuracyService>().AsSingle();
                Container.BindInterfacesAndSelfTo<PseudoEnemyService>().AsSingle();
                Container.BindInterfacesAndSelfTo<PaintGameplayGenerationService>().AsSingle();
                
                Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle();
                Container.Bind<SettingsService>().AsSingle();
                Container.Bind<IPopupService>().To<PopupService>().AsSingle();
                Container.Bind<IAudioPoolService>().To<AudioPoolService>().AsSingle();
            }

            BindLocationServices();
            void BindLocationServices()
            {
                Container.Bind<MainLocationProxy>().AsSingle();
            }
            
            BindCharacterServices();
            void BindCharacterServices()
            {
                Container.Bind<ICharacterStorageService>().To<CharacterStorageService>().AsSingle();
                Container.Bind<ICharacterCustomizationService>().To<CharacterCustomizationService>().AsSingle();
                Container.Bind<ICharacterCreationService>().To<CharacterCreationService>().AsSingle();
                Container.Bind<ICharacterGenderService>().To<CharacterGenderService>().AsSingle();
                
                Container.Bind<ICharactersServiceFacade>().To<CharactersServiceFacade>().AsSingle();
            }
        }
    }
}