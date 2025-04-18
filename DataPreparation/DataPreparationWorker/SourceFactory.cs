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
    private readonly HistoryStore<IFactoryData> _createdHistory = new();
    private static readonly ThreadSafeCounter IdGeneratorCounter = new(); 
    
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
    
    public IList<object> Was<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase
    {
        return WasData<TDataFactory>(out createdIds);
    }
    
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return WasData<TDataFactory>(out createdIds).Cast<T>().ToList();
    }

    public bool Register<T, TDataFactory>(T data, out long? createdId,IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return Register<TDataFactory>(data, out createdId, args);
    }
    public bool Register<TDataFactory>(object data, out long? createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase
    {
        var factoryBase = serviceProvider.GetService<TDataFactory>() ??
                          throw new InvalidOperationException($"No factory or register found for {typeof(TDataFactory)}");
        var id = IdGeneratorCounter.GetNextId();
        if (AddDataToHistory<TDataFactory>(id, args, data, factoryBase))
        {
            createdId = id;
            return true;
        }
        createdId = null;
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
        
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (GetByIdForHistory(createdId, history, out var data)) return data;
        }
      
        logger.LogInformation($"[{nameof(GetById)}]: No data found for id {createdId}");
        return default;
    }
    
    public object? GetById(long createdId)
    {
        foreach (var (_,history) in _localDataCache)
        {
            if (GetByIdForHistory(createdId, history, out var data)) return data;
        }

        logger.LogInformation($"[{nameof(GetById)}]: No data found for id {createdId}");
        return default;
    }

    private bool GetByIdForHistory(long createdId, HistoryStore<IFactoryData> history, out object? data)
    {
        var factoryData = history.GetById(createdId);
        if (factoryData != null)
        {
            logger.LogInformation($"[{nameof(GetById)}]: Data for id {createdId} was found");
            data = factoryData.Data;
            return true;
        }
        data = default;
        return false;
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
    

    public async ValueTask DisposeAsync()
    {
        logger.LogDebug("Factory disposing");
        var exceptionAggregator = new ExceptionAggregator();
        
        while (_createdHistory.TryPop(out var data))
        {
            if (data == null)
            {
                logger.LogWarning("Some history data are null. Cannot delete data.");
                continue;
            }

            var factoryType = data.FactoryBase.GetType();
            
            try
            {
                bool deleted;
                switch (data.FactoryBase)
                {
                    case IDataRegister sync:
                        deleted = sync.Delete(data.Id, data.Data, data.Args);
                        break;
                    case IDataRegisterAsync async:
                        deleted = await async.Delete(data.Id, data.Data, data.Args).ConfigureAwait(false);
                        break;
                    default:
                        AddException( $"No correct factory type found for data: {data.FactoryBase}. Cannot delete data. " +
                                      $"Create a factory that implements Data Factory Object interface.");
                        continue;
                }

                if (deleted)
                    logger.LogInformation($"Deleted data for {factoryType} with id {data.Id}");
                else
                    AddException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}");
            }
            catch (Exception e)
            {
                AddException($"Error to delete data for {factoryType} with id {data.Id} and arguments {data.Args}", e);
            }
        }
        
        _localDataCache.Clear();
        _createdHistory.Clear();
        
        if (exceptionAggregator.HasExceptions) throw exceptionAggregator.Get()!;
        logger.LogInformation("Factory disposed - Data deleted");
        
        
        void AddException(string message, Exception? innerException = null)
        {
            var ex = new InvalidOperationException(message, innerException);
            logger.LogError(ex, message);
            exceptionAggregator.Add(ex);
        }
    }

    #endregion
    
    #region Helper Methods

    #region New data
    
    private IList<TRet> NewData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null)
        where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: Creation of {size} data for {typeof(TDataFactory)} with was called");

        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<TRet>();
        if (size <= 0)
        {
            logger.LogWarning($"[{nameof(New)}]: No data will be created for {typeof(TDataFactory)}");
            return items;
        }
        
        if(size < argsList.Count)
        {
            logger.LogWarning($"[{nameof(New)}]: Size of requested items is smaller than the number of arguments. Only {size} data will be created");
        }
        else if(size > argsList.Count)
        {
            logger.LogInformation($"[{nameof(New)}]: Size of requested items is bigger than the number of arguments. Some items will be without arguments");
        }
        
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");

        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var createdId = IdGeneratorCounter.GetNextId();
            var data =   CreateData(createFunc, createdId, args,factory);
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogDebug($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
    private TRet NewData<TRet,TDataFactory>(Func<TDataFactory, long,IDataParams?, TRet> createFunc,out long createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase where TRet : notnull
    {
        logger.LogDebug($"[{nameof(NewDataAsync)}]: Creation of data for {typeof(TDataFactory)}  with type {typeof(TRet)} was called");
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = IdGeneratorCounter.GetNextId();
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

        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<Task<TRet>>();
           
        if (size <= 0)
        {
            logger.LogWarning($"[{nameof(New)}]: No data will be created for {typeof(TDataFactory)}");
            return items;
        }
        if(size < argsList.Count)
        {
            logger.LogWarning($"[{nameof(New)}]: Size of requested items is smaller than the number of arguments. Only {size} data will be created");
        }
        else if(size > argsList.Count)
        {
            logger.LogInformation($"[{nameof(New)}]: Size of requested items is bigger than the number of arguments. Some items will be without arguments");
        }

        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");


        for (int i = 0; i < size; i++)
        {
            var args = argsList.ElementAtOrDefault(i);
            var createdId = IdGeneratorCounter.GetNextId();
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
        createdId = IdGeneratorCounter.GetNextId();
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
            var e = new InvalidOperationException($"Failed to create data. Create method returned null for {typeof(TDataFactory)}" +
                                                  $" id {createdId} and arguments {args}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)}");
            throw e;
        }

        IFactoryData factoryData =  new FactoryData(createdId, data, args, factoryBase);

        if (!_createdHistory.TryPush(createdId, factoryData))
        {
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {data.GetType()}");
            throw e;
        }

        //Get the local data history
        HistoryStore< IFactoryData> historyStore;
        try
        {
            historyStore = _localDataCache.GetOrAdd(typeof(TDataFactory),_ => new());
        }
        catch (Exception e)
        {
            logger.LogError("Failed to create history for {typeof(TDataFactory)}",e);
            throw new InvalidOperationException($"Failed to create history for {typeof(TDataFactory)}",e);
        }
        //Update the local data history
        if(!historyStore.TryPush(createdId,factoryData))
        {
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
        if (size <= 0)
        {
            logger.LogWarning($"[{nameof(Get)}]: No data will be retrieved for {typeof(TDataFactory)}");
            createdIds = new List<long>();
            return new List<TRet>();
        }

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
        if (size <= 0)
        {
            logger.LogWarning($"[{nameof(Get)}]: No data will be retrieved for {typeof(TDataFactory)}");
            createdIds = new List<long>();
            return new List<Task<TRet>>();
        }
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
                throw CastExeption(logger,$"Data is not of type {typeof(TRet)} for operation {nameof(Get)}.",e);
            }
        }
        createdIds = new List<long>();
        retData = new List<TRet>();
        return false;
    }

    #endregion

    #region GetById

    #endregion
    private  InvalidCastException CastExeption(ILogger log,string text, Exception? exception = null)
    {
        var ex = new InvalidCastException(text,exception);
        log.LogError(ex,"Error in cast:");
        return ex;
    }
    #endregion
}

