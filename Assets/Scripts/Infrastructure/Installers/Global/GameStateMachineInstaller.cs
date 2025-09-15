using Game.Infrastructure.Main;
using Game.Infrastructure.Main.UI.States;
using Zenject;

namespace Game.Infrastructure.Installers.Global
{
    public class GameStateMachineInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle();
            
            BindStates();
        }

        private void BindStates()
        {
            Container.Bind<BootState>().AsSingle();
            Container.Bind<MainMenuState>().AsSingle();
            
            // Вместо привязки GamePlayState добавляем привязки для конкретных реализаций
            Container.Bind<LightGamePlayState>().AsSingle();
            Container.Bind<HardGamePlayState>().AsSingle();
        }
    }
}