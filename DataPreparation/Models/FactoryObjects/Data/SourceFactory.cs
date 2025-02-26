using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Exceptions;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DataPreparation.Factory.Testing;


public class SourceFactory(IServiceProvider serviceProvider, ILogger logger) : ISourceFactory
{
    private readonly ConcurrentDictionary<Type, HistoryStore<long,IFactoryData>> _localDataCache = new();
    private static readonly ThreadSafeCounter Counter = new(); 
    
    #region New
    #region New Synchronous Methods
    public object New<TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory
    {
        return CreateData<object,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return CreateData<T,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory
    {
        return CreateData<object,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds, argsEnumerable);
    }
    
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where T : notnull 
        where TDataFactory : IDataFactory<T>
    {
        return CreateData<T,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds, argsEnumerable);
    }

    #endregion
    #region New Asynchronous Methods
    public Task<object> NewAsync<TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        logger.LogDebug($"[{nameof(New)}]: New data for {typeof(TDataFactory)} was called");
        createdId = 0;
        var data = CreateDataAsync<object,TDataFactory>((factory, id, a ) =>  factory.Create(id,a,token), args);
        logger.LogInformation($"[{nameof(New)}]: Created data for {typeof(TDataFactory)} with id {createdId}");
        return data;
    }

    public Task<T> NewAsync<T, TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        logger.LogDebug($"[{nameof(New)}]: New data for {typeof(TDataFactory)} was called");
        createdId = 0;
        var data = CreateDataAsync<T,TDataFactory>(async (factory, id, a ) =>await factory.Create(id,a,token), args);
        logger.LogInformation($"[{nameof(New)}]: Created data for {typeof(TDataFactory)} with id {createdId}");
        return data;
    }

    public Task<object[]> NewAsync<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll( CreateDataAsync<object,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds, argsEnumerable));

    }

    public Task<T[]> NewAsync<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll( CreateData<Task<T>,TDataFactory>((factory, id, a) => factory.Create(id, a, token),size, out createdIds, argsEnumerable));
    }
    #endregion
    #endregion
    #region Get 
    #region Get  Synchronous Methods
    public object Get<TDataFactory>(out long createdId) where TDataFactory : IDataFactory
    {
        return GetData<object,TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId);
    }
    

    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return GetData<T,TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId);
    }

    public IList<object> Get<TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory
    {
        return GetData<object,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds);
    }
    
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return GetData<T,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds);
    }

    #endregion
    #region Get Asynchronous Methods
    public Task<object> GetAsync<TDataFactory>(out long createdId, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        return GetData<Task<object>,TDataFactory>((factory, id, a) => factory.Create(id, a,token), out createdId);
    }

    public Task<T> GetAsync<T, TDataFactory>(out long createdId, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return GetData<Task<T>,TDataFactory>((factory, id, a) => factory.Create(id, a,token), out createdId);
    }

    public Task<object[]> GetAsync<TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll( GetData<Task<object>,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds));
    }

    public Task<T[]> GetAsync<T, TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll( GetData<Task<T>,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds));
    }
    #endregion
    #endregion
    #region Was
    #region Was Synchronous Methods
    public IList<object> Was<TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where TDataFactory : IDataFactory
    {
        if (_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            logger.LogInformation($"[{nameof(Was)}]: Retrieved data for {typeof(TDataFactory)}");
            return data.GetAll(out createdIds).Select(o => o.Data).ToList();
        }
        logger.LogInformation($"[{nameof(Was)}]: No data found for {typeof(TDataFactory)}");
        createdIds = [];
        return [];
    }

    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where T : notnull where TDataFactory : IDataFactory<T>
    {
        return Was<TDataFactory>(out createdIds, args).Cast<T>().ToList();
    }
    #endregion
    #region Was Asynchronous Methods
    
    public Task<object[]> WasAsync<TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where TDataFactory : IDataFactoryAsync
    {
        return WasData<object,TDataFactory>(out createdIds);
    }

    private Task<TRet[]> WasData<TRet,TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryAsync
    {
        if (_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            logger.LogInformation($"[{nameof(Was)}]: Retrieved data for {typeof(TDataFactory)}");
            return Task.WhenAll( data.GetAll(out createdIds).Select(o => o.Data).Cast<Task<TRet>>());
        }
        logger.LogInformation($"[{nameof(Was)}]: No data found for {typeof(TDataFactory)}");
        createdIds = [];
        return Task.FromResult<TRet[]>([]);
    }

    public Task<T[]> WasAsync<T, TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return WasData<T,TDataFactory>(out createdIds);
    }
    
    #endregion
    #endregion
    #region Other Methods
    #region GetById
    
    public object? GetById<TDataFactory>(long createdId) where TDataFactory : IDataFactory
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            var factoryData = data.GetById(createdId);
            if (factoryData != null)
            {
                logger.LogInformation($"[{nameof(GetById)}]: Data for id {createdId} was found");
                return factoryData.Data;
            }
        }
        logger.LogInformation($"[{nameof(GetById)}]: No data found for {typeof(TDataFactory)} with id {createdId}");
        return default;
    }
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
       var foundData =  GetById<TDataFactory>(createdId);
       if (foundData != null)
       {
          return foundData is T data ? data 
               : throw CastExeption(logger,$"Data is not of type {typeof(T)} for operation {nameof(GetById)}.");
       }

       return default;
    }
    #endregion
    #endregion
    #region Dispose
    

    public void Dispose()
    {
        logger.LogInformation("Disposing SourceFactory");
        ExceptionAggregator exceptionAggregator = new ExceptionAggregator();
        foreach (var (factoryType, historyStore) in _localDataCache)
        {
            if (historyStore.IsEmpty()) continue;
            var factory = serviceProvider.GetService(factoryType);
            if (factory == null)
            {
                var ex  =exceptionAggregator.Add(new InvalidOperationException($"No factory service found for {factoryType}. Data will not be deleted. Data: {historyStore}"));
                logger.LogWarning(ex,$"Error on Dispose data {factoryType} with created data: {historyStore}");
                continue;
            }
            switch (factory)
            {
                case IDataFactory factorySync:
                {
                    foreach (var data in historyStore.GetAll(out _))
                    {
                        try
                        {
                            if (factorySync.Delete(data.Id, data.Data, data.Args))
                            {
                                logger.LogInformation($"Deleted data for {factoryType} with id {data.Id}");
                            }
                            else
                            {
                                var ex = new InvalidOperationException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}");
                                logger.LogError(ex, $"Failed to delete data for {factoryType} with id {data.Id}");
                                exceptionAggregator.Add(ex);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e,$"Error on Dispose data {factoryType} with created data: {historyStore}");
                            var ex = new InvalidOperationException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}",e);
                            exceptionAggregator.Add(ex);
                        }
                    }

                    break;
                }
                case IDataFactoryAsync factoryAsync:
                    foreach (var data in historyStore.GetAll(out _))
                    {
                        try
                        {
                            if (factoryAsync.Delete(data.Id, data.Data, data.Args).GetAwaiter().GetResult())
                            {
                                logger.LogInformation($"Deleted data for {factoryType} with id {data.Id}");
                            }
                            else
                            {
                                var ex = new InvalidOperationException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}");
                                logger.LogError(ex, $"Failed to delete data for {factoryType} with id {data.Id}");
                                exceptionAggregator.Add(ex);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e,$"Error on Dispose data {factoryType} with created data: {historyStore}");
                            var ex = new InvalidOperationException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}",e);
                            exceptionAggregator.Add(ex);
                        }
                    }
                    break;
                default:
                    var exception = new InvalidOperationException($"No correct factory type found for {factoryType}. Cannot delete data. Create a factory that implements {nameof(IDataFactory)} or {nameof(IDataFactoryAsync)}.");
                    logger.LogWarning(exception,$"Error on Dispose data {factoryType} with created data: {historyStore}");
                    exceptionAggregator.Add(exception);
                    break;
            }
            
        }
        _localDataCache.Clear();
        if(exceptionAggregator.HasExceptions)  throw exceptionAggregator.Get()!;
        logger.LogInformation("Disposed SourceFactory");
    }

    #endregion
    
    #region Helper Methods
    
    private IList<TRet> CreateData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: Creation of {size} data for {typeof(TDataFactory)} with was called");
        
        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<TRet>();

        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var data =   CreateData(createFunc,out var createdId, args);
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogDebug($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
    
    private TRet CreateData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,out long createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(CreateData)}]: Creation of data for {typeof(TDataFactory)}  with type {typeof(TRet)} was called");
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = Counter.Increment();
        //Create the data
        TRet data;
        try
        {
            data = createFunc(factory, createdId, args);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex,$"Creation of new data was canceled for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw;
        }
        catch (Exception e)
        {
           Dispose();
           var ex = new InvalidOperationException($"Failed to create data. Create method throw exception for {typeof(TDataFactory)}" +
                                                 $" with type {typeof(TRet)}, id {createdId} and arguments {args}",e);
           logger.LogError(ex,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
           throw ex;
        }
        
        if(data == null)
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to create data. Create method returned null for {typeof(TDataFactory)}" +
                                                  $" with type {typeof(TRet)}, id {createdId} and arguments {args}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw e;
        }
        
        //Get the local data history
        HistoryStore<long, IFactoryData> historyStore;
        try
        {
            historyStore = _localDataCache.GetOrAdd(typeof(TDataFactory),_ => new());
        }
        catch (Exception e)
        {
            Dispose();
            throw new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}",e);
        }
        //Update the local data history
        if(!historyStore.TryAdd(createdId,new FactoryData(createdId,data, args)))
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw e;
        }
        logger.LogDebug($"[{nameof(CreateData)}]: Created data for {typeof(TDataFactory)} with id {createdId} and type {typeof(TRet)}");

        return data;
    }
    private IList<Task<TRet>> CreateData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: Creation of {size} data for {typeof(TDataFactory)} with was called");
        
        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<TRet>();

        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var data =   CreateData(createFunc,out var createdId, args);
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogDebug($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
     private async Task<TRet> CreateDataAsync<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, Task<TRet>> createFunc, IDataParams? args = null) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(CreateData)}]: Creation of data for {typeof(TDataFactory)}  with type {typeof(TRet)} was called");
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        var createdId = Counter.Increment();
        //Create the data
        TRet data;
        try
        {
            data = await createFunc(factory, createdId, args);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex,$"Creation of new data was canceled for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw;
        }
        catch (Exception e)
        {
           Dispose();
           var ex = new InvalidOperationException($"Failed to create data. Create method throw exception for {typeof(TDataFactory)}" +
                                                 $" with type {typeof(TRet)}, id {createdId} and arguments {args}",e);
           logger.LogError(ex,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
           throw ex;
        }
        
        if(data == null)
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to create data. Create method returned null for {typeof(TDataFactory)}" +
                                                  $" with type {typeof(TRet)}, id {createdId} and arguments {args}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw e;
        }
        
        //Get the local data history
        HistoryStore<long, IFactoryData> historyStore;
        try
        {
            historyStore = _localDataCache.GetOrAdd(typeof(TDataFactory),_ => new());
        }
        catch (Exception e)
        {
            Dispose();
            throw new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}",e);
        }
        //Update the local data history
        if(!historyStore.TryAdd(createdId,new FactoryData(createdId,data, args)))
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw e;
        }
        logger.LogDebug($"[{nameof(CreateData)}]: Created data for {typeof(TDataFactory)} with id {createdId} and type {typeof(TRet)}");

        return data;
    }
    
    
    private TRet GetData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,out long createdId) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (history.GetLatest(out var item,out createdId))
            {
                logger.LogInformation($"[{nameof(Get)}]: Retrieved data for {typeof(TDataFactory)} with id {createdId}");
                if ((item as FactoryData)!.Data is not TRet data)
                {
                    Dispose();
                    throw CastExeption(logger,
                        $"Data is not of type {typeof(TRet)} for operation {nameof(Get)}.");
                }

                return data;
            }
        }

        logger.LogInformation($"[{nameof(Get)}]: No data found for {typeof(TDataFactory)}");
        return CreateData(createFunc,out createdId);
    }
    
    private IList<TRet> GetData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        IList<TRet> retData = new List<TRet>();
        createdIds = new List<long>();
        
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var historyStore))
        {
            var data = historyStore.GetLatest(size,out var ids).ToList();
            createdIds = ids.ToList();
            logger.LogDebug($"[{nameof(Get)}]: Retrieved from history {data.Count} data for {typeof(TDataFactory)}");
            if (data.Count == size)
            {
                var enumData = data.Select(o => o.Data);
                try
                {
                    return enumData.Cast<TRet>().ToList();
                }
                catch (InvalidCastException e)
                {
                    Dispose();
                    throw CastExeption(logger,$"Data is not of type {typeof(TRet)} for operation {nameof(Get)}.",e);
                }
            }
        }
        while (retData.Count < size)
        {
            var newItem = CreateData(createFunc,out var createdId);
            retData.Insert(0, newItem); // Insert new data at the beginning
            createdIds.Insert(0, createdId); // Insert the new createdId at the beginning
        }
        logger.LogInformation($"[{nameof(Get)}]: Retrieved {size} data for {typeof(TDataFactory)}");
        return retData;
    }
    private  Exception CastExeption(ILogger log,string text, Exception? exception = null)
    {
        var ex = new InvalidCastException(text,exception);
        log.LogError(ex,"Error in cast:");
        return ex;
    }
    #endregion
}

