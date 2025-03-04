using DataPreparation.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataPreparation.Helpers;

internal static class LoggerHelper
{
    internal static ILoggerFactory CreateOrNullLogger(Type fixtureType)
    {
        try
        {
            if (fixtureType.IsAssignableTo(typeof(IDataPreparationLoggerInitializer)))
            {
                var builder = fixtureType
                    .GetMethod(nameof(IDataPreparationLoggerInitializer.InitializeDataPreparationTestLogger))
                    ?.Invoke(null, null);

                if (builder is ILoggerFactory factory)
                {
                    return factory;
                }
            }
#if DEBUG
                Console.WriteLine($"LoggerFactory factory not found for {fixtureType} use NullLoggerFactory");
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