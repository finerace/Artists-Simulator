using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure.Configs;
using Game.Infrastructure.FSM;
using Game.Infrastructure.Main.Locations;
using Game.Infrastructure.Main.UI;
using Game.Infrastructure.Main.UI.States;
using Game.Services.Meta;
using Game.Services.Core;
using Game.Services.Common;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Infrastructure.Main
{
    
    public abstract class GamePlayState : IEnterableState, IExitableState
    {
        protected readonly UIStateMachine uiStateMachine;
        private readonly GameStateMachine gameStateMachine;
        protected readonly CamerasService camerasService;
        protected readonly MainLocationProxy mainLocationProxy;
        protected readonly PaintingService paintingService;
        protected readonly PaintGameplayGenerationService paintGameplayGenerationService;
        protected readonly PaintAccuracyService paintAccuracyService;
        protected readonly PseudoEnemyService pseudoEnemyService;

        protected CancellationTokenSource cancelTokenSource;

        protected GamePlayState(
            UIStateMachine uiStateMachine,
            GameStateMachine gameStateMachine,
            CamerasService camerasService,
            MainLocationProxy mainLocationProxy,
            PaintingService paintingService,
            PaintGameplayGenerationService paintGameplayGenerationService,
            PaintAccuracyService paintAccuracyService,
            PseudoEnemyService pseudoEnemyService)
        {
            this.uiStateMachine = uiStateMachine;
            this.gameStateMachine = gameStateMachine;
            this.camerasService = camerasService;
            this.mainLocationProxy = mainLocationProxy;
            this.paintingService = paintingService;
            this.paintGameplayGenerationService = paintGameplayGenerationService;
            this.paintAccuracyService = paintAccuracyService;
            this.pseudoEnemyService = pseudoEnemyService;
        }

        public virtual async UniTask Enter()
        {
            await uiStateMachine.EnterState<EnemySearchMenuUIState>();
            
            var enemySearchTime = Random.Range(0.5f, 2f);
            
            cancelTokenSource = new CancellationTokenSource();
            var waitTask = UniTask.Delay((int)(enemySearchTime * 1000), 
                cancellationToken: cancelTokenSource.Token);
            
            waitTask.GetAwaiter().OnCompleted(StartGameplay);
            
            async void StartGameplay()
            {
                Logs.Info("Attempt to start gameplay");
                
                if(waitTask.Status != UniTaskStatus.Succeeded ||
                   (cancelTokenSource != null && cancelTokenSource.Token.IsCancellationRequested))
                {
                    Logs.Info("Gameplay start is cancelled");
                    return;
                }
                
                await uiStateMachine.EnterState<GamePlayMenuUIState>();
                await camerasService.MoveMainCameraToPoint(mainLocationProxy.MainLocation.GamePlayCameraPoint);
                
                if(cancelTokenSource != null && cancelTokenSource.IsCancellationRequested)
                {
                    Logs.Info("Gameplay start is cancelled");
                    return;
                }
                
                paintGameplayGenerationService.OnCompetitiveGameFinished += HandleCompetitiveGameFinished;
                
                ActivateGameplayServices();
                DisposeCancelTokenSource();
            
                Logs.Info("Gameplay started");
            }
        }

        public virtual async UniTask Exit()
        {
            cancelTokenSource?.Cancel();
            
            paintingService.MoveBrushToStartPos();

            await DeactivateGameplayServices();
            
            var paintSurface = mainLocationProxy.MainLocation.MainPaintingSurface;
            paintSurface.ClearTexture(ConfigsProxy.PaintGameplaySystemsConfig.StartTextureColor);
            
            paintGameplayGenerationService.OnCompetitiveGameFinished -= HandleCompetitiveGameFinished;
            
            await uiStateMachine.WaitAwaiting();
            uiStateMachine.EnterState<MainMenuUIState>().Forget();
            await uiStateMachine.WaitAwaiting();
            
            await camerasService.MoveMainCameraToPoint(mainLocationProxy.MainLocation.MainMenuCameraPoint);
            
            DisposeCancelTokenSource();
        }
        
        private void HandleCompetitiveGameFinished(MatchResult result)
        {
            gameStateMachine.EnterState<MainMenuState>();
        }
        
        protected virtual void ActivateGameplayServices()
        {
            Logs.Debug("ActivateGameplayServices");
            
            paintingService.SetActive(true);
            paintAccuracyService.SetActive(true);
            pseudoEnemyService.SetActive(true);
            paintGameplayGenerationService.SetActive(true).Forget();
        }
        
        protected virtual async UniTask DeactivateGameplayServices()
        {
            Logs.Debug("DeactivateGameplayServices");
            
            paintingService.SetActive(false);
            paintAccuracyService.SetActive(false);
            pseudoEnemyService.SetActive(false);
            await paintGameplayGenerationService.SetActive(false);
        }
        
        private void DisposeCancelTokenSource()
        {
            if(cancelTokenSource == null) 
                return;
            
            cancelTokenSource.Dispose();
            cancelTokenSource = null;
        }
    }
} 