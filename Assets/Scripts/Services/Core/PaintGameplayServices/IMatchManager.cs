using System;
using UnityEngine;

namespace Game.Services.Core
{
    public interface IMatchManager
    {
        bool IsActiveGameplay { get; }
        bool IsMatchInitialized { get; }
        float CurrentMatchScore { get; }
        float MatchTimer { get; }
        float VictoryScoreThreshold { get; }
        MatchResult LastMatchResult { get; }
        
        event Action<MatchResult> OnCompetitiveGameFinished;
        event Action<float> OnTotalScoreChanged;
        event Action<float> OnMatchTimerUpdated;
        
        void Initialize();
        void Tick();
        void StartNewMatch(bool isLightMode, int playerLevel);
        void AddScore(float score);
        void StopMatch();
        void OnEnemyScoreChanged(float enemyScore);
        void SetLastMatchResult(MatchResult result);
    }
} 