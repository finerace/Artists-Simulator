using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Installers.Boot
{
    public class BootstrapInstaller : MonoInstaller
    {
        [SerializeField] private GameBootstrapper gameBootstrapper;

        public override void InstallBindings()
        {
            Container.Inject(gameBootstrapper);
        }
    }
}