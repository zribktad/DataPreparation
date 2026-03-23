using DataPreparation.Data.Setup;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DataPreparation.Factory.Testing;

/// <summary>
/// Implementation of ISourceFactory that creates, manages and tracks test data objects for tests.
/// The SourceFactory is responsible for creating new data instances, reusing existing ones,
/// and cleaning up all resources after test execution.
/// </summary>
/// <remarks>
/// SourceFactory maintains a history of all created objects to allow for:
/// - Retrieving previously created objects
/// - Automatic cleanup of all created objects
/// - Tracking relationships between created objects
/// 
/// It supports both synchronous and asynchronous factory patterns, and provides methods to:
/// - Create new instances (New/NewAsync)
/// - Get existing or create new instances (Get/GetAsync)
/// - Query previously created instances (Was)
/// - Retrieve instances by ID (GetById)
/// </remarks>
public class SourceFactory(IServiceProvider serviceProvider, ILogger logger) : ISourceFactory
{
    private readonly FactoryHistoryManager _history = new(logger);

    // -- New (sync) --

    /// <inheritdoc />
    public object New<TDataFactory>(out long createdId, IDataParams? args = null)
        where TDataFactory : IDataFactory
    {
        return NewSingle<object, TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId, args);
    }

    /// <inheritdoc />
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null)
        where TDataFactory : IDataFactory<T> where T : notnull
    {
        return NewSingle<T, TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId, args);
    }

    /// <inheritdoc />
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds,
        IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactory
    {
        return NewBatch<object, TDataFactory>((factory, id, a) => factory.Create(id, a), size, out createdIds, argsEnumerable);
    }

    /// <inheritdoc />
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds,
        IEnumerable<IDataParams?>? argsEnumerable = null)
        where T : notnull where TDataFactory : IDataFactory<T>
    {
        return NewBatch<T, TDataFactory>((factory, id, a) => factory.Create(id, a), size, out createdIds, argsEnumerable);
    }

    // -- New (async) --

    /// <inheritdoc />
    public Task<object> NewAsync<TDataFactory>(out long createdId, IDataParams? args = null,
        CancellationToken token = default)
        where TDataFactory : IDataFactoryAsync
    {
        return NewSingleAsync<object, TDataFactory>((factory, id, a) => factory.Create(id, a, token), out createdId, args);
    }

    /// <inheritdoc />
    public Task<T> NewAsync<T, TDataFactory>(out long createdId, IDataParams? args = null,
        CancellationToken token = default)
        where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return NewSingleAsync<T, TDataFactory>((factory, id, a) => factory.Create(id, a, token), out createdId, args);
    }

    /// <inheritdoc />
    public Task<object[]> NewAsync<TDataFactory>(int size, out IList<long> createdIds,
        IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default)
        where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll(NewBatchAsync<object, TDataFactory>(
            (factory, id, a) => factory.Create(id, a, token), size, out createdIds, argsEnumerable));
    }

    /// <inheritdoc />
    public Task<T[]> NewAsync<T, TDataFactory>(int size, out IList<long> createdIds,
        IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default)
        where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll(NewBatchAsync<T, TDataFactory>(
            (factory, id, a) => factory.Create(id, a, token), size, out createdIds, argsEnumerable));
    }

    // -- Get (sync) --

    /// <inheritdoc />
    public object Get<TDataFactory>(out long createdId) where TDataFactory : IDataFactory
    {
        return GetOrCreate<object, TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId);
    }

    /// <inheritdoc />
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return GetOrCreate<T, TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId);
    }

    /// <inheritdoc />
    public IList<object> Get<TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory
    {
        return GetOrCreateBatch<object, TDataFactory>((factory, id, a) => factory.Create(id, a), size, out createdIds);
    }

    /// <inheritdoc />
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds)
        where TDataFactory : IDataFactory<T> where T : notnull
    {
        return GetOrCreateBatch<T, TDataFactory>((factory, id, a) => factory.Create(id, a), size, out createdIds);
    }

    // -- Get (async) --

    /// <inheritdoc />
    public Task<object> GetAsync<TDataFactory>(out long createdId, CancellationToken token = default)
        where TDataFactory : IDataFactoryAsync
    {
        return GetOrCreateAsync<object, TDataFactory>((factory, id, a) => factory.Create(id, a, token), out createdId);
    }

    /// <inheritdoc />
    public Task<T> GetAsync<T, TDataFactory>(out long createdId, CancellationToken token = default)
        where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return GetOrCreateAsync<T, TDataFactory>((factory, id, a) => factory.Create(id, a, token), out createdId);
    }

    /// <inheritdoc />
    public Task<object[]> GetAsync<TDataFactory>(int size, out IList<long> createdIds,
        CancellationToken token = default)
        where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll(GetOrCreateBatchAsync<object, TDataFactory>(
            (factory, id, a) => factory.Create(id, a, token), size, out createdIds));
    }

    /// <inheritdoc />
    public Task<T[]> GetAsync<T, TDataFactory>(int size, out IList<long> createdIds,
        CancellationToken token = default)
        where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll(GetOrCreateBatchAsync<T, TDataFactory>(
            (factory, id, a) => factory.Create(id, a, token), size, out createdIds));
    }

    // -- Was --

    /// <inheritdoc />
    public IList<object> Was<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase
    {
        return _history.GetAll<TDataFactory>(out createdIds);
    }

    /// <inheritdoc />
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds)
        where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return _history.GetAll<TDataFactory>(out createdIds).Cast<T>().ToList();
    }

    // -- Register --

    /// <inheritdoc />
    public bool Register<T, TDataFactory>(T data, out long? createdId, IDataParams? args = null)
        where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return Register<TDataFactory>(data, out createdId, args);
    }

    /// <inheritdoc />
    public bool Register<TDataFactory>(object data, out long? createdId, IDataParams? args = null)
        where TDataFactory : IDataFactoryBase
    {
        var factoryBase = serviceProvider.GetService<TDataFactory>() ??
                          throw new InvalidOperationException($"No factory or register found for {typeof(TDataFactory)}");
        var id = _history.GetNextId();
        _history.AddToHistory<TDataFactory>(id, args, data, factoryBase);
        createdId = id;
        return true;
    }

    // -- GetById --

    /// <inheritdoc />
    public object? GetById<TDataFactory>(long createdId) where TDataFactory : IDataFactory
    {
        if (_history.TryGetById(typeof(TDataFactory), createdId, out var data)) return data;
        logger.LogInformation("[{Operation}]: No data found for id {Id}", nameof(GetById), createdId);
        return default;
    }

    /// <inheritdoc />
    public object? GetById(long createdId)
    {
        if (_history.TryGetByIdAny(createdId, out var data)) return data;
        logger.LogInformation("[{Operation}]: No data found for id {Id}", nameof(GetById), createdId);
        return default;
    }

    /// <inheritdoc />
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
        var foundData = GetById<TDataFactory>(createdId);
        if (foundData == null) return default;
        return foundData is T data
            ? data
            : throw new InvalidCastException($"Data is not of type {typeof(T)} for operation {nameof(GetById)}.");
    }

    // -- Dispose --

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _history.DisposeAllAsync().ConfigureAwait(false);
    }

    // -- Private: factory resolution (DRY) --

    private TDataFactory ResolveFactory<TDataFactory>() where TDataFactory : IDataFactoryBase
    {
        return serviceProvider.GetService<TDataFactory>()
               ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
    }

    private void ValidateBatchArgs<TDataFactory>(int size, List<IDataParams?> argsList)
        where TDataFactory : IDataFactoryBase
    {
        if (size <= 0)
        {
            logger.LogWarning("[{Operation}]: No data will be created for {FactoryType}", "New", typeof(TDataFactory));
            return;
        }

        if (size < argsList.Count)
            logger.LogWarning("[{Operation}]: Size {Size} is smaller than args count {ArgCount}. Only {Size} data will be created",
                "New", size, argsList.Count, size);
        else if (size > argsList.Count && argsList.Count > 0)
            logger.LogInformation("[{Operation}]: Size {Size} is bigger than args count {ArgCount}. Some items will be without arguments",
                "New", size, argsList.Count);
    }

    // -- Private: sync creation --

    private TRet NewSingle<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc,
        out long createdId, IDataParams? args = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        var factory = ResolveFactory<TDataFactory>();
        createdId = _history.GetNextId();
        return CreateAndTrack<TRet, TDataFactory>(createFunc, createdId, args, factory);
    }

    private IList<TRet> NewBatch<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc,
        int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        var argsList = argsEnumerable?.ToList() ?? [];
        createdIds = new List<long>();
        var items = new List<TRet>();

        ValidateBatchArgs<TDataFactory>(size, argsList);
        if (size <= 0) return items;

        var factory = ResolveFactory<TDataFactory>();
        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var createdId = _history.GetNextId();
            items.Add(CreateAndTrack<TRet, TDataFactory>(createFunc, createdId, args, factory));
            createdIds.Add(createdId);
        }

        logger.LogDebug("[{Operation}]: Created {Count} data for {FactoryType}", "New", size, typeof(TDataFactory));
        return items;
    }

    private TRet CreateAndTrack<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc,
        long createdId, IDataParams? args, TDataFactory factory)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        TRet data;
        try
        {
            data = createFunc(factory, createdId, args);
        }
        catch (Exception e)
        {
            var ex = new InvalidOperationException(
                $"Failed to create data. Create method threw exception for {typeof(TDataFactory)}" +
                $" with type {typeof(TRet)}, id {createdId} and arguments {args}", e);
            logger.LogError(ex, "Creation of new data failed for {FactoryType} with type {DataType}",
                typeof(TDataFactory), typeof(TRet));
            throw ex;
        }

        _history.AddToHistory<TDataFactory>(createdId, args, data, factory);
        logger.LogDebug("[{Operation}]: Created data for {FactoryType} with id {Id} and type {DataType}",
            "New", typeof(TDataFactory), createdId, typeof(TRet));
        return data;
    }

    // -- Private: async creation --

    private Task<TRet> NewSingleAsync<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc,
        out long createdId, IDataParams? args = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        var factory = ResolveFactory<TDataFactory>();
        createdId = _history.GetNextId();
        return CreateAndTrackAsync<TRet, TDataFactory>(createFunc, args, factory, createdId);
    }

    private IList<Task<TRet>> NewBatchAsync<TRet, TDataFactory>(
        Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc,
        int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        var argsList = argsEnumerable?.ToList() ?? [];
        createdIds = new List<long>();
        var items = new List<Task<TRet>>();

        ValidateBatchArgs<TDataFactory>(size, argsList);
        if (size <= 0) return items;

        var factory = ResolveFactory<TDataFactory>();
        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var createdId = _history.GetNextId();
            items.Add(CreateAndTrackAsync<TRet, TDataFactory>(createFunc, args, factory, createdId));
            createdIds.Add(createdId);
        }

        logger.LogDebug("[{Operation}]: Created {Count} data for {FactoryType}", "New", size, typeof(TDataFactory));
        return items;
    }

    private async Task<TRet> CreateAndTrackAsync<TRet, TDataFactory>(
        Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc,
        IDataParams? args, TDataFactory factory, long createdId)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        TRet data;
        try
        {
            data = await createFunc(factory, createdId, args).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Creation of new data was canceled for {FactoryType} with type {DataType}",
                typeof(TDataFactory), typeof(TRet));
            throw;
        }
        catch (Exception e)
        {
            var ex = new InvalidOperationException(
                $"Failed to create data. Create method threw exception for {typeof(TDataFactory)}" +
                $" with type {typeof(TRet)}, id {createdId} and arguments {args}", e);
            logger.LogError(ex, "Creation of new data failed for {FactoryType} with type {DataType}",
                typeof(TDataFactory), typeof(TRet));
            throw ex;
        }

        _history.AddToHistory<TDataFactory>(createdId, args, data, factory);
        logger.LogDebug("[{Operation}]: Created data for {FactoryType} with id {Id} and type {DataType}",
            "NewAsync", typeof(TDataFactory), createdId, typeof(TRet));
        return data;
    }

    // -- Private: get-or-create --

    private TRet GetOrCreate<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc,
        out long createdId)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (_history.TryGetLatest<TRet, TDataFactory>(out long? id, out var ret))
        {
            createdId = id!.Value;
            return ret!;
        }

        logger.LogDebug("[{Operation}]: No created data found for {FactoryType}", "Get", typeof(TDataFactory));
        return NewSingle<TRet, TDataFactory>(createFunc, out createdId);
    }

    private IList<TRet> GetOrCreateBatch<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc,
        int size, out IList<long> createdIds)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (size <= 0)
        {
            logger.LogWarning("[{Operation}]: No data will be retrieved for {FactoryType}", "Get", typeof(TDataFactory));
            createdIds = new List<long>();
            return new List<TRet>();
        }

        if (_history.TryGetLatest<TRet, TDataFactory>(size, out createdIds, out var retData)) return retData;

        while (retData.Count < size)
        {
            var newItem = NewSingle<TRet, TDataFactory>(createFunc, out var createdId);
            retData.Insert(0, newItem);
            createdIds.Insert(0, createdId);
        }

        logger.LogInformation("[{Operation}]: Retrieved {Count} data for {FactoryType}", "Get", size, typeof(TDataFactory));
        return retData;
    }

    private Task<TRet> GetOrCreateAsync<TRet, TDataFactory>(
        Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc, out long createdId)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (_history.TryGetLatest<TRet, TDataFactory>(out long? id, out var ret))
        {
            createdId = id!.Value;
            return Task.FromResult(ret)!;
        }

        logger.LogDebug("[{Operation}]: No created data found for {FactoryType}", "Get", typeof(TDataFactory));
        return NewSingleAsync<TRet, TDataFactory>(createFunc, out createdId);
    }

    private IList<Task<TRet>> GetOrCreateBatchAsync<TRet, TDataFactory>(
        Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc,
        int size, out IList<long> createdIds)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (size <= 0)
        {
            logger.LogWarning("[{Operation}]: No data will be retrieved for {FactoryType}", "Get", typeof(TDataFactory));
            createdIds = new List<long>();
            return new List<Task<TRet>>();
        }

        var ret = _history.TryGetLatest<TRet, TDataFactory>(size, out createdIds, out var latestData);
        IList<Task<TRet>> retData = latestData.Select(Task.FromResult).ToList();
        if (ret) return retData;

        while (retData.Count < size)
        {
            var newItem = NewSingleAsync<TRet, TDataFactory>(createFunc, out var createdId);
            retData.Insert(0, newItem);
            createdIds.Insert(0, createdId);
        }

        logger.LogInformation("[{Operation}]: Retrieved {Count} data for {FactoryType}", "Get", size, typeof(TDataFactory));
        return retData;
    }
}
