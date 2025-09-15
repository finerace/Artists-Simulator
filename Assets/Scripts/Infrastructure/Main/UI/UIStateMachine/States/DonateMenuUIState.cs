using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;

namespace Game.Infrastructure.Main.UI.States
{
    public class DonateMenuUIState : UniversalMenuUIState
    {
        public DonateMenuUIState(IAssetsService assetsService, MainScreenProxy mainScreenProxy) 
            : base(assetsService, mainScreenProxy)
        {}

        public override async UniTask Enter()
        {
            universalMenuView =
                await assetsService.GetAsset<UniversalMenuView>(ConfigsProxy.AssetsPathsConfig.DonateMenuID);

            await base.Enter();
        }
    }
}