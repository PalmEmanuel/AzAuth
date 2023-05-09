using Microsoft.Extensions.Logging;
using System.Management.Automation;

namespace PipeHow.AzAuth;

public abstract partial class PSLoggerCmdletBase : PSCmdlet, ILogger
{
    private readonly List<LogLevel> logLevels = new() {
        LogLevel.Trace,
        LogLevel.Debug,
        LogLevel.Information,
        LogLevel.Warning,
        LogLevel.Error
    };

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    public IDisposable? BeginScope<TState>(TState state) => default;
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

    public bool IsEnabled(LogLevel logLevel) => logLevels.Contains(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Information:
                WriteVerbose(formatter(state, exception));
                break;
            case LogLevel.Debug:
                WriteDebug(formatter(state, exception));
                break;
            case LogLevel.Warning:
                WriteWarning(formatter(state, exception));
                break;
            case LogLevel.Error:
                WriteError(new ErrorRecord(exception ?? new Exception(formatter(state, exception)), MyInvocation.InvocationName, ErrorCategory.InvalidOperation, null));
                break;
            default:
                break;
        }
    }
}
