using System.Reflection;
using DataPreparation.Models.Data;
using DataPreparation.Testing;
using Microsoft.Extensions.Logging;

namespace DataPreparation.DataHandlers;

/// <summary>
/// Core helper class that resolves and obtains data preparation instances from the dependency injection container.
/// This class serves as a bridge between test attributes and data preparation classes.
/// </summary>
/// <remarks>
/// GetDataPreparation has two primary functions:
/// 1. Resolve data preparation instances directly from types specified in attributes like UsePreparedData
/// 2. Resolve data preparation instances indirectly by looking up class/method associations in DataRelationStore
/// 
/// This enables both explicit data preparation via attributes and implicit data preparation via conventions.
/// </remarks>
internal static class GetDataPreparation
{
    /// <summary>
    /// Gets prepared data instances for a test case by resolving each specified data provider type.
    /// </summary>
    /// <param name="testStore">The test store containing the DI service provider</param>
    /// <param name="dataProviders">Array of types representing the data provider classes</param>
    /// <returns>A list of instantiated data preparation objects ready for execution</returns>
    internal static List<object> GetPreparedData(TestStore testStore, Type[] dataProviders)
    {
        return dataProviders.Select(type => ResolveFromServiceProvider(testStore, type)).ToList();
    }

    /// <summary>
    /// Gets prepared data instances by looking up data preparation classes associated with a specified
    /// class and/or methods in the DataRelationStore.
    /// </summary>
    internal static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation,
        Type classType, string[] methodsNames)
    {
        var logger = testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation));
        List<Type?> preparedDataTypes = [];

        if (useClassDataPreparation)
        {
            var classDataType = DataRelationStore.GetClassDataPreparationType(classType)
                ?? throw ThrowNotRegistered(logger, $"Prepared data for class {classType.FullName} not registered.");
            preparedDataTypes.Add(classDataType);
        }
        else
        {
            logger.LogTrace("Class data preparation not used for class {ClassType}", classType.FullName);
        }

        foreach (var methodName in methodsNames)
        {
            var methodInfo = classType.GetMethod(methodName)
                ?? throw ThrowNotRegistered(logger, $"Method {methodName} not found in class {classType.FullName}.");

            var methodDataType = DataRelationStore.GetMethodDataPreparationType(methodInfo)
                ?? throw ThrowNotRegistered(logger, $"Prepared data for method {methodInfo} not registered.");
            preparedDataTypes.Add(methodDataType);
        }

        return GetPreparedData(testStore, preparedDataTypes.ToArray());
    }

    /// <summary>
    /// Overload of GetPreparedDataFromCode that handles a single method name or null.
    /// </summary>
    public static List<object> GetPreparedDataFromCode(TestStore testStore, bool useClassDataPreparation,
        Type classType, string? methodsNames)
    {
        return GetPreparedDataFromCode(testStore, useClassDataPreparation, classType,
            methodsNames == null ? [] : [methodsNames]);
    }

    /// <summary>
    /// Filters and organizes parameter arrays for data preparation execution.
    /// </summary>
    public static (object[]?[], object[]?[]) FilterParams(
        bool useClassDataPreparation,
        string? methodName,
        object[]? classClassParamsUpData,
        object[]? methodParamsUpData,
        object[]? classParamsDownData,
        object[]? methodParamsDownData)
    {
        List<object[]?> paramsUpData = [];
        List<object[]?> paramsDown = [];

        if (useClassDataPreparation)
        {
            paramsUpData.Add(classClassParamsUpData);
            paramsDown.Add(classParamsDownData);
        }

        if (methodName != null)
        {
            paramsUpData.Add(methodParamsUpData);
            paramsDown.Add(methodParamsDownData);
        }

        return (paramsUpData.ToArray(), paramsDown.ToArray());
    }

    // -- Private helpers (DRY) --

    /// <summary>
    /// Resolves a single data preparation instance from the DI service provider.
    /// </summary>
    private static object ResolveFromServiceProvider(TestStore testStore, Type preparedDataType)
    {
        var logger = testStore.LoggerFactory.CreateLogger(typeof(GetDataPreparation));
        logger.LogTrace("Getting prepared data with type {TypeName}", preparedDataType.FullName);

        try
        {
            var preparedData = testStore.ServiceProvider.GetService(preparedDataType);
            if (preparedData == null)
            {
                throw ThrowNotRegistered(logger,
                    $"Prepared data with type {preparedDataType.FullName} not found.");
            }

            logger.LogDebug("Prepared data with type {TypeName} found", preparedDataType.FullName);
            return preparedData;
        }
        catch (InvalidOperationException e) when (e.Message.Contains("suitable constructor"))
        {
            logger.LogError(e, "Prepared data with type {TypeName} not found", preparedDataType.FullName);
            throw new InvalidOperationException(
                $"For prepared data with type {preparedDataType.FullName} not found suitable constructor, check Dependency Injection.", e);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Prepared data with type {TypeName} not found", preparedDataType.FullName);
            throw new InvalidOperationException(
                $"Prepared data with type {preparedDataType.FullName} not found.", e);
        }
    }

    /// <summary>
    /// DRY helper: logs an error and throws InvalidOperationException with the given message.
    /// </summary>
    private static InvalidOperationException ThrowNotRegistered(ILogger logger, string message)
    {
        var exception = new InvalidOperationException(message);
        logger.LogError(exception, "Data preparation failed: {Message}", message);
        return exception;
    }
}