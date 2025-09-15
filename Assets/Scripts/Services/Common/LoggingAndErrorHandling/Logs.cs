using System;
using System.Runtime.CompilerServices;
using Game.Infrastructure.Configs;
using UnityEngine;

namespace Game.Services.Common.Logging
{
    public static class Logs
    {
        private static ILogFormatter formatter = new DefaultLogFormatter();
        private static bool isInitialized = false;
        private static LoggingConfig config = ConfigsProxy.LoggingConfig;
        
        public static void Initialize()
        {
            if (isInitialized) 
            {
                Debug("Logs system already initialized");
                return;
            }

            isInitialized = true;
            
            if (config == null)
            {
                config = ConfigsProxy.LoggingConfig;

                if (config == null)
                {
                    config = new LoggingConfig();
                    
                    Warning("LoggingConfig not found!");
                    return;
                }
            }

#if UNITY_EDITOR
            EditorFileLogger.Initialize();
#endif

            Info("Logs system initialized");
        }
        
        public static void Debug(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.Debug, message, null, sourceFilePath, memberName, sourceLineNumber);
        }
        
        public static void Info(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.Info, message, null, sourceFilePath, memberName, sourceLineNumber);
        }
        
        public static void Warning(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.Warning, message, null, sourceFilePath, memberName, sourceLineNumber);
        }
        
        public static void DebugWarning(string message, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (config?.MinimumLogLevel <= LogLevel.Debug)
            {
                Log(LogLevel.Warning, message, null, sourceFilePath, memberName, sourceLineNumber);
            }
        }
        
        public static void Error(string message, Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.Error, message, exception, sourceFilePath, memberName, sourceLineNumber);
        }

        public static bool IsActiveWarning(bool isActive,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        
        {
            if(!isActive)
                Warning($"Warning! System active is {isActive}!", sourceFilePath, memberName, sourceLineNumber);
            
            return !isActive;
        }
        
        public static bool IsNotInitializedWarning(bool isInitialized,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if(!isInitialized)
                Warning($"Warning! System is not initialized!", sourceFilePath, memberName, sourceLineNumber);

            return !isInitialized;
        }
        
        public static void Log(LogLevel level, string message, Exception exception,
            string className, string methodName, int sourceLineNumber)
        {
            if (!ShouldLog(level)) return;
            
            var entry = new LogEntry(level, message, exception, className, methodName, sourceLineNumber);
            var formattedMessage = formatter.Format(entry);
            
            WriteToOutput(level, formattedMessage);
        }
        
        private static bool ShouldLog(LogLevel level)
        {
            if (!isInitialized) 
                return true;
            
            if (config == null) 
                return true;
            
            return level >= config.MinimumLogLevel;
        }
        
        private static void WriteToOutput(LogLevel level, string message)
        {
            if (config?.EnableUnityLogging != false)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        UnityEngine.Debug.Log(message);
                        break;
                    case LogLevel.Info:
                        UnityEngine.Debug.Log(message);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(message);
                        break;
                    case LogLevel.Error:
                        UnityEngine.Debug.LogError(message);
                        break;
                }
            }

#if UNITY_EDITOR
            EditorFileLogger.LogToFile(message);
#endif
            
            if (Application.platform == RuntimePlatform.WebGLPlayer && 
                config?.LogToPlayerPrefs == true)
            {
                LogToPlayerPrefs(message);
            }
        }
        
        private static void LogToPlayerPrefs(string message)
        {
            var existingLogs = PlayerPrefs.GetString("GameLogs", "");
            var newLogs = existingLogs + "\n" + message;
            
            var lines = newLogs.Split('\n');
            var maxEntries = config?.MaxLogEntriesInPlayerPrefs ?? 100;
            
            if (lines.Length > maxEntries)
            {
                var startIndex = lines.Length - maxEntries;
                newLogs = string.Join("\n", lines, startIndex, maxEntries);
            }
            
            PlayerPrefs.SetString("GameLogs", newLogs);
        }
    }
} 