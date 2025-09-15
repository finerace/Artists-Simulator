using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Common;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI
{
    
    public class LoadingScreenService : AssetService<LoadingScreenView, UniTask>
    {
        public LoadingScreenService(IAssetsService assetsService) : base(assetsService)
        {}
        
        public override async UniTask Initialize()
        {
            if(asset != null)
                return;
            
            asset = 
                await assetsService.GetAsset<LoadingScreenView>(ConfigsProxy.AssetsPathsConfig.LoadingMenuID);

            asset.Show().Forget();
        }
        
        public UniTask Show()
        {
            return asset.Show();
        }

        public UniTask Hide()
        {
            return asset.Hide();
        }
    }
}