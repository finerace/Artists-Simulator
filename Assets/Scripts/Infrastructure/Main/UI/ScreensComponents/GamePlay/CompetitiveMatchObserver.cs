using Game.Infrastructure.Configs;
using Game.Services.Core;
using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class CompetitiveMatchObserver : MonoBehaviour
    {
        [Header("Таймер матча")]
        [SerializeField] private TMP_Text matchTimerText;
        
        [Header("Прогресс очков")]
        [SerializeField] private FillBarController playerScoreBar;
        [SerializeField] private FillBarController enemyScoreBar;
        
        [Header("Настройки анимации")]
        [SerializeField] private bool useAnimationForScoreUpdates = true;
        
        private PaintGameplayGenerationService gameplayGenerationService;
        private PseudoEnemyService pseudoEnemyService;
        
        private float victoryThreshold = 1f;
        private float timerWarningThreshold = 10f;
        
        [Inject]
        private void Construct(
            PaintGameplayGenerationService gameplayGenerationService,
            PseudoEnemyService pseudoEnemyService)
        {
            this.gameplayGenerationService = gameplayGenerationService;
            this.pseudoEnemyService = pseudoEnemyService;
        }
        
        private void Initialize()
        {
            var config = ConfigsProxy.CompetitiveGameConfig;
            timerWarningThreshold = config.MatchEndDelay;
            
            // Обновляем порог победы из сервиса игры
            UpdateVictoryThreshold();
            
            // Подписываемся на события
            gameplayGenerationService.OnTotalScoreChanged += UpdatePlayerScore;
            gameplayGenerationService.OnMatchTimerUpdated += UpdateMatchTimer;
            pseudoEnemyService.OnEnemyScoreChanged += UpdateEnemyScore;
            
            // Инициализируем UI с текущими значениями
            UpdatePlayerScore(gameplayGenerationService.CurrentMatchScore);
            UpdateEnemyScore(pseudoEnemyService.EnemyScore);
            UpdateMatchTimer(config.MatchDurationSeconds);
        }
        
        private void UpdateVictoryThreshold()
        {
            // Используем тот же порог, что и в игровой логике
            victoryThreshold = gameplayGenerationService.VictoryScoreThreshold;
            if (victoryThreshold <= 0)
            {
                // Если порог еще не установлен, используем базовый
                victoryThreshold = ConfigsProxy.CompetitiveGameConfig.BaseVictoryScoreThreshold;
            }
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void OnDestroy()
        {
            if (gameplayGenerationService != null)
            {
                gameplayGenerationService.OnTotalScoreChanged -= UpdatePlayerScore;
                gameplayGenerationService.OnMatchTimerUpdated -= UpdateMatchTimer;
            }
            
            if (pseudoEnemyService != null)
            {
                pseudoEnemyService.OnEnemyScoreChanged -= UpdateEnemyScore;
            }
        }
        
        private void UpdatePlayerScore(float score)
        {
            // Обновляем порог перед каждым обновлением шкалы
            UpdateVictoryThreshold();
            
            if (playerScoreBar != null)
            {
                if (victoryThreshold <= 0)
                    return;
                    
                float normalizedScore = score / victoryThreshold;
                normalizedScore = Mathf.Clamp01(normalizedScore);
                
                if (useAnimationForScoreUpdates)
                {
                    playerScoreBar.SetFillAmount(normalizedScore);
                }
                else
                {
                    playerScoreBar.SetFillAmountImmediate(normalizedScore);
                }
            }
        }
        
        private void UpdateEnemyScore(float score)
        {
            // Обновляем порог перед каждым обновлением шкалы
            UpdateVictoryThreshold();
            
            if (enemyScoreBar != null)
            {
                if (victoryThreshold <= 0)
                    return;
                    
                float normalizedScore = score / victoryThreshold;
                normalizedScore = Mathf.Clamp01(normalizedScore);
                
                if (useAnimationForScoreUpdates)
                {
                    enemyScoreBar.SetFillAmount(normalizedScore);
                }
                else
                {
                    enemyScoreBar.SetFillAmountImmediate(normalizedScore);
                }
            }
        }

        private void UpdateMatchTimer(float timeRemaining)
        {
            if (matchTimerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeRemaining);
                matchTimerText.text = string.Format("{0:00}:{1:00}", 
                    timeSpan.Minutes, 
                    timeSpan.Seconds);
                
                if (timeRemaining <= timerWarningThreshold)
                {
                    matchTimerText.color = Color.red;
                }
                else
                {
                    matchTimerText.color = Color.white;
                }
            }
        }
    }
} 