using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services.Common.Logging;
using UnityEngine;

namespace Game.Infrastructure.Additionals.Trash
{
    public static class ExceptionHandler
    {
        #region Safe Async Operations
        
        public static async UniTask<T> HandleAsync<T>(
            Func<UniTask<T>> operation,
            T fallbackValue = default,
            string operationName = "",
            bool logError = true,
            Func<T> fallbackProvider = null)
        {
            try
            {
                return await operation();
            }
            catch (OperationCanceledException)
            {
                if (logError)
                    Logs.Info($"Operation cancelled: {operationName}");
                
                return fallbackProvider != null ? fallbackProvider.Invoke() : fallbackValue;
            }
            catch (Exception ex)
            {
                if (logError)
                    Logs.Error($"Failed operation: {operationName}", ex);
                
                return fallbackProvider != null ? fallbackProvider.Invoke() : fallbackValue;
            }
        }

        public static async UniTask<bool> HandleAsync(
            Func<UniTask> operation,
            string operationName = "",
            bool logError = true,
            Func<UniTask> fallbackOperation = null)
        {
            try
            {
                await operation();
                return true;
            }
            catch (OperationCanceledException)
            {
                if (logError)
                    Logs.Info($"Operation cancelled: {operationName}");
                if (fallbackOperation != null)
                    await HandleAsync(fallbackOperation, $"{operationName}_fallback", logError);
                return false;
            }
            catch (Exception ex)
            {
                if (logError)
                    Logs.Error($"Failed operation: {operationName}", ex);
                if (fallbackOperation != null)
                    await HandleAsync(fallbackOperation, $"{operationName}_fallback", logError);
                return false;
            }
        }
        
        #endregion

        #region Safe Sync Operations
        
        public static T Handle<T>(
            Func<T> operation,
            T fallbackValue = default,
            string operationName = "",
            bool logError = true,
            Func<T> fallbackProvider = null)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                if (logError)
                    Logs.Error($"Failed operation: {operationName}", ex);
                return fallbackProvider != null ? fallbackProvider.Invoke() : fallbackValue;
            }
        }

        public static bool Handle(
            Action operation,
            string operationName = "",
            bool logError = true,
            Action fallbackOperation = null)
        {
            try
            {
                operation();
                return true;
            }
            catch (Exception ex)
            {
                if (logError)
                    Logs.Error($"Failed operation: {operationName}", ex);
                fallbackOperation?.Invoke();
                return false;
            }
        }
        
        #endregion
        
        #region Retry Mechanisms
        
        public static async UniTask<T> RetryAsync<T>(
            Func<UniTask<T>> operation,
            int maxRetries = 3,
            float delayBetweenRetries = 1f,
            T fallbackValue = default,
            string operationName = "",
            CancellationToken cancellationToken = default)
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                        
                    return await operation();
                }
                catch (OperationCanceledException)
                {
                    Logs.Info($"Operation cancelled on attempt {attempt + 1}: {operationName}");
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Logs.Warning($"Attempt {attempt + 1}/{maxRetries} failed for {operationName}: {ex.Message}");
                    
                    if (attempt < maxRetries - 1)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenRetries), 
                            cancellationToken: cancellationToken);
                    }
                }
            }
            
            if (lastException != null)
                Logs.Error($"All {maxRetries} attempts failed for {operationName}", lastException);
                
            return fallbackValue;
        }
        
        #endregion
        
        #region Save Operations
        
        public static async UniTask<bool> SafeSave(
            Func<UniTask> cloudSaveOperation,
            Action localSaveFallback,
            string operationName = "Save")
        {
            var cloudSuccess = await HandleAsync(
                cloudSaveOperation,
                operationName: $"{operationName}_Cloud",
                logError: false
            );
            
            if (cloudSuccess)
            {
                Logs.Info($"Cloud save successful: {operationName}");
                return true;
            }
            
            var localSuccess = Handle(
                localSaveFallback,
                operationName: $"{operationName}_Local",
                logError: true
            );
            
            if (localSuccess)
            {
                Logs.Warning($"Used local save fallback for: {operationName}");
                return true;
            }
            
            Logs.Error($"Both cloud and local save failed for: {operationName}");
            return false;
        }
        
        #endregion
        
        #region Rollback Operations
        
        public static bool SafeRollbackOperation(
            Action operation,
            Action rollbackOperation,
            string operationName = "Operation")
        {
            try
            {
                operation();
                Logs.Debug($"Operation successful: {operationName}");
                return true;
            }
            catch (Exception ex)
            {
                Logs.Error($"Operation failed, rolling back: {operationName}", ex);
                
                Handle(
                    rollbackOperation,
                    operationName: $"{operationName}_Rollback",
                    logError: true
                );
                
                return false;
            }
        }
        
        #endregion
        
        #region Global Exception Handler
        
        public static void InitializeGlobalExceptionHandling()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Logs.Info("Global exception handling initialized");
        }
        
        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                Logs.Error($"Unity Error: {condition}", new Exception($"{condition}\n{stackTrace}"));
            }
        }
        
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logs.Error("Unhandled exception occurred", ex);
            }
        }
        
        #endregion
    }
} 