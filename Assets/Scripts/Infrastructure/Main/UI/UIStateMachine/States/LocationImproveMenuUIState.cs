using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Common;

namespace Game.Infrastructure.Main.UI.States
{
    public class LocationImproveMenuUIState : UniversalMenuUIState
    {
        private readonly CamerasService camerasService;
        private readonly MainLocationProxy mainLocationProxy;
        private readonly ILocationImprovementsService locationImprovementsService;

        public LocationImproveMenuUIState(IAssetsService assetsService, 
            MainScreenProxy mainScreenProxy,
            CamerasService camerasService,
            MainLocationProxy mainLocationProxy,
            ILocationImprovementsService locationImprovementsService) 
            : base(assetsService, mainScreenProxy)
        {
            this.camerasService = camerasService;
            this.mainLocationProxy = mainLocationProxy;
            this.locationImprovementsService = locationImprovementsService;
        }

        public override async UniTask Enter()
        {
            universalMenuView =
                await assetsService.GetAsset<UniversalMenuView>(ConfigsProxy.AssetsPathsConfig.LocationImproveMenuID);

            camerasService.MoveMainCameraToPoint(mainLocationProxy.MainLocation.CenterLocationImproveCameraPoint)
                .Forget();
            
            locationImprovementsService.BuildAllLocationImproves().Forget();
            
            await base.Enter();
        }
        
        public override async UniTask Exit()
        {
            locationImprovementsService.DestroyLocationImproves().Forget();
            
            await base.Exit();
        }
        
    }
}