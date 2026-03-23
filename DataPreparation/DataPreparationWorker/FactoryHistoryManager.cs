using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Exceptions;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Factory.Testing;

/// <summary>
/// Manages the history of all data objects created by factories during test execution.
/// Provides methods for adding, retrieving, and cleaning up factory-created data.
/// </summary>
/// <remarks>
/// Extracted from SourceFactory to satisfy SRP — SourceFactory handles creation logic,
/// while FactoryHistoryManager handles tracking and lifecycle.
/// </remarks>
internal class FactoryHistoryManager(ILogger logger)
{
    /// <summary>Dictionary that tracks created objects grouped by factory type</summary>
    private readonly ConcurrentDictionary<Type, HistoryStore<IFactoryData>> _factoryHistory = new();

    /// <summary>Main history store that tracks all created objects across all factory types</summary>
    private readonly HistoryStore<IFactoryData> _allHistory = new();

    /// <summary>Thread-safe ID generator for creating unique IDs for each data object</summary>
    private static readonly ThreadSafeCounter IdGenerator = new();

    /// <summary>
    /// Generates the next unique ID for a factory-created data object.
    /// </summary>
    internal long GetNextId() => IdGenerator.GetNextId();

    /// <summary>
    /// Adds a factory-created data object to both the global and per-factory history stores.
    /// </summary>
    /// <typeparam name="TDataFactory">The factory type used to create the data</typeparam>
    /// <param name="createdId">The unique ID assigned to the data</param>
    /// <param name="args">Optional creation parameters</param>
    /// <param name="data">The created data object</param>
    /// <param name="factoryBase">The factory instance that created the data</param>
    /// <exception cref="InvalidOperationException">Thrown when data is null or history insertion fails</exception>
    internal void AddToHistory<TDataFactory>(long createdId, IDataParams? args, object data, IDataFactoryBase factoryBase)
        where TDataFactory : IDataFactoryBase
    {
        if (data == null)
        {
            var e = new InvalidOperationException(
                $"Failed to create data. Create method returned null for {typeof(TDataFactory)} id {createdId} and arguments {args}");
            logger.LogError(e, "Creation of new data failed for {FactoryType}", typeof(TDataFactory));
            throw e;
        }

        IFactoryData factoryData = new FactoryData(createdId, data, args, factoryBase);

        if (!_allHistory.TryPush(createdId, factoryData))
        {
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e, "Creation of new data failed for {FactoryType} with type {DataType}",
                typeof(TDataFactory), data.GetType());
            throw e;
        }

        try
        {
            var historyStore = _factoryHistory.GetOrAdd(typeof(TDataFactory), _ => new());
            if (!historyStore.TryPush(createdId, factoryData))
            {
                var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
                logger.LogError(e, "Creation of new data failed for {FactoryType} with type {DataType}",
                    typeof(TDataFactory), data.GetType());
                throw e;
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create history for {FactoryType}", typeof(TDataFactory));
            throw new InvalidOperationException($"Failed to create history for {typeof(TDataFactory)}", e);
        }
    }

    /// <summary>
    /// Tries to get the latest data object created by the specified factory type.
    /// </summary>
    internal bool TryGetLatest<TRet, TDataFactory>(out long? createdId, out TRet? ret)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (_factoryHistory.TryGetValue(typeof(TDataFactory), out var history) &&
            history.TryGetLatest(out var item, out createdId))
        {
            logger.LogDebug("[{Operation}]: Retrieved data for {FactoryType} with id {Id}",
                "Get", typeof(TDataFactory), createdId);

            if ((item as FactoryData)!.Data is not TRet data)
            {
                throw CastException($"Data is not of type {typeof(TRet)} for operation Get.");
            }

            ret = data;
            return true;
        }

        createdId = null;
        ret = default;
        return false;
    }

    /// <summary>
    /// Tries to get the latest N data objects created by the specified factory type.
    /// </summary>
    internal bool TryGetLatest<TRet, TDataFactory>(int size, out IList<long> createdIds, out IList<TRet> retData)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (_factoryHistory.TryGetValue(typeof(TDataFactory), out var historyStore))
        {
            historyStore.TryGetLatest(size, out var data, out var ids);
            logger.LogDebug("[{Operation}]: Retrieved from history {Count} data for {FactoryType}",
                "Get", data.Count, typeof(TDataFactory));

            var enumData = data.Select(o => o.Data);
            try
            {
                retData = enumData.Cast<TRet>().ToList();
                createdIds = ids.ToList();
                return data.Count == size;
            }
            catch (InvalidCastException e)
            {
                throw CastException($"Data is not of type {typeof(TRet)} for operation Get.", e);
            }
        }

        createdIds = new List<long>();
        retData = new List<TRet>();
        return false;
    }

    /// <summary>
    /// Retrieves all data objects created by the specified factory type.
    /// </summary>
    internal IList<object> GetAll<TDataFactory>(out IList<long> createdIds)
        where TDataFactory : IDataFactoryBase
    {
        if (_factoryHistory.TryGetValue(typeof(TDataFactory), out var data))
        {
            logger.LogInformation("[{Operation}]: Retrieved data for {FactoryType}", "Was", typeof(TDataFactory));
            return data.GetAll(out createdIds).Select(o => o.Data).ToList();
        }

        logger.LogInformation("[{Operation}]: No data found for {FactoryType}", "Was", typeof(TDataFactory));
        createdIds = [];
        return [];
    }

    /// <summary>
    /// Retrieves a data object by its ID from a specific factory's history.
    /// </summary>
    internal bool TryGetById(Type factoryType, long createdId, out object? data)
    {
        if (_factoryHistory.TryGetValue(factoryType, out var history))
        {
            var factoryData = history.GetById(createdId);
            if (factoryData != null)
            {
                logger.LogInformation("[{Operation}]: Data for id {Id} was found", "GetById", createdId);
                data = factoryData.Data;
                return true;
            }
        }

        data = default;
        return false;
    }

    /// <summary>
    /// Retrieves a data object by its ID, searching all factory histories.
    /// </summary>
    internal bool TryGetByIdAny(long createdId, out object? data)
    {
        foreach (var (_, history) in _factoryHistory)
        {
            var factoryData = history.GetById(createdId);
            if (factoryData != null)
            {
                logger.LogInformation("[{Operation}]: Data for id {Id} was found", "GetById", createdId);
                data = factoryData.Data;
                return true;
            }
        }

        data = default;
        return false;
    }

    /// <summary>
    /// Disposes all tracked data by invoking Delete on each factory in LIFO order.
    /// </summary>
    /// <exception cref="AggregateException">Contains all exceptions that occurred during cleanup</exception>
    internal async ValueTask DisposeAllAsync()
    {
        logger.LogDebug("Factory disposing");
        var exceptionAggregator = new ExceptionAggregator();

        while (_allHistory.TryPop(out var data))
        {
            if (data == null)
            {
                logger.LogWarning("Some history data are null. Cannot delete data.");
                continue;
            }

            var factoryType = data.FactoryBase.GetType();

            try
            {
                bool deleted = data.FactoryBase switch
                {
                    IDataRegister sync => sync.Delete(data.Id, data.Data, data.Args),
                    IDataRegisterAsync async => await async.Delete(data.Id, data.Data, data.Args).ConfigureAwait(false),
                    _ => throw new InvalidOperationException(
                        $"No correct factory type found for data: {data.FactoryBase}. Cannot delete data. " +
                        $"Create a factory that implements Data Factory Object interface.")
                };

                if (deleted)
                    logger.LogInformation("Deleted data for {FactoryType} with id {Id}", factoryType, data.Id);
                else
                    LogAndCollect(exceptionAggregator,
                        $"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}");
            }
            catch (InvalidOperationException e)
            {
                LogAndCollect(exceptionAggregator,
                    $"Error to delete data for {factoryType} with id {data.Id} and arguments {data.Args}", e);
            }
            catch (Exception e)
            {
                LogAndCollect(exceptionAggregator,
                    $"Error to delete data for {factoryType} with id {data.Id} and arguments {data.Args}", e);
            }
        }

        _factoryHistory.Clear();
        _allHistory.Clear();

        if (exceptionAggregator.HasExceptions) throw exceptionAggregator.Get()!;
        logger.LogInformation("Factory disposed - Data deleted");
    }

    private InvalidCastException CastException(string text, Exception? exception = null)
    {
        var ex = new InvalidCastException(text, exception);
        logger.LogError(ex, "Error in cast:");
        return ex;
    }

    private void LogAndCollect(ExceptionAggregator aggregator, string message, Exception? innerException = null)
    {
        var ex = new InvalidOperationException(message, innerException);
        logger.LogError(ex, "{Message}", message);
        aggregator.Add(ex);
    }
}
