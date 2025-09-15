using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class MainMenuUIState : IEnterableState, IExitableState
    {
        private readonly MainScreenProxy mainScreenProxy;
        private readonly CamerasService camerasService;
        private readonly MainLocationProxy mainLocationProxy;
        private readonly ICharactersServiceFacade charactersService;
        private readonly IPopupService popupService;

        public MainMenuUIState(
            MainScreenProxy mainScreenProxy,
            CamerasService camerasService,
            MainLocationProxy mainLocationProxy,
            ICharactersServiceFacade charactersService,
            IPopupService popupService)
        {
            this.mainScreenProxy = mainScreenProxy;
            this.camerasService = camerasService;
            this.mainLocationProxy = mainLocationProxy;
            this.charactersService = charactersService;
            this.popupService = popupService;
        }
        
        public UniTask Enter()
        {
            mainScreenProxy.MainScreen.ShowPanels().Forget();
            
            camerasService.MoveMainCameraToPoint(mainLocationProxy.MainLocation.MainMenuCameraPoint).Forget();
            
            MoveCharacterToDefaultPos();
            void MoveCharacterToDefaultPos()
            {
                var posPointT = mainLocationProxy.MainLocation.MainCharacterDefaultPoint;
                charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId).transform
                    .SetPositionAndRotation(posPointT.position, posPointT.rotation);
            }

            MoveCurrencyPanelsToDefaultPos();
            void MoveCurrencyPanelsToDefaultPos()
            {
                var mainScreen = mainScreenProxy.MainScreen;

                mainScreen.MoveStatsPanels(mainScreen.CoinsCurrencyPoint, mainScreen.DiamondsCurrencyPoint,
                        mainScreen.PlayerPanelPoint).Forget();
            }
            
            return default;
        }

        public async UniTask Exit()
        {
            await mainScreenProxy.MainScreen.HidePanels();
        }

    }
}