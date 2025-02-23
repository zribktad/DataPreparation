using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Engine;

namespace DataPreparation.Factory.Testing;

public class SourceFactory(IServiceProvider serviceProvider, ILogger<ISourceFactory> logger) : ISourceFactory
{
    private readonly ConcurrentDictionary<Type, HistoryStore<long,IFactoryData>> _localDataCache = new();
    private static readonly ThreadSafeCounter Counter = new(); 
    #region New
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        logger.LogDebug($"[{nameof(New)}]: New data for {typeof(TDataFactory)} was called");
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = Counter.Increment();
     
        var data = factory.Create(createdId,args);
        
        var dataCache = _localDataCache.GetOrAdd(typeof(TDataFactory),_ => new());
        
        if(!dataCache.TryAdd(createdId,new FactoryData<T?>(createdId,data, args)))
        {
            throw new InvalidOperationException($"Failed to add data to cache for {typeof(TDataFactory)}");
        }
        logger.LogInformation($"[{nameof(New)}]: Created data for {typeof(TDataFactory)} with id {createdId}");
        return data;
    }
    
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where T : notnull 
        where TDataFactory : IDataFactory<T>
    {   
        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<T>();

        for (int i = 0; i < size; i++)
        {
            var data = New<T, TDataFactory>(out var createdId, argsList.ElementAtOrDefault(i));
            createdIds.Add(createdId);
            items.Add(data);
        }
        logger.LogInformation($"[{nameof(New)}]: Created {size} data for {typeof(TDataFactory)}");
        return items;
    }
    #endregion
    #region Was

    public IEnumerable<T> Was<T, TDataFactory>( out IEnumerable<long> createdIds, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        if (_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            logger.LogInformation($"[{nameof(Was)}]: Retrieved data for {typeof(TDataFactory)}");
            return data.GetAll(out createdIds).Select(o => o.Data).Cast<T>();
        }
        logger.LogInformation($"[{nameof(Was)}]: No data found for {typeof(TDataFactory)}");
        createdIds = new List<long>();
        return new List<T>();
    }
    #endregion
    #region Get

    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            var factoryData = data.GetById(createdId);
            if (factoryData != null)
            {
                logger.LogInformation($"[{nameof(GetById)}]: Data for id {createdId} was found");
                return ((factoryData as FactoryData<T>)!).GetData();
            }
        }
        logger.LogInformation($"[{nameof(GetById)}]: No data found for {typeof(TDataFactory)} with id {createdId}");
        return default;
    }
    
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
     
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (history.GetLatest(out var item,out createdId))
            {
                logger.LogInformation($"[{nameof(Get)}]: Retrieved data for {typeof(TDataFactory)} with id {createdId}");
                return (item as FactoryData<T>)!.GetData();
            }
        }

        logger.LogInformation($"[{nameof(Get)}]: No data found for {typeof(TDataFactory)}");
        return New<T, TDataFactory>(out createdId);
        
    }
    
    
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory<T> where T : notnull
    {
        IList<T> retData = new List<T>();
        createdIds = new List<long>();
        
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var historyStore))
        {
            var data = historyStore.GetLatest(size,out var ids).ToList();
            createdIds = ids.ToList();
            logger.LogDebug($"[{nameof(Get)}]: Retrieved from history {data.Count} data for {typeof(TDataFactory)}");
            if (data.Count == size)
                return data.Select(o => (o as FactoryData<T>)!.GetData()).ToList();
        }
        while (retData.Count < size)
        {
            var newItem = New<T, TDataFactory>(out var createdId);
            retData.Insert(0, newItem); // Insert new data at the beginning
            createdIds.Insert(0, createdId); // Insert the new createdId at the beginning
        }
        logger.LogInformation($"[{nameof(Get)}]: Retrieved {size} data for {typeof(TDataFactory)}");
        return retData;
    }
    #endregion
    
    #region Dispose
    
    ~SourceFactory()
    {
       
    }
  

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        logger.LogInformation("Disposing SourceFactory");
        foreach (var (factoryType, historyStore) in _localDataCache)
        {
            if (historyStore.IsEmpty()) continue;
            var factory = (serviceProvider.GetRequiredService(factoryType) as IDataFactory) ??
                          throw new InvalidOperationException($"No factory found for {factoryType}.");

            foreach (var data in historyStore.GetAll(out _))
            {
                if (!factory.Delete(data.Id, data.Data, data.Args))
                {
                    logger.LogWarning($"Failed to delete data for {factoryType} with id {data.Id}");
                }
                else
                {
                    logger.LogInformation($"Deleted data for {factoryType} with id {data.Id}");
                }
            }
        }
        _localDataCache.Clear();
        logger.LogInformation("Disposed SourceFactory");
    }

    #endregion
    
    
    
}

