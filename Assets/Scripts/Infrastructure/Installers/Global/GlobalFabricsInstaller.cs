using Game.Infrastructure.FSM;
using Game.Services.Meta;
using Game.Services.Common;
using Zenject;

namespace Game.Infrastructure.Installers.Global
{
    public class GlobalFabricsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<StatesFactory>().AsSingle();
            Container.Bind<IAssetsService>().To<AssetsService>().AsSingle();
        }
    }
}