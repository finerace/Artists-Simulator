namespace Game.Services.Common.Logging
{
    public interface ILogFormatter
    {
        string Format(LogEntry entry);
    }
} 