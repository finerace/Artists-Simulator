using System;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using UnityEngine;
using Zenject;
using Game.Services.Common.Logging;

namespace Game.Services.Core
{
    public class PseudoEnemyService : ITickable
    {
        private readonly IPlayerLevelService playerLevelService;
        
        private bool isActive;
        private float enemyScore;
        private float matchTimer;
        private float difficultyMultiplier;
        private bool isLightMode;
        private bool matchInProgress;
        
        private float enemyInstructionScore; 
        private float instructionTimer;
        
        public event Action<float> OnEnemyScoreChanged;
        public event Action<float> OnEnemyInstructionCompleted;
        
        private CompetitiveGameConfig config => ConfigsProxy.CompetitiveGameConfig;

        public float EnemyScore => enemyScore;
        public float MatchTimeRemaining => matchTimer;
        public bool IsLightMode => isLightMode;
        
        public PseudoEnemyService(IPlayerLevelService playerLevelService)
        {
            this.playerLevelService = playerLevelService;
        }
        
        public void StartNewMatch(bool isLightMode)
        {
            this.isLightMode = isLightMode;
            
            difficultyMultiplier = config.GetDifficultyMultiplier(playerLevelService.CurrentLevel);

            enemyScore = 0;
            enemyInstructionScore = 0;
            matchTimer = config.MatchDurationSeconds;
            matchInProgress = true;
            isActive = true;
            
            ResetInstructionTimer();
            
            OnEnemyScoreChanged?.Invoke(enemyScore);
            
            Logs.Debug($"Enemy match started. Light mode: {isLightMode}");
        }
        
        public void StopMatch()
        {
            if (!matchInProgress)
                return;
                
            matchInProgress = false;
            isActive = false;
            
            Logs.Debug($"Enemy match stopped. Final score: {enemyScore:F2}");
        }
        
        public void SetActive(bool active)
        {
            if (isActive != active)
                Logs.Debug($"PseudoEnemyService active state changed: {isActive} → {active}");
                
            isActive = active;
            
            if (!active)
            {
                matchInProgress = false;
            }
            else
            {
                enemyInstructionScore = 0;
                ResetInstructionTimer();
            }
        }
        
        public float GetMatchProgress()
        {
            if (!matchInProgress) return 0f;
            
            float totalTime = config.MatchDurationSeconds;
            float elapsed = totalTime - matchTimer;
            return Mathf.Clamp01(elapsed / totalTime);
        }
        
        private void ResetInstructionTimer()
        {
            float matchProgress = GetMatchProgress();
            float matchProgressMultiplier = config.GetMatchProgressMultiplier(matchProgress);
            
            float speedDivider = difficultyMultiplier * matchProgressMultiplier;
            if (isLightMode)
                speedDivider *= config.EnemyLightModeMultiplier;
            
            float baseDuration = config.EnemyMaxInstructionDuration / speedDivider;
            float randomFactor = UnityEngine.Random.Range(0.9f, 1.1f);
            
            instructionTimer = baseDuration * randomFactor;
            
            instructionTimer = Mathf.Clamp(
                instructionTimer,
                config.EnemyMinInstructionDuration,
                config.EnemyMaxInstructionDuration);
        }
        
        public void Tick()
        {
            if (!isActive || !matchInProgress)
                return;
                
            matchTimer -= Time.deltaTime;
            
            instructionTimer -= Time.deltaTime;
            
            UpdateEnemyInstructionScore();
            
            if (instructionTimer <= 0)
            {
                CompleteEnemyInstruction();
            }
        }
        
        private void UpdateEnemyInstructionScore()
        {
            float baseScorePerInstruction = config.EnemyBaseAccuracyScore;
            
            if (isLightMode)
            {
                baseScorePerInstruction *= config.EnemyLightModeMultiplier;
            }
            
            float completionPercentage = 1.0f - (instructionTimer / GetMaxInstructionDuration());
            
            float scoreProgress = Mathf.Clamp01(completionPercentage) * baseScorePerInstruction;
            
            enemyInstructionScore = scoreProgress * config.EnemyScoreGainRate;
        }
        
        private float GetMaxInstructionDuration()
        {
            if (isLightMode)
            {
                return config.EnemyMaxInstructionDuration / config.EnemyLightModeMultiplier;
            }
            
            return config.EnemyMaxInstructionDuration;
        }
        
        private void CompleteEnemyInstruction()
        {
            if (enemyInstructionScore > 0)
            {
                float finalMultiplier = UnityEngine.Random.Range(
                    config.EnemyInstructionMinRandomFactor, 
                    config.EnemyInstructionMaxRandomFactor);
                    
                float finalInstructionScore = enemyInstructionScore * finalMultiplier;
                
                enemyScore += finalInstructionScore;
                
                OnEnemyInstructionCompleted?.Invoke(finalInstructionScore);
                OnEnemyScoreChanged?.Invoke(enemyScore);
                
                Logs.Debug($"Enemy instruction completed. Score gained: {finalInstructionScore:F2}, Total: {enemyScore:F2}");
            }
            
            enemyInstructionScore = 0;
            ResetInstructionTimer();
        }
    }
} 