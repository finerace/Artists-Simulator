using Cysharp.Threading.Tasks;
using Game.Infrastructure.Main.Locations;
using Game.Services.Meta;
using Game.Services.Common;
using System;
using Zenject;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;

namespace Game.Services.Core
{
    
    public class PaintGameplayGenerationService : ITickable
    {
        private readonly PathGenerationService pathGenerationService;
        private readonly IPlayerLevelService playerLevelService;
        private readonly MatchManager matchManager;
        private readonly IRewardCalculatorService rewardCalculatorService;
        private readonly PaintAccuracyService paintAccuracyService;
        private readonly PaintingService paintingService;
        
        private bool isLightMode;

        public event Action<MatchResult> OnCompetitiveGameFinished
        {
            add => matchManager.OnCompetitiveGameFinished += value;
            remove => matchManager.OnCompetitiveGameFinished -= value;
        }
        public event Action<float> OnTotalScoreChanged
        {
            add => matchManager.OnTotalScoreChanged += value;
            remove => matchManager.OnTotalScoreChanged -= value;
        }
        public event Action<float> OnMatchTimerUpdated
        {
            add => matchManager.OnMatchTimerUpdated += value;
            remove => matchManager.OnMatchTimerUpdated -= value;
        }
        
        public float CurrentMatchScore => matchManager.CurrentMatchScore;
        public MatchResult LastMatchResult => matchManager.LastMatchResult;
        public float VictoryScoreThreshold => matchManager.VictoryScoreThreshold;
        
        [Inject]
        public PaintGameplayGenerationService(
            IAssetsService assetsService,
            PaintAccuracyService paintAccuracyService,
            PaintingService paintingService,
            IPlayerLevelService playerLevelService,
            ICurrenciesService currenciesService,
            PseudoEnemyService pseudoEnemyService)
        {
            this.playerLevelService = playerLevelService;
            this.paintAccuracyService = paintAccuracyService;
            this.paintingService = paintingService;
            
            rewardCalculatorService = new RewardCalculatorService(currenciesService, playerLevelService);
            matchManager = new MatchManager(pseudoEnemyService);
            pathGenerationService = new PathGenerationService(assetsService);
        }
        
        [LogMethod(LogLevel.Debug)]
        public void Initialize(MainLocationProxy mainLocationProxy)
        {
            matchManager.Initialize();
            pathGenerationService.Initialize(mainLocationProxy.MainLocation.MainPaintingSurface);
                
            SubscribeToPathGenerationEvents();
        }
        
        public void Tick()
        {
            matchManager.Tick();
        }
        
        [LogMethod(LogLevel.Info, LogLevel.Debug)]
        public async UniTask GeneratePaintPaths(bool isLightMode = false)
        {
            this.isLightMode = isLightMode;
                
            if (!matchManager.IsActiveGameplay)
                matchManager.StartNewMatch(isLightMode, playerLevelService.CurrentLevel);
                
            paintAccuracyService.SetActive(false);
            paintingService.SetInputActive(false);
                
            var (paintPath, pathProgress) = await pathGenerationService.GenerateNewPath(isLightMode);
                
            if (paintPath != null && pathProgress != null)
            {
                paintAccuracyService.SetNewPath(paintPath, pathProgress, isLightMode);
                    
                paintingService.SetInputActive(true);
                paintAccuracyService.SetActive(true);
                    
                if (isLightMode)
                    paintingService.SetFollowTarget(pathProgress.PathDotT);
            }
        }
        
        private void ProcessMatchResults(MatchResult result)
        {
            var fullResult = rewardCalculatorService.CalculateAndApplyRewards(result);
            matchManager.SetLastMatchResult(fullResult);
        }
        
        private void SubscribeToPathGenerationEvents()
        {
            pathGenerationService.OnPathDragStarted += () => paintAccuracyService.SetCalculationsStopActive(false);
            pathGenerationService.OnPathDragStopped += () => paintAccuracyService.SetCalculationsStopActive(true);
            pathGenerationService.OnPathProgressComplete += OnPathProgressCompleted;
            matchManager.OnCompetitiveGameFinished += ProcessMatchResults;
        }
        
        private void OnPathProgressCompleted(PaintPathProgress pathProgress)
        {
            float previousInstructionScore = paintAccuracyService.GetCurrentScoreAndReset();
            if (previousInstructionScore > 0)
                matchManager.AddScore(previousInstructionScore);
                
            if (matchManager.IsActiveGameplay)
                GeneratePaintPaths(isLightMode).Forget();
        }
        
        public async UniTask SetActive(bool isActive)
        {
            if (!isActive)
            {
                await pathGenerationService.WaitForPathGenerationComplete();

                pathGenerationService.UnsubscribeFromEvents();
                matchManager.StopMatch();

                await pathGenerationService.Clear();
            }
                
            Logs.Debug($"PaintGameplayGenerationService active: {isActive}");
        }
    }
    
    public enum GameResultType
    {
        PlayerVictory,
        EnemyVictory,
        Tie,
        TimeUp
    }
    
    public class MatchResult
    {
        public GameResultType ResultType { get; }
        public float PlayerScore { get; }
        public float EnemyScore { get; }
        public int CoinsReward { get; }
        public int GemsReward { get; }
        public int ExperienceReward { get; }

        public MatchResult(GameResultType resultType, float playerScore, float enemyScore,
                          int coinsReward, int gemsReward, int experienceReward)
        {
            ResultType = resultType;
            PlayerScore = playerScore;
            EnemyScore = enemyScore;
            CoinsReward = coinsReward;
            GemsReward = gemsReward;
            ExperienceReward = experienceReward;
        }
        
        public static MatchResult CreateGameResult(GameResultType resultType, float playerScore, float enemyScore)
        {
            return new MatchResult(resultType, playerScore, enemyScore, 0, 0, 0);
        }
    }
} 