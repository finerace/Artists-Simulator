using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class DifficultySwitchMenuUIState : UniversalMenuUIState
    {
        public DifficultySwitchMenuUIState(IAssetsService assetsService, MainScreenProxy mainScreenProxy) 
            : base(assetsService, mainScreenProxy)
        {}

        public override async UniTask Enter()
        {
            universalMenuView =
                await assetsService.GetAsset<UniversalMenuView>(ConfigsProxy.AssetsPathsConfig.DifficultySwitchMenuID);

            await base.Enter();
        }
    }
}