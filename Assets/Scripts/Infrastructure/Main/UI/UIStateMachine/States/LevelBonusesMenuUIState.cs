using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class LevelBonusesMenuUIState : UniversalMenuUIState
    {
        public LevelBonusesMenuUIState(IAssetsService assetsService, MainScreenProxy mainScreenProxy) 
            : base(assetsService, mainScreenProxy)
        {}

        public override async UniTask Enter()
        {
            universalMenuView =
                await assetsService.GetAsset<UniversalMenuView>(ConfigsProxy.AssetsPathsConfig.LevelBonusesMenuID);

            await base.Enter();
        }
    }
}