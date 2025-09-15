using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.Locations;
using Game.Infrastructure.Main.UI;
using Game.Infrastructure.Main.UI.States;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Additional.MagicAttributes;

namespace Game.Infrastructure.Main
{
    
    public class MainMenuState : IEnterableState
    {
        private readonly CamerasService camerasService;
        private readonly MainLocationProxy mainLocationProxy;
        private readonly ICharactersServiceFacade charactersService;
        private readonly UIStateMachine uiStateMachine;
        
        internal MainMenuState(
            CamerasService camerasService,
            MainLocationProxy mainLocationProxy,
            ICharactersServiceFacade charactersService,
            UIStateMachine uiStateMachine)
        {
            this.camerasService = camerasService;
            this.mainLocationProxy = mainLocationProxy;
            this.charactersService = charactersService;
            this.uiStateMachine = uiStateMachine;
        }
        
        public async UniTask Enter()
        {
            camerasService.SetMainCameraToPoint(mainLocationProxy.MainLocation.MainMenuCameraPoint);
            
            MainCharacterSetPos();
            void MainCharacterSetPos()
            {
                var characterPos = mainLocationProxy.MainLocation.MainCharacterDefaultPoint;
                charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId).transform
                    .SetPositionAndRotation(characterPos.position, characterPos.rotation);
            }

            await uiStateMachine.WaitAwaiting();
            
            uiStateMachine.EnterState<MainMenuUIState>().Forget();
        }
    }
}