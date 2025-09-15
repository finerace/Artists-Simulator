using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;

namespace Game.Infrastructure.Main.UI.States
{
    public class EnemySearchMenuUIState : UniversalMenuUIState
    {
        
        public EnemySearchMenuUIState(IAssetsService assetsService, 
            MainScreenProxy mainScreenProxy) 
            : base(assetsService, mainScreenProxy)
        {}

        public override async UniTask Enter()
        {
            universalMenuView =
                await assetsService.GetAsset<UniversalMenuView>(ConfigsProxy.AssetsPathsConfig.EnemySearchMenuID);

            var enemySearchMenuView = 
                universalMenuView.GetComponent<EnemySearchMenuView>();
            
            enemySearchMenuView.StartLoadingCircleAnimation();
            enemySearchMenuView.StartSearchLabelAnimation();

            await base.Enter();
        }

        public override UniTask Exit()
        {
            return base.Exit();
        }
    }
}