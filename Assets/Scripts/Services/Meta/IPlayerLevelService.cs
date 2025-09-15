using System;

namespace Game.Services.Meta
{
    public interface IPlayerLevelService
    {
        event Action<int> OnExperienceChanged;
        event Action<int> OnLevelChanged;
        
        int CurrentLevel { get; }
        float LevelProgress { get; }
        
        void Initialize(int startExperience = 0);
        void AddExperience(int expAmount);
        void LoseExperience(int expAmount);
    }
} 