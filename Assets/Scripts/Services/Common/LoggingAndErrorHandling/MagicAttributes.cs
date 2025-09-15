using System;
using System.Reflection;
using MethodBoundaryAspect.Fody.Attributes;
using Game.Services.Common.Logging;
using Game.Infrastructure.Configs;

namespace Game.Additional.MagicAttributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public sealed class LogMethodAttribute : OnMethodBoundaryAspect
    {
        private readonly LogLevel entryLogLevel;
        private readonly LogLevel exitLogLevel;
        
        private readonly int lineNumber;
        
        public LogMethodAttribute(LogLevel entryLogLevel = LogLevel.Debug, LogLevel exitLogLevel = LogLevel.None, int lineNumber = -1)
        {
            this.entryLogLevel = entryLogLevel;
            this.exitLogLevel = exitLogLevel;
            this.lineNumber = lineNumber;
        }
        
        public LogMethodAttribute(int lineNumber = -1, LogLevel entryLogLevel = LogLevel.Debug, LogLevel exitLogLevel = LogLevel.None)
        {
            this.entryLogLevel = entryLogLevel;
            this.exitLogLevel = exitLogLevel;
            this.lineNumber = lineNumber;
        }
        
        public LogMethodAttribute()
        {
            this.entryLogLevel = LogLevel.Debug;
            this.exitLogLevel = LogLevel.Debug;
            this.lineNumber = -1;
        }
        
        public override void OnEntry(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;
            var className = methodInfo?.DeclaringType?.Name ?? "Unknown";
            var methodName = methodInfo?.Name ?? "Unknown";
            
            var message = "[ENTRY]";
            
            if(entryLogLevel != LogLevel.None)
                Logs.Log(entryLogLevel, message, null, className, methodName, 0);
        }
        
        public override void OnExit(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;
            var className = methodInfo?.DeclaringType?.Name ?? "Unknown";
            var methodName = methodInfo?.Name ?? "Unknown";
            
            var message = $"[EXIT]";
            
            if(exitLogLevel != LogLevel.None)
                Logs.Log(exitLogLevel, message, null, className, methodName, 0);
        }
        
        public override void OnException(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;
            var className = methodInfo?.DeclaringType?.Name ?? "Unknown";
            var methodName = methodInfo?.Name ?? "Unknown";
            var operationName = $"{className}.{methodName}";
            
            var config = ConfigsProxy.ExceptionHandlingConfig;
            
            if (config?.logAllExceptions == true)
            {
                Logs.Log(LogLevel.Error, $"[ERROR] {operationName} failed", args.Exception, 
                    className, methodName, -1);
            }
            
            args.FlowBehavior = FlowBehavior.Return;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class)]
    public sealed class SafeExecutionAttribute : OnMethodBoundaryAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;
            var className = methodInfo?.DeclaringType?.Name ?? "Unknown";
            var methodName = methodInfo?.Name ?? "Unknown";
            var operationName = $"{className}.{methodName}";

            HandleExceptionLogging();
            void HandleExceptionLogging()
            {
                if (args.Exception is OperationCanceledException)
                {
                    Logs.Log(LogLevel.Debug, $"[CANCELLED] {operationName} was cancelled", null, 
                        className, methodName, -1);
                }
                else
                {
                    Logs.Log(LogLevel.Error, $"[ERROR] {operationName} failed", args.Exception, 
                        className, methodName, -1);
                }
            }
            
            args.FlowBehavior = FlowBehavior.Return;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class)]
    public sealed class SafeAnimationAttribute : OnMethodBoundaryAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            var methodInfo = args.Method as MethodInfo;
            var className = methodInfo?.DeclaringType?.Name ?? "Unknown";
            var methodName = methodInfo?.Name ?? "Unknown";
            var operationName = $"{className}.{methodName}";

            HandleAnimationException();
            void HandleAnimationException()
            {
                switch (args.Exception)
                {
                    case OperationCanceledException:
                        Logs.Log(LogLevel.Debug, $"🎭 [ANIMATION_CANCELLED] {operationName} animation was cancelled gracefully", null, 
                            className, methodName, -1);
                        break;
                    
                    case UnityEngine.MissingReferenceException:
                        Logs.Log(LogLevel.Debug, $"🎬 [ANIMATION_ORPHANED] {operationName} tried to animate destroyed Unity object - no worries, happens when UI closes during animation", null, 
                            className, methodName, -1);
                        break;
                    
                    case ArgumentNullException when args.Exception.Message.Contains("Transform") || args.Exception.Message.Contains("RectTransform"):
                        Logs.Log(LogLevel.Debug, $"🎪 [ANIMATION_NULL_TARGET] {operationName} received null animation target - UI probably closed mid-animation", null, 
                            className, methodName, -1);
                        break;
                    
                    case NullReferenceException when args.Exception.StackTrace?.Contains("gameObject") == true:
                        Logs.Log(LogLevel.Debug, $"🎨 [ANIMATION_DESTROYED_OBJECT] {operationName} animation target was destroyed - this is normal when UI closes quickly", null, 
                            className, methodName, -1);
                        break;
                    
                    default:
                        Logs.Log(LogLevel.Error, $"🚫 [ANIMATION_UNEXPECTED_ERROR] {operationName} animation failed with unexpected error", args.Exception, 
                            className, methodName, -1);
                        break;
                }
            }
            
            args.FlowBehavior = FlowBehavior.Return;
        }
    }
} 