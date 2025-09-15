using System;
using System.IO;
using Game.Additional.MagicAttributes;

namespace Game.Services.Common.Logging
{
    public readonly struct LogEntry
    {
        public readonly DateTime Timestamp;
        public readonly LogLevel Level;
        public readonly string Message;
        public readonly Exception Exception;
        public readonly string MemberName;
        public readonly string SourceFilePath;
        public readonly string SourceFileName;
        public readonly int SourceLineNumber;
        
        
        public LogEntry(LogLevel level, string message, Exception exception = null,
            string sourceFilePath = "", string memberName = "", int sourceLineNumber = 0)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
            Exception = exception;
            MemberName = memberName;
            SourceFilePath = sourceFilePath;
            SourceFileName = GetSafeFileName(sourceFilePath);
            SourceLineNumber = sourceLineNumber;
        }
        
        private static string GetSafeFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";
            
            try
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
            catch
            {
                return filePath;
            }
        }
    }
} 