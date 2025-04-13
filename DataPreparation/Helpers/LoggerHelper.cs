using DataPreparation.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataPreparation.Helpers;

internal static class LoggerHelper
{
    internal static ILoggerFactory CreateOrNullLogger(FixtureInfo fixtureInfo)
    {
        try
        {
            if (fixtureInfo.Instance is IDataPreparationLogger dataPreparationLoggerInitializer)
            {
                var builder = dataPreparationLoggerInitializer.InitializeDataPreparationTestLogger();
                if (builder is { } factory)
                {
                    return factory;
                }
            }
#if DEBUG
                Console.WriteLine($"LoggerFactory factory not found for {fixtureInfo.GetType()} use NullLoggerFactory");
#endif
        }
        catch (Exception e)
        {
#if DEBUG
            Console.Error.WriteLine(e);
            throw;
#endif
        }
        
        return NullLoggerFactory.Instance;
    }


    internal static void Log( Action<ILogger> logAction,params ILogger?[] loggers)
    {
        foreach (var logger in loggers)
        {
            if(logger == null) continue;
            logAction(logger);
        }
    }
}