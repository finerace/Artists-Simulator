using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Services.Meta;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class PlayerLevelObserver : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private FillBarController levelProgressBar;
        
        [SerializeField] private float fillDuration = 0.5f;
        [SerializeField] private float levelTextScaleDuration = 0.3f;
        [SerializeField] private float initializationDelay = 0.5f;
        [SerializeField] private bool useInitializationDelay = true;
        
        private IPlayerLevelService playerLevelService;
        private int displayedLevel;
        private float displayedProgress;
        private bool isAnimating;
        private CancellationTokenSource animationCts;
        private bool isInitialized;
        private bool wasEverInitialized;
        
        private Queue<LevelChangeOperation> pendingOperations = new Queue<LevelChangeOperation>();
        private bool isProcessingOperations;
        
        private struct LevelChangeOperation
        {
            public int TargetLevel;
            public float TargetProgress;
            public bool IsLevelUp;
            public bool SkipAnimation;
            
            public LevelChangeOperation(int targetLevel, float targetProgress, bool isLevelUp, bool skipAnimation = false)
            {
                TargetLevel = targetLevel;
                TargetProgress = targetProgress;
                IsLevelUp = isLevelUp;
                SkipAnimation = skipAnimation;
            }
        }
        
        [Inject]
        private void Construct(IPlayerLevelService playerLevelService)
        {
            this.playerLevelService = playerLevelService;
            
            if (gameObject.activeInHierarchy)
            {
                InitializeAsync().Forget();
            }
        }
        
        private void OnEnable()
        {
            if (playerLevelService != null && !isInitialized)
            {
                InitializeAsync().Forget();
            }
        }
        
        private void OnDisable()
        {
            UnsubscribeEvents();
            CancelAnimations();
            isInitialized = false;
        }
        
        private void OnDestroy()
        {
            CancelAnimations();
        }
        
        private async UniTask InitializeAsync()
        {
            UnsubscribeEvents();
            
            // Очищаем очередь операций перед инициализацией
            pendingOperations.Clear();
            isProcessingOperations = false;
            
            int targetLevel = playerLevelService.CurrentLevel;
            float targetProgress = playerLevelService.LevelProgress;
            
            // При первой инициализации пропускаем анимации
            if (!wasEverInitialized)
            {
                displayedLevel = 0; // Начинаем с 0 уровня
                displayedProgress = 0f;
                
                UpdateLevelText(targetLevel, false);
                levelProgressBar.SetFillAmountImmediate(targetProgress);
                
                displayedLevel = targetLevel;
                displayedProgress = targetProgress;
                
                wasEverInitialized = true;
            }
            else
            {
                if (useInitializationDelay)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(initializationDelay));
                }
                
                // Используем тот же механизм анимации, что и при обычном изменении опыта
                if (targetLevel != displayedLevel)
                {
                    EnqueueLevelChangeOperation(targetLevel, targetProgress);
                }
                else if (!Mathf.Approximately(targetProgress, displayedProgress))
                {
                    AnimateProgressBarAsync(targetProgress).Forget();
                }
            }
            
            // Подписываемся на события только после инициализации
            playerLevelService.OnExperienceChanged += HandleExperienceChanged;
            playerLevelService.OnLevelChanged += HandleLevelChanged;
            
            isInitialized = true;
        }
        
        private void UnsubscribeEvents()
        {
            if (playerLevelService != null)
            {
                playerLevelService.OnExperienceChanged -= HandleExperienceChanged;
                playerLevelService.OnLevelChanged -= HandleLevelChanged;
            }
        }
        
        private void CancelAnimations()
        {
            animationCts?.Cancel();
            animationCts?.Dispose();
            animationCts = null;
            isAnimating = false;
        }
        
        private void HandleExperienceChanged(int totalExperience)
        {
            float targetProgress = playerLevelService.LevelProgress;
            int currentLevel = playerLevelService.CurrentLevel;
            
            if (currentLevel == displayedLevel)
            {
                AnimateProgressBarAsync(targetProgress).Forget();
            }
            else
            {
                EnqueueLevelChangeOperation(currentLevel, targetProgress);
            }
        }
        
        private void HandleLevelChanged(int newLevel)
        {
            if (newLevel == displayedLevel)
                return;
                
            float targetProgress = playerLevelService.LevelProgress;
            EnqueueLevelChangeOperation(newLevel, targetProgress);
        }
        
        private void EnqueueLevelChangeOperation(int targetLevel, float targetProgress)
        {
            bool isLevelUp = targetLevel > displayedLevel;
            var operation = new LevelChangeOperation(targetLevel, targetProgress, isLevelUp);
            pendingOperations.Enqueue(operation);
            
            if (!isProcessingOperations)
            {
                ProcessPendingOperationsAsync().Forget();
            }
        }
        
        private async UniTask ProcessPendingOperationsAsync()
        {
            if (isProcessingOperations || pendingOperations.Count == 0)
                return;
                
            isProcessingOperations = true;
            
            try
            {
                while (pendingOperations.Count > 0)
                {
                    var operation = pendingOperations.Dequeue();
                    await ProcessLevelChangeAsync(operation);
                }
            }
            finally
            {
                isProcessingOperations = false;
                
                if (pendingOperations.Count > 0)
                {
                    ProcessPendingOperationsAsync().Forget();
                }
            }
        }
        
        private async UniTask ProcessLevelChangeAsync(LevelChangeOperation operation)
        {
            if (isAnimating)
            {
                CancelAnimations();
            }
            
            isAnimating = true;
            animationCts = new CancellationTokenSource();
            
            try
            {
                if (operation.IsLevelUp)
                {
                    for (int level = displayedLevel; level < operation.TargetLevel; level++)
                    {
                        await AnimateToFullProgressAsync(animationCts.Token);
                        
                        if (animationCts.IsCancellationRequested) 
                            break;
                        
                        displayedLevel = level + 1;
                        UpdateLevelText(displayedLevel, true);
                        
                        levelProgressBar.SetFillAmountImmediate(0f);
                        displayedProgress = 0f;
                        
                        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: animationCts.Token);
                        
                        if (animationCts.IsCancellationRequested) 
                            break;
                    }
                }
                else
                {
                    for (int level = displayedLevel; level > operation.TargetLevel; level--)
                    {
                        await AnimateToEmptyProgressAsync(animationCts.Token);
                        
                        if (animationCts.IsCancellationRequested) 
                            break;
                        
                        displayedLevel = level - 1;
                        UpdateLevelText(displayedLevel, true);
                        
                        levelProgressBar.SetFillAmountImmediate(1f);
                        displayedProgress = 1f;
                        
                        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: animationCts.Token);
                        
                        if (animationCts.IsCancellationRequested) 
                            break;
                    }
                }
                
                await AnimateProgressBarAsync(operation.TargetProgress);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            finally
            {
                isAnimating = false;
            }
        }
        
        private async UniTask AnimateProgressBarAsync(float targetProgress)
        {
            if (Mathf.Approximately(displayedProgress, targetProgress))
                return;
                
            CancelAnimations();
            isAnimating = true;
            animationCts = new CancellationTokenSource();
            
            try
            {
                displayedProgress = targetProgress;
                levelProgressBar.SetFillAmount(targetProgress, fillDuration);
                
                await UniTask.Delay(TimeSpan.FromSeconds(fillDuration), cancellationToken: animationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена
            }
            finally
            {
                isAnimating = false;
            }
        }
        
        private async UniTask AnimateToFullProgressAsync(CancellationToken cancellationToken)
        {
            displayedProgress = 1f;
            levelProgressBar.SetFillAmount(1f, fillDuration);
            
            await UniTask.Delay(TimeSpan.FromSeconds(fillDuration), cancellationToken: cancellationToken);
        }
        
        private async UniTask AnimateToEmptyProgressAsync(CancellationToken cancellationToken)
        {
            displayedProgress = 0f;
            levelProgressBar.SetFillAmount(0f, fillDuration);
            
            await UniTask.Delay(TimeSpan.FromSeconds(fillDuration), cancellationToken: cancellationToken);
        }
        
        private void UpdateLevelText(int level, bool animate)
        {
            if (levelText == null) 
                return;
                
            if (animate)
            {
                Vector3 originalScale = levelText.transform.localScale;
                
                levelText.transform
                    .DOScale(originalScale * 1.2f, levelTextScaleDuration / 2)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        levelText.text = level.ToString();
                        levelText.transform
                            .DOScale(originalScale, levelTextScaleDuration / 2)
                            .SetEase(Ease.InQuad);
                    });
            }
            else
            {
                levelText.text = level.ToString();
            }
        }
    }
} 
 