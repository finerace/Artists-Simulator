using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Game.Services.Common;
using UnityEngine;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI
{
    
    public class MainScreenProxy : AssetService<MainScreenView, UniTask>
    {
        private readonly CamerasService camerasService;

        public MainScreenProxy(IAssetsService assetsService, CamerasService camerasService) : base(assetsService)
        {
            this.camerasService = camerasService;
        }
        
        public MainScreenView MainScreen => asset;

        public override async UniTask Initialize()
        {
            if(asset != null)
                return;
            
            asset = 
                await assetsService.GetAsset<MainScreenView>(ConfigsProxy.AssetsPathsConfig.MainMenuID);

            asset.transform.GetChild(0).GetComponent<Canvas>().worldCamera = 
                camerasService.MainCamera;
        }
    }
}