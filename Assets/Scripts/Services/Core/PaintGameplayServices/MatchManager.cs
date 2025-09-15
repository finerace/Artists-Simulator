using System;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Services.Core
{
    public class MatchManager : IMatchManager
    {
        private readonly PseudoEnemyService pseudoEnemyService;
        
        private bool isActiveGameplay;
        private bool isMatchInitialized;
        private float totalMatchScore;
        private float matchTimer;
        private float victoryScoreThreshold;
        private MatchResult lastMatchResult;
        
        public bool IsActiveGameplay => isActiveGameplay;
        public bool IsMatchInitialized => isMatchInitialized;
        public float CurrentMatchScore => totalMatchScore;
        public float MatchTimer => matchTimer;
        public float VictoryScoreThreshold => victoryScoreThreshold;
        public MatchResult LastMatchResult => lastMatchResult;
        
        public event Action<MatchResult> OnCompetitiveGameFinished;
        public event Action<float> OnTotalScoreChanged;
        public event Action<float> OnMatchTimerUpdated;
        
        public MatchManager(PseudoEnemyService pseudoEnemyService)
        {
            this.pseudoEnemyService = pseudoEnemyService;
        }
        
        [LogMethod(LogLevel.None, LogLevel.Debug)]
        public void Initialize()
        {
            lastMatchResult = null;
            isActiveGameplay = false;
            isMatchInitialized = false;
            
            if (pseudoEnemyService != null)
                pseudoEnemyService.OnEnemyScoreChanged += OnEnemyScoreChanged;
        }
        
        public void Tick()
        {
            if (!isActiveGameplay || !isMatchInitialized)
                return;
            
            matchTimer -= Time.deltaTime;
            matchTimer = Mathf.Max(0, matchTimer);
            
            OnMatchTimerUpdated?.Invoke(matchTimer);

            if (matchTimer <= 0)
            {
                HandleMatchEnd(GameResultType.TimeUp);
            }
        }
        
        [LogMethod(LogLevel.Debug)]
        public void StartNewMatch(bool isLightMode, int playerLevel)
        {
            isActiveGameplay = true;
            lastMatchResult = null;
            
            if (!isMatchInitialized)
            {
                isMatchInitialized = true;
                totalMatchScore = 0f;
                matchTimer = ConfigsProxy.CompetitiveGameConfig.MatchDurationSeconds;
                victoryScoreThreshold = ConfigsProxy.CompetitiveGameConfig.CalculateVictoryScoreThreshold(playerLevel);
                
                OnMatchTimerUpdated?.Invoke(matchTimer);
                
                pseudoEnemyService.StartNewMatch(isLightMode);
            }
        }
        
        public void AddScore(float score)
        {
            if (!isActiveGameplay || !isMatchInitialized)
                return;
                
            if (score > 0)
            {
                totalMatchScore += score;
                OnTotalScoreChanged?.Invoke(totalMatchScore);
                
                CheckPlayerWin();
            }
        }
        
        [LogMethod(LogLevel.Debug)]
        public void StopMatch()
        {
            isActiveGameplay = false;
            isMatchInitialized = false;
            
            if (pseudoEnemyService != null)
            {
                pseudoEnemyService.StopMatch();
            }
        }
        
        public void OnEnemyScoreChanged(float enemyScore)
        {
            if (!isActiveGameplay || !isMatchInitialized)
                return;

            CheckPlayerWin();
            
            if (enemyScore >= victoryScoreThreshold)
            {
                HandleMatchEnd(GameResultType.EnemyVictory);
            }
        }
        
        public void SetLastMatchResult(MatchResult result)
        {
            lastMatchResult = result;
        }
        
        private void CheckPlayerWin()
        {
            if (!isActiveGameplay || !isMatchInitialized)
                return;
                
            if (totalMatchScore >= victoryScoreThreshold)
                HandleMatchEnd(GameResultType.PlayerVictory);
        }
        
        private void HandleMatchEnd(GameResultType resultType)
        {
            if (!isMatchInitialized)
                return;
            
            isMatchInitialized = false;
            isActiveGameplay = false;

            float enemyScore = pseudoEnemyService?.EnemyScore ?? 0f;
            GameResultType finalResultType;

            switch (resultType)
            {
                case GameResultType.TimeUp:
                    if (totalMatchScore > enemyScore)
                        finalResultType = GameResultType.PlayerVictory;
                    else if (enemyScore > totalMatchScore)
                        finalResultType = GameResultType.EnemyVictory;
                    else
                        finalResultType = GameResultType.Tie;
                    break;
                    
                default:
                    finalResultType = resultType;
                    break;
            }

            var gameResult = MatchResult.CreateGameResult(finalResultType, totalMatchScore, enemyScore);
            OnCompetitiveGameFinished?.Invoke(gameResult);
            
            if (pseudoEnemyService != null)
            {
                pseudoEnemyService.StopMatch();
            }
            
            Logs.Debug($"Match ended - Result: {finalResultType}, Player: {totalMatchScore}, Enemy: {enemyScore}");
        }
    }
} 