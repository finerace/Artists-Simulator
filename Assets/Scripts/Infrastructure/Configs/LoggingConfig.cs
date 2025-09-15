using Game.Services.Common.Logging;
using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "LoggingConfig", menuName = "Configs/LoggingConfig", order = 10)]
    public class LoggingConfig : ScriptableObject
    {
        [Header("Настройки логгирования")]
        [SerializeField] private LogLevel minimumLogLevel = LogLevel.Info;
        [SerializeField] private bool enableConsoleLogging = true;
        [SerializeField] private bool enableUnityLogging = true;
        [SerializeField] private bool enableTimestamps = true;
        
        [Header("Формат логов")]
        [SerializeField] private bool showTimestamp = true;
        [SerializeField] private bool showMethodName = true;
        [SerializeField] private bool showFileName = true;
        [SerializeField] private bool showLineNumber = true;
        
        [Header("Файловые логи (Unity Editor)")]
        [SerializeField] private bool enableEditorFileLogs = true;
        [SerializeField] private int maxSessionLogFiles = 10;
        [SerializeField] private string logFileDirectory = "Logs/Sessions";
        
        [Header("WebGL настройки")]
        [SerializeField] private bool useUnityLogForWebGL = true;
        [SerializeField] private bool logToPlayerPrefs = false;
        [SerializeField] private int maxLogEntriesInPlayerPrefs = 100;
        
        public LogLevel MinimumLogLevel => minimumLogLevel;
        public bool EnableConsoleLogging => enableConsoleLogging;
        public bool EnableUnityLogging => enableUnityLogging;
        public bool EnableTimestamps => enableTimestamps;
        
        public bool ShowTimestamp => showTimestamp;
        public bool ShowMethodName => showMethodName;
        public bool ShowFileName => showFileName;
        public bool ShowLineNumber => showLineNumber;
        
        public bool EnableEditorFileLogs => enableEditorFileLogs;
        public int MaxSessionLogFiles => maxSessionLogFiles;
        public string LogFileDirectory => logFileDirectory;
        
        public bool UseUnityLogForWebGL => useUnityLogForWebGL;
        public bool LogToPlayerPrefs => logToPlayerPrefs;
        public int MaxLogEntriesInPlayerPrefs => maxLogEntriesInPlayerPrefs;
    }
} 