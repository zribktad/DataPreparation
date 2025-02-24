using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public static class TryCatchHelper
{
    public static T Execute<T>(Func<T> function, string customMessage, ILogger logger)
    {
        try
        {
            return function();
        }
        catch (Exception ex)
        {
            string logMessage = $"{customMessage}: {ex.Message}";
            logger.LogWarning(ex,logMessage);
            throw; // Re-throw the exception after logging
        }
    }

    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> function, string customMessage, ILogger logger)
    {
        try
        {
            return await function();
        }
        catch (Exception ex)
        {
            string logMessage = $"{customMessage}: {ex.Message}";
            logger.LogWarning(ex,logMessage);
            throw; // Re-throw the exception after logging
        }
    }
}