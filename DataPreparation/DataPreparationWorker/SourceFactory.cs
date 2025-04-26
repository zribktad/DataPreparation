using System.Collections.Concurrent;
using DataPreparation.Data.Setup;
using DataPreparation.Exceptions;
using DataPreparation.Models.Data;
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
    /// <summary>Dictionary that tracks created objects grouped by factory type</summary>
    private readonly ConcurrentDictionary<Type, HistoryStore<IFactoryData>> _factoryHistory = new();
    
    /// <summary>Main history store that tracks all created objects across all factory types</summary>
    private readonly HistoryStore<IFactoryData> _allHistory = new();
    
    /// <summary>Thread-safe ID generator for creating unique IDs for each data object</summary>
    private static readonly ThreadSafeCounter IdGenerator = new(); 
    
    #region New
    #region New Synchronous Methods
    /// <summary>
    /// Creates a new instance of an object using the specified data factory.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>A new instance created by the factory</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
    public object New<TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory
    {
        return NewData<object,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }

    /// <summary>
    /// Creates a new instance of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>A new instance of type T created by the factory</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull
    {
        return NewData<T,TDataFactory>( (factory, id, a) => factory.Create(id, a),out createdId, args);
    }

    /// <summary>
    /// Creates multiple instances of objects using the specified data factory.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional parameters to pass to the factory for each created instance</param>
    /// <returns>A list of created instances</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory
    {
        return NewData<object,TDataFactory>((factory, id, a) => factory.Create(id, a),size, out createdIds, argsEnumerable);
    }
    
    /// <summary>
    /// Creates multiple instances of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of objects to create</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional parameters to pass to the factory for each created instance</param>
    /// <returns>A list of created instances of type T</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
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
    #region Get Synchronous Methods
    /// <summary>
    /// Gets the latest instance created by the specified factory, or creates a new one if none exists.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the returned object</param>
    /// <returns>An existing or newly created instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
    /// <exception cref="InvalidCastException">Thrown when the retrieved object is not of the expected type</exception>
    public object Get<TDataFactory>(out long createdId) where TDataFactory : IDataFactory
    {
        return GetData<object,TDataFactory>((factory, id, a) => factory.Create(id, a), out createdId);
    }
    
    /// <summary>
    /// Gets the latest instance of type T created by the specified factory, or creates a new one if none exists.
    /// </summary>
    /// <typeparam name="T">The type of object to get</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the returned object</param>
    /// <returns>An existing or newly created instance of type T</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved or creation fails</exception>
    /// <exception cref="InvalidCastException">Thrown when the retrieved object is not of type T</exception>
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
    
    /// <summary>
    /// Retrieves all objects that were previously created by the specified factory.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory</typeparam>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved objects</param>
    /// <returns>A list of all previously created objects by this factory</returns>
    public IList<object> Was<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase
    {
        return WasData<TDataFactory>(out createdIds);
    }
    
    /// <summary>
    /// Retrieves all objects of type T that were previously created by the specified factory.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory</typeparam>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved objects</param>
    /// <returns>A list of all previously created objects of type T by this factory</returns>
    /// <exception cref="InvalidCastException">Thrown when any retrieved object is not of type T</exception>
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return WasData<TDataFactory>(out createdIds).Cast<T>().ToList();
    }

    /// <summary>
    /// Registers an existing object with the specified factory.
    /// This allows tracking and cleanup of objects not created by the factory.
    /// </summary>
    /// <typeparam name="T">The type of object to register</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="createdId">Output parameter that receives the ID assigned to the object, or null if registration fails</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration succeeded, false otherwise</returns>
    public bool Register<T, TDataFactory>(T data, out long? createdId,IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T>
    {
        return Register<TDataFactory>(data, out createdId, args);
    }

    /// <summary>
    /// Registers an existing object with the specified factory.
    /// This allows tracking and cleanup of objects not created by the factory.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="createdId">Output parameter that receives the ID assigned to the object, or null if registration fails</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration succeeded, false otherwise</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factory cannot be resolved</exception>
    public bool Register<TDataFactory>(object data, out long? createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase
    {
        var factoryBase = serviceProvider.GetService<TDataFactory>() ??
                          throw new InvalidOperationException($"No factory or register found for {typeof(TDataFactory)}");
        var id = IdGenerator.GetNextId();
        if (AddDataToHistory<TDataFactory>(id, args, data, factoryBase))
        {
            createdId = id;
            return true;
        }
        createdId = null;
        return false;
    }

    /// <summary>
    /// Implementation of the Was operation that retrieves all objects created by the specified factory type.
    /// </summary>
    private IList<object> WasData<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase
    {
        if (_factoryHistory.TryGetValue(typeof(TDataFactory), out var data))
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
    
    /// <summary>
    /// Retrieves an object by its ID from the specified factory's history.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of the factory that created the object</typeparam>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The object with the specified ID, or null if not found</returns>
    public object? GetById<TDataFactory>(long createdId) where TDataFactory : IDataFactory
    {
        if(_factoryHistory.TryGetValue(typeof(TDataFactory), out var history))
        {
            if (GetByIdForHistory(createdId, history, out var data)) return data;
        }
      
        logger.LogInformation($"[{nameof(GetById)}]: No data found for id {createdId}");
        return default;
    }
    
    /// <summary>
    /// Retrieves an object by its ID from any factory history.
    /// </summary>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The object with the specified ID, or null if not found</returns>
    public object? GetById(long createdId)
    {
        foreach (var (_,history) in _factoryHistory)
        {
            if (GetByIdForHistory(createdId, history, out var data)) return data;
        }

        logger.LogInformation($"[{nameof(GetById)}]: No data found for id {createdId}");
        return default;
    }

    /// <summary>
    /// Helper method that attempts to retrieve an object by ID from a specific history store.
    /// </summary>
    /// <param name="createdId">The ID to look up</param>
    /// <param name="history">The history store to search</param>
    /// <param name="data">Output parameter that receives the found object, or null if not found</param>
    /// <returns>True if the object was found, false otherwise</returns>
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

    /// <summary>
    /// Retrieves an object of type T by its ID from the specified factory's history.
    /// </summary>
    /// <typeparam name="T">The expected type of the object</typeparam>
    /// <typeparam name="TDataFactory">The type of the factory that created the object</typeparam>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The object with the specified ID cast to type T, or default(T) if not found</returns>
    /// <exception cref="InvalidCastException">Thrown when the found object is not of type T</exception>
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull
    {
       var foundData = GetById<TDataFactory>(createdId);
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
    
    /// <summary>
    /// Asynchronously disposes of all created data objects, invoking their cleanup logic.
    /// This is called automatically when the test completes to ensure proper cleanup.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation</returns>
    /// <exception cref="AggregateException">Contains all exceptions that occurred during cleanup</exception>
    public async ValueTask DisposeAsync()
    {
        logger.LogDebug("Factory disposing");
        var exceptionAggregator = new ExceptionAggregator();
        
        // Process each data item in LIFO order (last created, first deleted)
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
                bool deleted;
                switch (data.FactoryBase)
                {
                    case IDataRegister sync:
                        // Use synchronous delete method for IDataRegister
                        deleted = sync.Delete(data.Id, data.Data, data.Args);
                        break;
                    case IDataRegisterAsync async:
                        // Use asynchronous delete method for IDataRegisterAsync
                        deleted = await async.Delete(data.Id, data.Data, data.Args).ConfigureAwait(false);
                        break;
                    default:
                        // If the factory doesn't implement either interface, log an error
                        AddException( $"No correct factory type found for data: {data.FactoryBase}. Cannot delete data. " +
                                      $"Create a factory that implements Data Factory Object interface.");
                        continue;
                }

                // Log success or failure of the delete operation
                if (deleted)
                    logger.LogInformation($"Deleted data for {factoryType} with id {data.Id}");
                else
                    AddException($"Failed to delete data for {factoryType} with id {data.Id} and arguments {data.Args}");
            }
            catch (Exception e)
            {
                // Collect exceptions during cleanup
                AddException($"Error to delete data for {factoryType} with id {data.Id} and arguments {data.Args}", e);
            }
        }
        
        // Clear all history stores
        _factoryHistory.Clear();
        _allHistory.Clear();
        
        // If any exceptions occurred during cleanup, throw them as an aggregate
        if (exceptionAggregator.HasExceptions) throw exceptionAggregator.Get()!;
        logger.LogInformation("Factory disposed - Data deleted");
        
        // Helper method to add exceptions to the aggregator
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
            var createdId = IdGenerator.GetNextId();
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
        createdId = IdGenerator.GetNextId();
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
            var createdId = IdGenerator.GetNextId();
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
        createdId = IdGenerator.GetNextId();
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

        if (!_allHistory.TryPush(createdId, factoryData))
        {
            var e = new InvalidOperationException($"Failed to add data to history for {typeof(TDataFactory)}");
            logger.LogError(e,$"Creation of new data failed for {typeof(TDataFactory)} with type {data.GetType()}");
            throw e;
        }

        //Get the local data history
        HistoryStore< IFactoryData> historyStore;
        try
        {
            historyStore = _factoryHistory.GetOrAdd(typeof(TDataFactory),_ => new());
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
        if(_factoryHistory.TryGetValue(typeof(TDataFactory), out var history))
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
        if(_factoryHistory.TryGetValue(typeof(TDataFactory), out var historyStore))
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
    /// <summary>
    /// Creates a properly formatted InvalidCastException with logging.
    /// </summary>
    /// <param name="log">The logger to use</param>
    /// <param name="text">The error message</param>
    /// <param name="exception">Optional inner exception</param>
    /// <returns>An InvalidCastException with the specified message</returns>
    private InvalidCastException CastExeption(ILogger log, string text, Exception? exception = null)
    {
        var ex = new InvalidCastException(text, exception);
        log.LogError(ex, "Error in cast:");
        return ex;
    }
    #endregion
}

