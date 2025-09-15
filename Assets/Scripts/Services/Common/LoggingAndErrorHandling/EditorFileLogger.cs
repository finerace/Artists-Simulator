#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Infrastructure.Configs;
using UnityEngine;

namespace Game.Services.Common.Logging
{
    public static class EditorFileLogger
    {
        private static List<string> sessionLogs = new List<string>();
        private static string currentSessionFile;
        private static bool isInitialized = false;
        private static LoggingConfig config;
        
        public static void Initialize()
        {
            if (isInitialized) 
                return;
            
            config = ConfigsProxy.LoggingConfig;
            if (config?.EnableEditorFileLogs != true) return;
            
            isInitialized = true;
            StartNewSession();
            
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
        }
        
        public static void LogToFile(string message)
        {
            if (!isInitialized) 
                return;
            
            if (config?.EnableEditorFileLogs != true) return;
            
            sessionLogs.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}");
        }
        
        private static void StartNewSession()
        {
            if (config == null) 
                return;
            
            string logDir = Path.Combine(Application.dataPath, "..", config.LogFileDirectory);
            Directory.CreateDirectory(logDir);
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            currentSessionFile = Path.Combine(logDir, $"Session_{timestamp}.txt");
            
            sessionLogs.Clear();
            sessionLogs.Add($"=== New session started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            
            CleanupOldLogFiles(logDir, config.MaxSessionLogFiles);
        }
        
        private static void SaveCurrentSession()
        {
            if (!isInitialized || sessionLogs.Count <= 1) return;
            
            try
            {
                sessionLogs.Add($"=== Session ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                File.WriteAllLines(currentSessionFile, sessionLogs);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Log session save error: {ex.Message}");
            }
        }
        
        private static void CleanupOldLogFiles(string logDir, int maxFiles)
        {
            try
            {
                var logFiles = Directory.GetFiles(logDir, "Session_*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToArray();

                if (logFiles.Length < maxFiles) 
                    return;
                
                var filesToDelete = logFiles.Skip(maxFiles - 1);
                foreach (var file in filesToDelete)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Old log files cleanup error: {ex.Message}");
            }
        }
        
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode ||
                state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                SaveCurrentSession();
            }
            
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode ||
                state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                StartNewSession();
            }
        }
        
        private static void OnDomainUnload(object sender, EventArgs e)
        {
            SaveCurrentSession();
        }
        
        public static string GetCurrentSessionPath()
        {
            return currentSessionFile;
        }
        
        public static int GetCurrentSessionLogCount()
        {
            return sessionLogs.Count;
        }
    }
}
#endif 