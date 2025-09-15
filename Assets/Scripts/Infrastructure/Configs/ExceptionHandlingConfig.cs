using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(menuName = "Configs/ExceptionHandlingConfig", fileName = "ExceptionHandlingConfig")]
    public class ExceptionHandlingConfig : ScriptableObject
    {
        [Header("Global Settings")]
        [Tooltip("Включить глобальную обработку исключений")]
        public bool enableGlobalExceptionHandling = true;
        
        [Tooltip("Логгировать все исключения (даже обработанные)")]
        public bool logAllExceptions = true;
        
        [Header("Retry Settings")]
        [Tooltip("Максимальное количество попыток для критических операций")]
        [Range(1, 10)]
        public int maxRetries = 3;
        
        [Tooltip("Задержка между попытками (секунды)")]
        [Range(0.1f, 10f)]
        public float delayBetweenRetries = 1f;
        
        [Tooltip("Экспоненциальное увеличение задержки")]
        public bool useExponentialBackoff = true;
        
        [Header("Save Operations")]
        [Tooltip("Максимальное количество попыток сохранения")]
        [Range(1, 5)]
        public int maxSaveRetries = 2;
        
        [Tooltip("Задержка между попытками сохранения (секунды)")]
        [Range(0.5f, 5f)]
        public float saveRetryDelay = 1.5f;
        
        [Tooltip("Автоматический fallback на локальные сохранения")]
        public bool autoFallbackToLocalSave = true;
        
        [Header("Asset Loading")]
        [Tooltip("Максимальное количество попыток загрузки ассетов")]
        [Range(1, 5)]
        public int maxAssetLoadRetries = 3;
        
        [Tooltip("Задержка между попытками загрузки ассетов (секунды)")]
        [Range(0.5f, 5f)]
        public float assetLoadRetryDelay = 1f;
        
        [Tooltip("Использовать fallback ассеты при неудачной загрузке")]
        public bool useFallbackAssets = true;
        
        [Header("Network Operations")]
        [Tooltip("Таймаут для сетевых операций (секунды)")]
        [Range(5f, 60f)]
        public float networkOperationTimeout = 15f;
        
        [Tooltip("Максимальное количество попыток сетевых операций")]
        [Range(1, 5)]
        public int maxNetworkRetries = 3;
        
        [Header("Currency Operations")]
        [Tooltip("Включить автоматический rollback валютных операций")]
        public bool enableCurrencyRollback = true;
        
        [Tooltip("Логгировать все валютные операции")]
        public bool logAllCurrencyOperations = false;
        
        [Header("Performance")]
        [Tooltip("Максимальное количество исключений в секунду для логгирования")]
        [Range(1, 100)]
        public int maxExceptionsPerSecond = 10;
        
        [Tooltip("Включить сбор статистики исключений")]
        public bool enableExceptionStatistics = true;
        
        [Header("Development")]
        [Tooltip("Показывать UI уведомления об ошибках (только в debug режиме)")]
        public bool showDebugErrorNotifications = false;
        
        [Tooltip("Сохранять детальные логи исключений в файлы")]
        public bool saveDetailedErrorLogs = true;
        
        [Tooltip("Включить подробное логгирование в editor")]
        public bool verboseLoggingInEditor = true;
        
        #region Runtime Properties
        
        public float GetRetryDelay(int attemptNumber)
        {
            if (!useExponentialBackoff)
                return delayBetweenRetries;
                
            return delayBetweenRetries * Mathf.Pow(2, attemptNumber - 1);
        }
        
        public bool ShouldLogException()
        {
            return logAllExceptions;
        }
        
        public bool IsDebugMode()
        {
            return Application.isEditor || Debug.isDebugBuild;
        }
        
        #endregion
    }
} 