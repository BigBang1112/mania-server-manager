using Microsoft.Extensions.Logging;

namespace ManiaServerManager;

internal sealed class Logger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        Console.WriteLine($"[{logLevel}] {message}");

        if (exception is not null)
        {
            Console.WriteLine(exception);
        }
    }
}
