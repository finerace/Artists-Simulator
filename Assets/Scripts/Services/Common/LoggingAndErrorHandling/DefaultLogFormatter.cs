using System.Text;
using Game.Infrastructure.Configs;

namespace Game.Services.Common.Logging
{
    public class DefaultLogFormatter : ILogFormatter
    {
        public string Format(LogEntry entry)
        {
            var config = ConfigsProxy.LoggingConfig;
            var sb = new StringBuilder();
            
            if (config?.ShowTimestamp != false)
            {
                sb.Append("[");
                sb.Append(entry.Timestamp.ToString("HH:mm:ss.fff"));
                sb.Append("] ");
            }
            
            sb.Append("[");
            sb.Append(GetLevelString(entry.Level));
            sb.Append("]");
            
            if ((config?.ShowFileName != false && !string.IsNullOrEmpty(entry.SourceFileName)) ||
                (config?.ShowMethodName != false && !string.IsNullOrEmpty(entry.MemberName)) ||
                (config?.ShowLineNumber != false && entry.SourceLineNumber > 0))
            {
                sb.Append(" [");
                
                if (config?.ShowFileName != false && !string.IsNullOrEmpty(entry.SourceFileName))
                {
                    sb.Append(entry.SourceFileName);
                }
                
                if (config?.ShowMethodName != false && !string.IsNullOrEmpty(entry.MemberName))
                {
                    if (config?.ShowFileName != false && !string.IsNullOrEmpty(entry.SourceFileName))
                        sb.Append(".");
                    sb.Append(entry.MemberName);
                }
                
                if (config?.ShowLineNumber != false && entry.SourceLineNumber > 0)
                {
                    sb.Append(":");
                    sb.Append(entry.SourceLineNumber);
                }
                
                sb.Append("]");
            }
            
            sb.Append(" ");
            sb.Append(EnsureEndPunctuation(entry.Message));
            
            if (entry.Exception != null)
            {
                sb.AppendLine();
                sb.Append("Exception: ");
                sb.Append(entry.Exception.ToString());
            }
            
            return sb.ToString();
        }
        
        private string GetLevelString(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                _ => "UNKNOWN"
            };
        }
        
        private string EnsureEndPunctuation(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;
            
            char lastChar = message[message.Length - 1];
            
            if (lastChar == '.' || lastChar == '!' || lastChar == '?' || 
                lastChar == ':' || lastChar == ';' || lastChar == ',')
            {
                return message;
            }
            
            return message + ".";
        }
    }
} 