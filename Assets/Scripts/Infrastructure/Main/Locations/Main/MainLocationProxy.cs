using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;
using Game.Services.Common;

namespace Game.Infrastructure.Main.Locations
{
    
    public class MainLocationProxy
    {
        private MainLocationView mainLocation;
        private readonly IAssetsService assetsService;
        
        public MainLocationView MainLocation => mainLocation;

        public MainLocationProxy(IAssetsService assetsService)
        {
            this.assetsService = assetsService;
        }
        
        public async UniTask Initialize()
        {
            if (mainLocation != null)
                return;
            
            mainLocation = 
                await assetsService.GetAsset<MainLocationView>(ConfigsProxy.AssetsPathsConfig.MainLocationID);
        }
    }
}