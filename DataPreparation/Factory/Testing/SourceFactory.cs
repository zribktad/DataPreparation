using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Extensions;
using DataPreparation.Models.Data;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Factory.Testing;

public class SourceFactory(IServiceProvider serviceProvider) : ISourceFactory, IDisposable
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<long,IFactoryData>> _localDataCache = new(); //only for one test and one thread, what if user in test se more than one thread? 
    private static readonly ConcurrentDictionary<Type,ThreadSafeCounter> GlobalIdCache = new(); // global id cache for all factories and threads
    #region New
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T>
    {
        //Get the factory
        var factory = serviceProvider.GetService<TDataFactory>() ?? throw new InvalidOperationException($"No factory found for {typeof(TDataFactory)}.");
        
        //Update the global data cache
        createdId = GlobalIdCache.GetOrAdd(typeof(TDataFactory), new ThreadSafeCounter()).Increment();
        
        var data = factory.Create(createdId,args);
        
        var dataCache = _localDataCache.GetOrAdd(typeof(TDataFactory), new ConcurrentDictionary<long, IFactoryData>());;
        
        if(dataCache.TryAdd(createdId,new FactoryData<T?>(data, args)) == false)
        {
            throw new InvalidOperationException($"Failed to add data to cache for {typeof(TDataFactory)}");
        }

      
        return data;
    }
    
    public IEnumerable<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) 
        where TDataFactory : IDataFactory<T>
    {   
        var argsList = argsEnumerable?.ToList() ?? new List<IDataParams?>();
        createdIds = new List<long>();
        var items = new List<T>();

        for (int i = 0; i < size; i++)
        {
            var data = New<T, TDataFactory>(out long createdId, argsList.ElementAtOrDefault(i));
            createdIds.Add(createdId);
            items.Add(data);
        }

        return items;
    }
    #endregion
    #region Was

    public IEnumerable<T> Was<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T>
    {
        if (_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            return data.Values.Select(o => o.Data).Cast<T>();
        }
        return new List<T>();
    }
    #endregion
    #region Get

    public T? Get<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T>
    {
        if(_localDataCache.TryGetValue(typeof(TDataFactory), out var data))
        {
            if (data?.TryGetValue(createdId, out var factoryData) == true)
            {
                return ((factoryData as FactoryData<T>)!).GetData();
            }
        }
        return default;
    }
    #endregion
    
    #region Dispose
    
    public void Dispose()
    {
        foreach (var (dataType,data) in _localDataCache)
        {
            if (!data.Any()) continue;
            
            IDataFactory factory = (serviceProvider.GetRequiredService(dataType) as IDataFactory) ?? throw new InvalidOperationException($"No factory found for {dataType}.");
            foreach (var (id, obj) in data)
            {
                factory.Delete(id, obj.Data, obj.Args);
            }

        }
        _localDataCache.Clear();
        
    }
    #endregion
    
    
    
}

