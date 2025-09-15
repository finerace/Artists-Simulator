using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.UI.GamePlayUIState;
using Game.Services.Core;
using Game.Services.Meta;
using Game.Services.Common;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Infrastructure.Main.UI.States
{
    
    public class GamePlayMenuUIState : IEnterableState, IExitableState
    {
        private readonly IAssetsService assetsService;
        private readonly MainScreenProxy mainScreenProxy;
        private readonly PaintGameplayGenerationService gameplayGenerationService;
        private readonly ICharactersServiceFacade charactersService;

        private GamePlayMenuView gamePlayMenuView;
        private int delayTime
        {
            get => (int)(ConfigsProxy.CompetitiveGameConfig.MatchEndDelay * 1000);
        }
        
        public GamePlayMenuUIState(
            IAssetsService assetsService,
            MainScreenProxy mainScreenProxy,
            PaintGameplayGenerationService gameplayGenerationService,
            ICharactersServiceFacade charactersService)
        {
            this.assetsService = assetsService;
            this.mainScreenProxy = mainScreenProxy;
            this.gameplayGenerationService = gameplayGenerationService;
            this.charactersService = charactersService;
        }

        public async UniTask Enter()
        {
            gamePlayMenuView =
                await assetsService.GetAsset<GamePlayMenuView>(ConfigsProxy.AssetsPathsConfig.GamePlayMenuID);
            
            await gamePlayMenuView.Initialize(charactersService);
            
            mainScreenProxy.MainScreen.ToMainCanvas(gamePlayMenuView.GetComponent<RectTransform>());
            mainScreenProxy.MainScreen.MoveStatsPanels(gamePlayMenuView.CoinsHidePoint,
                gamePlayMenuView.CrystalsHidePoint,
                gamePlayMenuView.LevelBarHidePoint).Forget();

            gamePlayMenuView.SetMainGameplayFrameShowInstant(false);
            gamePlayMenuView.SetBattleFramesShowInstant(false);

            await gamePlayMenuView.SetBattleFramesShow(true);
            
            await UniTask.Delay(delayTime);

            await gamePlayMenuView.SetMainGameplayFrameShow(true);
            await gamePlayMenuView.SetBattleFramesShow(false);
        }

        public async UniTask Exit()
        {
            await gamePlayMenuView.SetBattleFramesShow(true);
            gamePlayMenuView.SetMainGameplayFrameShowInstant(false);
            
            await UniTask.Delay(delayTime);
            
            var result = gameplayGenerationService.LastMatchResult;
            if (result != null)
            {
                await gamePlayMenuView.ShowRewards(
                    result.CoinsReward,
                    result.GemsReward,
                    result.ExperienceReward);
                
                await gamePlayMenuView.WaitForClose();
                await gamePlayMenuView.HideRewards();
            }
            else
            {
                await gamePlayMenuView.WaitForClose();
            }
            
            await gamePlayMenuView.SetBattleFramesShow(false);
            
            gamePlayMenuView.DestroyCharacters();
            
            assetsService.ReleaseAsset(gamePlayMenuView.gameObject);
        }
    }
}