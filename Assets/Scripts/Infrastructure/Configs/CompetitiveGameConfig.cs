using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "CompetitiveGameConfig", menuName = "Configs/CompetitiveGameConfig", order = 1)]
    public class CompetitiveGameConfig : ScriptableObject
    {
        [Header("Main Match Settings")]
        [Tooltip("Match duration in seconds")]
        [SerializeField] private float matchDurationSeconds = 120f;
        [Tooltip("Base score threshold required for victory")]
        [SerializeField] private float baseVictoryScoreThreshold = 1000f;
        [Tooltip("Victory threshold multiplier from player level (0.05 = +5% per level)")]
        [SerializeField] private float levelVictoryThresholdMultiplier = 0.05f;
        [Tooltip("Random victory threshold variation in percent (±5%)")]
        [SerializeField] private float victoryThresholdRandomness = 0.05f;
        [Tooltip("Delay before match end after victory condition is met")]
        [SerializeField] private float matchEndDelay = 3f;
        
        [Header("Player Level Calculation Formula")]
        [Tooltip("Base experience multiplier for level formula")]
        [SerializeField] private float expBaseMultiplier = 100f;
        [Tooltip("Experience growth power (2 = quadratic)")]
        [SerializeField] private float expLevelPower = 2f;
        [Tooltip("Starting player level")]
        [SerializeField] private int startLevel = 1;
        
        [Header("Enemy Base Score Gain Rate")]
        [Tooltip("Base enemy accuracy score (higher = harder)")]
        [SerializeField] private float enemyBaseAccuracyScore = 5f;
        [Tooltip("Enemy score gain rate (higher = faster)")]
        [SerializeField] private float enemyScoreGainRate = 2f;
        
        [Header("Enemy Instruction Settings")]
        [Tooltip("Minimum time to complete instruction")]
        [SerializeField] private float enemyMinInstructionDuration = 0.8f;
        [Tooltip("Maximum time to complete instruction")]
        [SerializeField] private float enemyMaxInstructionDuration = 2.5f;
        [Tooltip("Minimum random factor when completing (0.85 = -15%)")]
        [SerializeField] private float enemyInstructionMinRandomFactor = 0.85f;
        [Tooltip("Maximum random factor when completing (1.15 = +15%)")]
        [SerializeField] private float enemyInstructionMaxRandomFactor = 1.15f;
        [Tooltip("Score gain multiplier in easy mode (0.8 = 80% of normal)")]
        [SerializeField] private float enemyLightModeMultiplier = 0.8f;
        
        [Header("Dynamic Difficulty")]
        [Tooltip("Difficulty growth multiplier per level (0.1 = +10% per level)")]
        [SerializeField] private float difficultyLevelMultiplier = 0.1f;
        
        [Header("Match Progress Difficulty")]
        [Tooltip("Enable difficulty increase during match")]
        [SerializeField] private bool enableMatchProgressDifficulty = true;
        [Tooltip("Maximum difficulty increase percentage by match end (0.3 = +30%)")]
        [SerializeField] private float matchProgressMaxBonus = 0.3f;
        [Tooltip("Match progress difficulty curve (X=0 to X=1)")]
        [SerializeField] private AnimationCurve matchProgressCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Header("Rewards and Experience")]
        [Tooltip("Base coins for victory")]
        [SerializeField] private int victoryCoins = 100;
        [Tooltip("Base gems for victory")]
        [SerializeField] private int victoryGems = 10;
        [Tooltip("Base coins for defeat")]
        [SerializeField] private int defeatCoins = 30;
        [Tooltip("Base coins for tie")]
        [SerializeField] private int tieCoins = 50;
        [Tooltip("Base experience for victory")]
        [SerializeField] private int victoryExp = 50;
        [Tooltip("Base experience loss on defeat (negative number)")]
        [SerializeField] private int defeatExp = -15;
        [Tooltip("Base experience for tie")]
        [SerializeField] private int tieExp = 10;
        
        [Header("Dynamic Reward Modifiers")]
        [Tooltip("Minimum victory reward multiplier (when score difference is minimal)")]
        [SerializeField] private float minVictoryRewardMultiplier = 1.05f;
        [Tooltip("Maximum victory reward multiplier (when score difference is maximal)")]
        [SerializeField] private float maxVictoryRewardMultiplier = 1.5f;
        [Tooltip("Minimum defeat reward multiplier (when score difference is maximal)")]
        [SerializeField] private float minDefeatRewardMultiplier = 0.5f;
        [Tooltip("Maximum defeat reward multiplier (when score difference is minimal)")]
        [SerializeField] private float maxDefeatRewardMultiplier = 0.95f;
        [Tooltip("Score difference ratio to maximum possible difference for modifier calculation")]
        [SerializeField] private float scoreDifferenceRatio = 0.5f;
        
        [Header("Random Element in Rewards")]
        [Tooltip("Minimum random reward multiplier (0.9 = -10%)")]
        [SerializeField] private float minRandomRewardMultiplier = 0.9f;
        [Tooltip("Maximum random reward multiplier (1.1 = +10%)")]
        [SerializeField] private float maxRandomRewardMultiplier = 1.1f;
        
        public float MatchDurationSeconds => matchDurationSeconds;
        public float BaseVictoryScoreThreshold => baseVictoryScoreThreshold;
        public float MatchEndDelay => matchEndDelay;
        
        public float EnemyBaseAccuracyScore => enemyBaseAccuracyScore;
        public float EnemyScoreGainRate => enemyScoreGainRate;

        public float EnemyMinInstructionDuration => enemyMinInstructionDuration;
        public float EnemyMaxInstructionDuration => enemyMaxInstructionDuration;
        public float EnemyInstructionMinRandomFactor => enemyInstructionMinRandomFactor;
        public float EnemyInstructionMaxRandomFactor => enemyInstructionMaxRandomFactor;
        public float EnemyLightModeMultiplier => enemyLightModeMultiplier;
        
        public int VictoryCoins => victoryCoins;
        public int VictoryGems => victoryGems;
        public int DefeatCoins => defeatCoins;
        public int TieCoins => tieCoins;
        public int VictoryExp => victoryExp;
        public int DefeatExp => defeatExp;
        public int TieExp => tieExp;
        
        public float GetDifficultyMultiplier(int playerLevel)
        {
            return 1f + (playerLevel - startLevel) * difficultyLevelMultiplier;
        }
        
        public float GetMatchProgressMultiplier(float matchProgress)
        {
            if (!enableMatchProgressDifficulty) return 1.0f;
            
            float normalizedProgress = Mathf.Clamp01(matchProgress);
            float curveValue = matchProgressCurve.Evaluate(normalizedProgress);
            return 1.0f + (matchProgressMaxBonus * curveValue);
        }
        
        public int CalculateExpForLevel(int level)
        {
            if (level <= startLevel) return 0;
            return Mathf.FloorToInt(expBaseMultiplier * Mathf.Pow(level - startLevel, expLevelPower));
        }
        
        public int CalculateLevelFromExp(int experience)
        {
            if (experience <= 0) return startLevel;
            
            float levelFloat = startLevel + Mathf.Pow(experience / expBaseMultiplier, 1f / expLevelPower);
            return Mathf.FloorToInt(levelFloat);
        }
        
        public float CalculateVictoryScoreThreshold(int playerLevel)
        {
            float baseThreshold = baseVictoryScoreThreshold;
            float levelMultiplier = 1f + (playerLevel - startLevel) * levelVictoryThresholdMultiplier;
            float randomFactor = 1f + Random.Range(-victoryThresholdRandomness, victoryThresholdRandomness);
            
            return baseThreshold * levelMultiplier * randomFactor;
        }
        
        public float CalculateRewardMultiplier(float playerScore, float enemyScore, bool isVictory)
        {
            float scoreDifference = Mathf.Abs(playerScore - enemyScore);
            float maxPossibleDifference = baseVictoryScoreThreshold * scoreDifferenceRatio;
            float normalizedDifference = Mathf.Clamp01(scoreDifference / maxPossibleDifference);
            
            float rewardMultiplier;
            if (isVictory)
            {
                rewardMultiplier = Mathf.Lerp(minVictoryRewardMultiplier, maxVictoryRewardMultiplier, normalizedDifference);
            }
            else
            {
                rewardMultiplier = Mathf.Lerp(maxDefeatRewardMultiplier, minDefeatRewardMultiplier, normalizedDifference);
            }
            
            float randomMultiplier = Random.Range(minRandomRewardMultiplier, maxRandomRewardMultiplier);
            
            return rewardMultiplier * randomMultiplier;
        }
    }
} 