using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using Game.Services.Common.Logging;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class CharacterCustomisationUIState : UniversalMenuUIState
    {
        private readonly CamerasService camerasService;
        private readonly MainLocationProxy mainLocationProxy;
        private readonly ICharactersServiceFacade charactersService;
        
        private IShopPresenter shopPresenter;

        public CharacterCustomisationUIState(IAssetsService assetsService, MainScreenProxy mainScreenProxy,
            CamerasService camerasService,
            MainLocationProxy mainLocationProxy,
            ICharactersServiceFacade charactersService) 
            : base(assetsService, mainScreenProxy)
        {
            this.camerasService = camerasService;
            this.mainLocationProxy = mainLocationProxy;
            this.charactersService = charactersService;
        }
        
        public override async UniTask Enter()
        {
            var shopInstaller =
                await assetsService
                    .GetAsset<ShopInstaller>(ConfigsProxy.AssetsPathsConfig.CustomisationMenuID);
            
            shopPresenter = shopInstaller.ShopPresenter;
            universalMenuView = shopInstaller.UniversalMenuView;
            
            camerasService.MoveMainCameraToPoint(mainLocationProxy.MainLocation.CharacterCustomisationCameraPoint)
                .Forget();
            
            if (shopPresenter == null)
                Logs.Error("Shop presenter is null");
            
            shopPresenter.Initialize().Forget();

            var character = charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            if (character != null)
            {
                character.transform.position = mainLocationProxy.MainLocation.MainCharacterShopPoint.position;
                character.transform.rotation = mainLocationProxy.MainLocation.MainCharacterShopPoint.rotation;
            }
            
            await base.Enter();
        }
        
        public override async UniTask Exit()
        {
            await shopPresenter.DisposeAsync();
            
            await base.Exit();
            
            var character = charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            if (character != null)
            {
                character.transform.position = mainLocationProxy.MainLocation.MainCharacterDefaultPoint.position;
                character.transform.rotation = mainLocationProxy.MainLocation.MainCharacterDefaultPoint.rotation;
            }
        }
    }
}