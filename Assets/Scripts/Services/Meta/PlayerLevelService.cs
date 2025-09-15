using System;
using Game.Infrastructure.Configs;
using Game.Services.Common.Logging;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Services.Meta
{
    
    public class PlayerLevelService : IPlayerLevelService
    {
        private int totalExperience;
        
        public event Action<int> OnExperienceChanged;
        public event Action<int> OnLevelChanged;
        
        public int CurrentLevel => CalculateLevel(totalExperience);
        public float LevelProgress => CalculateLevelProgress();
        
        public void Initialize(int startExperience = 0)
        {
            totalExperience = startExperience;
            Logs.Debug($"PlayerLevel initialized with {startExperience} exp");
        }
        
        public void AddExperience(int expAmount)
        {
            if (expAmount <= 0)
            {
                Logs.Debug($"Invalid exp amount: {expAmount}");
                return;
            }
            
            int oldLevel = CurrentLevel;
            int oldExp = totalExperience;
            
            totalExperience += expAmount;
            OnExperienceChanged?.Invoke(totalExperience);
            
            int newLevel = CurrentLevel;
            if (newLevel != oldLevel)
            {
                Logs.Info($"Level up! {oldLevel} → {newLevel} (+{expAmount} exp)");
                OnLevelChanged?.Invoke(newLevel);
            }
            else
                Logs.Info($"Added {expAmount} exp ({oldExp} → {totalExperience})");
        }
        
        public void LoseExperience(int expAmount)
        {
            if (expAmount <= 0)
            {
                Logs.Info($"Invalid exp loss amount: {expAmount}");
                return;
            }
            
            int oldLevel = CurrentLevel;
            int oldExp = totalExperience;
            
            totalExperience = Mathf.Max(0, totalExperience - expAmount);
            OnExperienceChanged?.Invoke(totalExperience);
            
            int newLevel = CurrentLevel;
            if (newLevel != oldLevel)
            {
                Logs.Info($"Level down! {oldLevel} → {newLevel} (-{expAmount} exp)");
                OnLevelChanged?.Invoke(newLevel);
            }
            else
                Logs.Info($"Lost {expAmount} exp ({oldExp} → {totalExperience})");
        }
        
        private int CalculateLevel(int experience)
        {
            return ConfigsProxy.CompetitiveGameConfig.CalculateLevelFromExp(experience);
        }
        
        private float CalculateLevelProgress()
        {
            int currentLevel = CurrentLevel;
            int expForCurrentLevel = ConfigsProxy.CompetitiveGameConfig.CalculateExpForLevel(currentLevel);
            int expForNextLevel = ConfigsProxy.CompetitiveGameConfig.CalculateExpForLevel(currentLevel + 1);
            
            float progress = (float)(totalExperience - expForCurrentLevel) / 
                           (expForNextLevel - expForCurrentLevel);
            
            return Mathf.Clamp01(progress);
        }
        
        public float GetEnemyDifficultyMultiplier()
        {
            return ConfigsProxy.CompetitiveGameConfig.GetDifficultyMultiplier(CurrentLevel);
        }
    }
} 