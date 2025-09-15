using System;

namespace Game.Services.Core
{
    public interface IRewardCalculatorService
    {
        MatchResult CalculateAndApplyRewards(MatchResult gameResult);
    }
} 