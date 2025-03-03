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
    private readonly ConcurrentDictionary<Type, HistoryStore<IFactoryData>> _localDataCache = new();
    private readonly ConcurrentStack<IFactoryData> _createdHistory = new();
    private static readonly ThreadSafeCounter Counter = new(); 
    
    #region New
    #region New Synchronous Methods
    public object New<TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory
    {
        return NewData<object,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return NewData<T,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory
    {
        return NewData<object,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds, argsEnumerable);
    }
    
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where T : notnull 
        where TDataFactory : IDataFactory<T>
    {
        return NewData<T,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds, argsEnumerable);
    }

    #endregion
    #region New Asynchronous Methods
    public Task<object> NewAsync<TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        logger.LogDebug($"[{nameof(New)}]: New data for {typeof(TDataFactory)} was called");
        var data = NewDataAsync<object,TDataFactory>((factory, id, a ) =>  factory.Create(id,a,token),out createdId, args);
        logger.LogInformation($"[{nameof(New)}]: Created data for {typeof(TDataFactory)} with id {createdId}");
        return data;
    }

    public Task<T> NewAsync<T, TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        logger.LogDebug($"[{nameof(New)}]: New data for {typeof(TDataFactory)} was called");
        var data = NewDataAsync<T,TDataFactory>( (factory, id, a ) => factory.Create(id,a,token),out createdId, args);
        logger.LogInformation($"[{nameof(New)}]: Created data for {typeof(TDataFactory)} with id {createdId}");
        return data;
    }

    public Task<object[]> NewAsync<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll( NewDataAsync<object,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds, argsEnumerable));

    }

    public Task<T[]> NewAsync<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll( NewDataAsync<T,TDataFactory>((factory, id, a) => factory.Create(id, a, token),size, out createdIds, argsEnumerable));
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
        return GetDataAsync<object,TDataFactory>((factory, id, a) => factory.Create(id, a,token), out createdId);
    }

    public Task<T> GetAsync<T, TDataFactory>(out long createdId, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return GetDataAsync<T,TDataFactory>((factory, id, a) => factory.Create(id, a,token), out createdId);
    }

    public Task<object[]> GetAsync<TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync
    {
        return Task.WhenAll( GetDataAsync<object,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds));
    }

    public Task<T[]> GetAsync<T, TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where T : notnull where TDataFactory : IDataFactoryAsync<T>
    {
        return Task.WhenAll( GetDataAsync<T,TDataFactory>((factory, id, a) => factory.Create(id, a,token),size, out createdIds));
    }
    #endregion
    #endregion
    #region Was
    
    public IList<object> Was<TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where TDataFactory : IDataFactoryBase
    {
        return WasData<TDataFactory>(out createdIds);
    }
    
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds, IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return WasData<TDataFactory>(out createdIds).Cast<T>().ToList();
    }

    public bool Register<T, TDataFactory>(T data, out long? createdId,IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        var id = Counter.Increment();
        createdId = null;
        var factoryBase = serviceProvider.GetService<TDataFactory>() ??
                          throw new InvalidOperationException($"No factory or register found for {typeof(TDataFactory)}");
        if (AddDataToHistory<TDataFactory>(id, args, data, factoryBase))
        {
            createdId = id;
            return true;
        }
        return false;
    }

    private IList<object> WasData<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase
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
        int createdHistoryCount = _createdHistory.Count;
        for (int i = 0; i < createdHistoryCount; i++)
        {
            _createdHistory.TryPop(out var data);
            switch (data?.FactoryBase)
            {
                case IDataRegister factorySync:
                {
                        try
                        {
                            if (factorySync.Delete(data.Id, data.Data, data.Args))
                            {
                                logger.LogInformation($"Deleted data for {factorySync.GetType()} with id {data.Id}");
                            }
                            else
                            {
                                var ex = new InvalidOperationException($"Failed to delete data for {factorySync.GetType()} with id {data.Id} and arguments {data.Args}");
                                logger.LogError(ex, $"Failed to delete data for {factorySync.GetType()} with id {data.Id}");
                                exceptionAggregator.Add(ex);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e,$"Error on Dispose data {factorySync.GetType()} with created data: {factorySync.GetType()}");
                            var ex = new InvalidOperationException($"Failed to delete data for {factorySync.GetType()} with id {data.Id} and arguments {data.Args}",e);
                            exceptionAggregator.Add(ex);
                        }
                        break;
                }
                case IDataRegisterAsync factoryAsync:
                        try
                        {
                            if (factoryAsync.Delete(data.Id, data.Data, data.Args).GetAwaiter().GetResult())
                            {
                                logger.LogInformation($"Deleted data for {factoryAsync.GetType()} with id {data.Id}");
                            }
                            else
                            {
                                var ex = new InvalidOperationException($"Failed to delete data for {factoryAsync.GetType()} with id {data.Id} and arguments {data.Args}");
                                logger.LogError(ex, $"Failed to delete data for {factoryAsync.GetType()} with id {data.Id}");
                                exceptionAggregator.Add(ex);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e,$"Error on Dispose data {factoryAsync.GetType()} with created data: {factoryAsync.GetType()}");
                            var ex = new InvalidOperationException($"Failed to delete data for {factoryAsync.GetType()} with id {data.Id} and arguments {data.Args}",e);
                            exceptionAggregator.Add(ex);
                        }
                        break;
               default:
                    var exception = new InvalidOperationException($"No correct factory type found for data: {data}. Cannot delete data. Create a factory that implements {nameof(IDataFactory)} or {nameof(IDataFactoryAsync)}.");
                    logger.LogWarning(exception,$"Error on Dispose with created data: {data}");
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

    #region New data
    
    //list
    private IList<TRet> NewData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: Creation of {size} data for {typeof(TDataFactory)} with was called");
        
        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<TRet>();

        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var data =   NewData(createFunc,out var createdId, args);
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogDebug($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
    //one
    private TRet NewData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,out long createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(NewDataAsync)}]: Creation of data for {typeof(TDataFactory)}  with type {typeof(TRet)} was called");
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = Counter.Increment();
        //Create the data
        return CreateData(createFunc, createdId, args, factory);
    }

    private TRet CreateData<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, TRet> createFunc, long createdId, IDataParams? args, TDataFactory factory)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        TRet data;
        try
        {
            data = createFunc(factory, createdId, args);
        }
        catch (Exception e)
        {
            Dispose();
            var ex = new InvalidOperationException($"Failed to create data. Create method throw exception for {typeof(TDataFactory)}" +
                                                   $" with type {typeof(TRet)}, id {createdId} and arguments {args}",e);
            logger.LogError(ex,$"Creation of new data failed for {typeof(TDataFactory)} with type {typeof(TRet)}");
            throw ex;
        }
        
        AddDataToHistory<TDataFactory>(createdId, args, data,factory);
        logger.LogDebug($"[{nameof(NewDataAsync)}]: Created data for {typeof(TDataFactory)} with id {createdId} and type {typeof(TRet)}");

        return data;
    }
    //list
    private IList<Task<TRet>> NewDataAsync<TRet,TDataFactory>(Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc,int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: Creation of {size} data for {typeof(TDataFactory)} with was called");
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");

        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<Task<TRet>>();
        
        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var createdId = Counter.Increment();
            Task<TRet> data =  CreateDataAsync(createFunc, args, factory, createdId);
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogDebug($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
    //one
    private  Task<TRet> NewDataAsync<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, Task<TRet>> createFunc, out long createdId ,IDataParams? args = null) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        //Update the global data cache
        createdId = Counter.Increment();
        return CreateDataAsync(createFunc, args, factory, createdId);
    }

    private async Task<TRet> CreateDataAsync<TRet, TDataFactory>(Func<TDataFactory, long, IDataParams?, Task<TRet>> createFunc, IDataParams? args, TDataFactory factory,
        long createdId) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(NewDataAsync)}]: Creation of data for {typeof(TDataFactory)}  with type {typeof(TRet)} was called");
        //Create the data
        TRet data;
        try
        {
            data = await createFunc(factory, createdId, args).ConfigureAwait(false);
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
        
        AddDataToHistory<TDataFactory>(createdId, args, data,factory);
        logger.LogDebug($"[{nameof(NewDataAsync)}]: Created data for {typeof(TDataFactory)} with id {createdId} and type {typeof(TRet)}");

        return data;
    }
    
    private bool AddDataToHistory<TDataFactory>(long createdId, IDataParams? args, object data, IDataFactoryBase factoryBase)
        where TDataFactory : IDataFactoryBase
    {
        if(data == null)
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to create data. Create method returned null for {typeof(TDataFactory)}" +
                                                  $" id {createdId} and arguments {args}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)}");
            throw e;
        }

        IFactoryData factoryData =  new FactoryData(createdId, data, args, factoryBase);
        
        _createdHistory.Push(factoryData);
        
        //Get the local data history
        HistoryStore< IFactoryData> historyStore;
        try
        {
            historyStore = _localDataCache.GetOrAdd(typeof(TDataFactory),_ => new());
        }
        catch (Exception e)
        {
            logger.LogError("Failed to create history for {typeof(TDataFactory)}",e);
            Dispose();
            throw new InvalidOperationException($"Failed to create history for {typeof(TDataFactory)}",e);
        }
        //Update the local data history
        if(!historyStore.TryAdd(createdId,factoryData))
        {
            Dispose();
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {data.GetType()}");
            throw e;
        }

        return true;
    }

    #endregion
    #region Get data
    private TRet GetData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,out long createdId) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (TryGetLatest<TRet, TDataFactory>(out long? id, out var ret))
        {
            createdId = id!.Value;
            return ret!;
        }

        logger.LogDebug($"[{nameof(Get)}]: No created data found for {typeof(TDataFactory)}");
        return NewData(createFunc,out createdId);
    }
    private IList<TRet> GetData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (TryGetLatest<TRet, TDataFactory>(size, out createdIds, out var retData)) return retData;
        while (retData.Count < size)
        {
            var newItem = NewData(createFunc,out var createdId);
            retData.Insert(0, newItem); // Insert new data at the beginning
            createdIds.Insert(0, createdId); // Insert the new createdId at the beginning
        }
        logger.LogInformation($"[{nameof(Get)}]: Retrieved {size} data for {typeof(TDataFactory)}");
        return retData;
    }
    
    private Task<TRet> GetDataAsync<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, Task<TRet>> createFunc,out long createdId) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if (TryGetLatest<TRet, TDataFactory>(out long? id, out var ret))
        {
            createdId = id!.Value;
            return Task.FromResult(ret)!;
        }

        logger.LogDebug($"[{nameof(Get)}]: No created data found for {typeof(TDataFactory)}");
        return NewDataAsync(createFunc,out createdId);
    }
    private IList<Task<TRet>> GetDataAsync<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, Task<TRet>> createFunc,int size, out IList<long> createdIds) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        var ret = TryGetLatest<TRet, TDataFactory>(size, out createdIds, out var latestData) ;
        
        IList<Task<TRet>> retData = latestData.Select(Task.FromResult).ToList();
        if(ret) return retData;
        
        while (retData.Count < size)
        {
            var newItem = NewDataAsync(createFunc,out var createdId);
            retData.Insert(0, newItem); // Insert new data at the beginning
            createdIds.Insert(0, createdId); // Insert the new createdId at the beginning
        }
        logger.LogInformation($"[{nameof(Get)}]: Retrieved {size} data for {typeof(TDataFactory)}");
        return retData;
    }
    private bool TryGetLatest<TRet, TDataFactory>(out long? createdId, out TRet? ret)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (history.TryGetLatest(out var item,out createdId))
            {
                logger.LogDebug($"[{nameof(Get)}]: Retrieved data for {typeof(TDataFactory)} with id {createdId}");
                if ((item as FactoryData)!.Data is not TRet data)
                {
                    Dispose();
                    throw CastExeption(logger,
                        $"Data is not of type {typeof(TRet)} for operation {nameof(Get)}.");
                }

                ret = data;
                return true;
            }
        }
        
        createdId = null;
        ret = default;
        return false;
    }

    private bool TryGetLatest<TRet, TDataFactory>(int size, out IList<long> createdIds, out IList<TRet> retData) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var historyStore))
        {
            historyStore.TryGetLatest(size, out var data, out var ids);
            logger.LogDebug($"[{nameof(Get)}]: Retrieved from history {data.Count} data for {typeof(TDataFactory)}");
            var enumData = data.Select(o => o.Data);
            try
            {
                retData = enumData.Cast<TRet>().ToList();
                createdIds = ids.ToList();
                return data.Count == size;
            }
            catch (InvalidCastException e)
            {
                Dispose();
                throw CastExeption(logger,$"Data is not of type {typeof(TRet)} for operation {nameof(Get)}.",e);
            }
        }
        createdIds = new List<long>();
        retData = new List<TRet>();
        return false;
    }

    #endregion
    private  Exception CastExeption(ILogger log,string text, Exception? exception = null)
    {
        var ex = new InvalidCastException(text,exception);
        log.LogError(ex,"Error in cast:");
        return ex;
    }
    #endregion
}

