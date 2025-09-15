using Cysharp.Threading.Tasks;
using Game.Infrastructure.Main.Locations;
using Game.Infrastructure.Main.UI;
using Game.Services.Core;
using Game.Services.Meta;
using Game.Services.Common;

namespace Game.Infrastructure.Main
{
    public class HardGamePlayState : GamePlayState
    {
        public HardGamePlayState(UIStateMachine uiStateMachine, GameStateMachine gameStateMachine, CamerasService camerasService, MainLocationProxy mainLocationProxy, PaintingService paintingService, PaintGameplayGenerationService paintGameplayGenerationService, PaintAccuracyService paintAccuracyService, PseudoEnemyService pseudoEnemyService) : base(uiStateMachine, gameStateMachine, camerasService, mainLocationProxy, paintingService, paintGameplayGenerationService, paintAccuracyService, pseudoEnemyService)
        {
        }

        protected override void ActivateGameplayServices()
        {
            base.ActivateGameplayServices();
            paintGameplayGenerationService.GeneratePaintPaths(isLightMode: false).Forget();
        }
    }
} 