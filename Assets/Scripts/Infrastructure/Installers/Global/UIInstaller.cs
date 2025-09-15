using Game.Infrastructure.Main.UI;
using Game.Infrastructure.Main.UI.States;
using Zenject;

namespace Game.Infrastructure.Installers.Global
{
    public class UIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindUIStateMachine();
            void BindUIStateMachine()
            {
                Container.Bind<UIStateMachine>().AsSingle();

                Container.Bind<BootUIState>().AsSingle();
                Container.Bind<MainMenuUIState>().AsSingle();
                
                Container.Bind<SettingsMenuUIState>().AsSingle();
                Container.Bind<DonateMenuUIState>().AsSingle();
                Container.Bind<LocationImproveMenuUIState>().AsSingle();
                Container.Bind<CharacterCustomisationUIState>().AsSingle();
                Container.Bind<ImprovementMenuUIState>().AsSingle();
                Container.Bind<LevelBonusesMenuUIState>().AsSingle();
                
                Container.Bind<DifficultySwitchMenuUIState>().AsSingle();
                Container.Bind<EnemySearchMenuUIState>().AsSingle();
                Container.Bind<GamePlayMenuUIState>().AsSingle();
            }

            BindUIServices();
            void BindUIServices()
            {
                Container.Bind<LoadingScreenService>().AsSingle();
                Container.Bind<MainScreenProxy>().AsSingle();
            }
        }
    }
}
