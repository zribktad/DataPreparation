using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Factory.Testing;

public class SourceFactory(IServiceProvider serviceProvider) : ISourceFactory, IDisposable
{
    private readonly ConcurrentDictionary<Type, HistoryStore<long,IFactoryData>> _localDataCache = new();
    private static readonly ThreadSafeCounter Counter = new(); 
    #region New
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = Counter.Increment();
        
        var data = factory.Create(createdId,args);
        
        var dataCache = _localDataCache.GetOrAdd(typeof(TDataFactory), new HistoryStore<long, IFactoryData>());
        
        if(!dataCache.TryAdd(createdId,new FactoryData<T?>(createdId,data, args)))
        {
            throw new InvalidOperationException($"Failed to add data to cache for {typeof(TDataFactory)}");
        }
        Console.WriteLine($"Created data for {typeof(TDataFactory)} with id {createdId}");
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

        return items;
    }
    #endregion
    #region Was

    public IEnumerable<T> Was<T, TDataFactory>( out IEnumerable<long> createdIds, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        if (_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            Console.WriteLine($"Retrieved data for {typeof(TDataFactory)}");
            return data.GetAll(out createdIds).Select(o => o.Data).Cast<T>();
        }
        Console.WriteLine($"No data found for {typeof(TDataFactory)}");
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
            if (factoryData == null)
            {
                return ((factoryData as FactoryData<T>)!).GetData();
            }
        }
        Console.WriteLine($"No data found for {typeof(TDataFactory)} with id {createdId}");
        return default;
    }
    
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
     
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (history.GetLatest(out var item,out createdId))
            {
                return (item as FactoryData<T>)!.GetData();
            }
        }
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
            if (data.Count == size)
                return data.Select(o => (o as FactoryData<T>)!.GetData()).ToList();
        }
        while (retData.Count < size)
        {
            var newItem = New<T, TDataFactory>(out var createdId);
            retData.Insert(0, newItem); // Insert new data at the beginning
            createdIds.Insert(0, createdId); // Insert the new createdId at the beginning
        }
        return retData;
    }
    #endregion
    
    #region Dispose
    
    
    ~SourceFactory()
    {
        foreach (var (factoryType, historyStore) in _localDataCache)
        {
            if (historyStore.IsEmpty()) continue;
            var factory = (serviceProvider.GetRequiredService(factoryType) as IDataFactorySync) ??
                          throw new InvalidOperationException($"No factory found for {factoryType}.");

            foreach (var data in historyStore.GetAll(out _))
            {
                if (!factory.Delete(data.Id, data.Data, data.Args))
                {
                    Console.Error.WriteLine(
                        $"Failed to delete data for {factoryType} with id {data.Id} and content {data.Data}");
                }
                else
                {
                    Console.WriteLine($"Deleted data for {factoryType} with id {data.Id}");
                }
            }
        }
        _localDataCache.Clear();
        
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    #endregion
    
    
    
}

