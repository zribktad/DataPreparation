using DataPreparation.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataPreparation.Helpers;

/// <summary>
/// Helper for creating logger factories and logging to multiple loggers simultaneously.
/// </summary>
internal static class LoggerHelper
{
    /// <summary>
    /// Creates a logger factory from the fixture's IDataPreparationLogger implementation,
    /// or falls back to NullLoggerFactory if none is configured.
    /// </summary>
    /// <param name="fixtureInfo">The fixture information to check for logger configuration</param>
    /// <returns>A configured ILoggerFactory or NullLoggerFactory</returns>
    internal static ILoggerFactory CreateOrNullLogger(FixtureInfo fixtureInfo)
    {
        if (fixtureInfo.Instance is IDataPreparationLogger loggerInitializer)
        {
            var factory = loggerInitializer.InitializeDataPreparationTestLogger();
            if (factory is not null) return factory;
        }

        return NullLoggerFactory.Instance;
    }

    /// <summary>
    /// Executes a logging action on all non-null loggers.
    /// </summary>
    /// <param name="logAction">The logging action to execute</param>
    /// <param name="loggers">The loggers to write to</param>
    internal static void Log(Action<ILogger> logAction, params ILogger?[] loggers)
    {
        foreach (var logger in loggers)
        {
            if (logger == null) continue;
            logAction(logger);
        }
    }
}