using Game.Infrastructure.Configs;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Core
{
    public class RewardCalculatorService : IRewardCalculatorService
    {
        private readonly ICurrenciesService currenciesService;
        private readonly IPlayerLevelService playerLevelService;
        
        public RewardCalculatorService(ICurrenciesService currenciesService, IPlayerLevelService playerLevelService)
        {
            this.currenciesService = currenciesService;
            this.playerLevelService = playerLevelService;
        }
        
        public MatchResult CalculateAndApplyRewards(MatchResult gameResult)
        {
            var config = ConfigsProxy.CompetitiveGameConfig;
            
            bool isVictory = gameResult.ResultType == GameResultType.PlayerVictory;
            float rewardMultiplier = config.CalculateRewardMultiplier(gameResult.PlayerScore, gameResult.EnemyScore, isVictory);
            
            int coinsReward = 0;
            int crystalsReward = 0;
            int expReward = 0;
            
            switch (gameResult.ResultType)
            {
                case GameResultType.PlayerVictory:
                    coinsReward = Mathf.RoundToInt(config.VictoryCoins * rewardMultiplier);
                    crystalsReward = Mathf.RoundToInt(config.VictoryGems * rewardMultiplier);
                    expReward = Mathf.RoundToInt(config.VictoryExp * rewardMultiplier);
                    
                    currenciesService.AddCoins(coinsReward);
                    currenciesService.AddCrystals(crystalsReward);
                    playerLevelService.AddExperience(expReward);
                    break;
                    
                case GameResultType.EnemyVictory:
                    coinsReward = Mathf.RoundToInt(config.DefeatCoins * rewardMultiplier);
                    expReward = Mathf.RoundToInt(config.DefeatExp * rewardMultiplier);
                    
                    currenciesService.AddCoins(coinsReward);
                    playerLevelService.LoseExperience(Mathf.Abs(expReward));
                    break;
                    
                case GameResultType.Tie:
                    coinsReward = Mathf.RoundToInt(config.TieCoins * rewardMultiplier);
                    expReward = Mathf.RoundToInt(config.TieExp * rewardMultiplier);
                    
                    currenciesService.AddCoins(coinsReward);
                    playerLevelService.AddExperience(expReward);
                    break;
            }
            
            return new MatchResult(gameResult.ResultType, gameResult.PlayerScore, gameResult.EnemyScore,
                                  coinsReward, crystalsReward, expReward);
        }
    }
} 