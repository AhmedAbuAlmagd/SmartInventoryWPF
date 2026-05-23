using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace POSDesktopSystem.Infrastructure.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName);
    }

    public void Dispose() { }
}

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private static readonly object _lockInfo = new object();

    public FileLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null) return;

        Task.Run(() =>
        {
            try
            {
                lock (_lockInfo)
                {
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var logDir = Path.Combine(appData, "POSDesktopSystem", "logs");
                    if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

                    var logFile = Path.Combine(logDir, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");

                    using var writer = new StreamWriter(logFile, true);
                    writer.Write($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}");

                    if (exception != null)
                    {
                        writer.Write($" {exception.Message} {exception.StackTrace}");
                    }
                    writer.WriteLine();
                }
            }
            catch { }
        });
    }
}
