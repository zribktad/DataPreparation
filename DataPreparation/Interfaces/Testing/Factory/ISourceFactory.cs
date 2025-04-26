using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

/// <summary>
/// Primary interface for creating, retrieving, and managing test data objects during tests.
/// The ISourceFactory provides a fluent API for test data creation and lifecycle management.
/// </summary>
/// <remarks>
/// ISourceFactory is the main entry point for tests to:
/// 
/// 1. Create new test data objects (New/NewAsync methods)
/// 2. Retrieve existing or create new data objects (Get/GetAsync methods)
/// 3. Query for previously created objects (Was methods)
/// 4. Retrieve objects by their ID (GetById methods)
/// 5. Register external objects for tracking (Register methods)
/// 
/// All objects created or registered with ISourceFactory are automatically tracked and cleaned up
/// when the test completes, ensuring proper isolation between tests.
/// 
/// Usage example:
/// <code>
/// // In a test method
/// var customer = PreparationContext.GetFactory().New<Customer, CustomerFactory>();
/// var order = PreparationContext.GetFactory().New<Order, OrderFactory(new OrderParams { CustomerId = customer.Id });
/// 
/// // Test assertions...
/// 
/// // No cleanup needed - automatic disposal after test completes
/// </code>
/// </remarks>
public interface ISourceFactory : IAsyncDisposable
{
    #region Asynchronous Methods

    // Creating new objects asynchronously

    #region New
    /// <summary>
    /// Asynchronously creates a new instance using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to the created instance</returns>
    public Task<object> NewAsync<TDataFactory>(IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => NewAsync<TDataFactory>(out _, args, token);
    
    /// <summary>
    /// Asynchronously creates a new instance using the specified factory type and returns its ID.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to the created instance</returns>
    public Task<object> NewAsync<TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    
    /// <summary>
    /// Asynchronously creates a new typed instance using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to the created typed instance</returns>
    public Task<T> NewAsync<T, TDataFactory>(IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => NewAsync<T, TDataFactory>(out _, args, token);
    
    /// <summary>
    /// Asynchronously creates a new typed instance using the specified factory type and returns its ID.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to the created typed instance</returns>
    public Task<T> NewAsync<T, TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    
    /// <summary>
    /// Asynchronously creates multiple instances using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of created instances</returns>
    public Task<object[]> NewAsync<TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => NewAsync<TDataFactory>(size, out _, argsEnumerable, token);
    
    /// <summary>
    /// Asynchronously creates multiple instances using the specified factory type and returns their IDs.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of created instances</returns>
    public Task<object[]> NewAsync<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    
    /// <summary>
    /// Asynchronously creates multiple typed instances using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of created typed instances</returns>
    public Task<T[]> NewAsync<T, TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => NewAsync<T, TDataFactory>(size, out _, argsEnumerable, token);
    
    /// <summary>
    /// Asynchronously creates multiple typed instances using the specified factory type and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of objects to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of created typed instances</returns>
    public Task<T[]> NewAsync<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    
    #endregion
    // Retrieving or creating objects asynchronously
    #region Get
    /// <summary>
    /// Gets an existing instance or asynchronously creates a new one using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an existing or newly created instance</returns>
    public Task<object> GetAsync<TDataFactory>(CancellationToken token = default) where TDataFactory : IDataFactoryAsync => GetAsync<TDataFactory>(out _,token);
    
    /// <summary>
    /// Gets an existing instance or asynchronously creates a new one using the specified factory type, and returns its ID.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the retrieved or created object</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an existing or newly created instance</returns>
    public Task<object> GetAsync<TDataFactory>(out long createdId, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    
    /// <summary>
    /// Gets an existing typed instance or asynchronously creates a new one using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an existing or newly created typed instance</returns>
    public Task<T> GetAsync<T, TDataFactory>(CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => GetAsync<T, TDataFactory>(out _, token);
    
    /// <summary>
    /// Gets an existing typed instance or asynchronously creates a new one using the specified factory type, and returns its ID.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the retrieved or created object</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an existing or newly created typed instance</returns>
    public Task<T> GetAsync<T, TDataFactory>(out long createdId, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    
    /// <summary>
    /// Gets existing instances or asynchronously creates new ones using the specified factory type, and returns their IDs.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved or created objects</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of existing or newly created instances</returns>
    public Task<object[]> GetAsync<TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    
    /// <summary>
    /// Gets existing instances or asynchronously creates new ones using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of existing or newly created instances</returns>
    public Task<object[]> GetAsync<TDataFactory>(int size, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => GetAsync<TDataFactory>(size, out _, token);
    
    /// <summary>
    /// Gets existing typed instances or asynchronously creates new ones using the specified factory type, and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved or created objects</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of existing or newly created typed instances</returns>
    public Task<T[]> GetAsync<T, TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    
    /// <summary>
    /// Gets existing typed instances or asynchronously creates new ones using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="token">Cancellation token to cancel the operation</param>
    /// <returns>A task that resolves to an array of existing or newly created typed instances</returns>
    public Task<T[]> GetAsync<T, TDataFactory>(int size, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => GetAsync<T, TDataFactory>(size, out _, token);
    #endregion
    #endregion

    #region Synchronous Methods

    // Creating new objects synchronously
    #region New
    /// <summary>
    /// Creates a new instance using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>The created instance</returns>
    public object New<TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory => New<TDataFactory>(out _, args);
    
    /// <summary>
    /// Creates a new instance using the specified factory type and returns its ID.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>The created instance</returns>
    public object New<TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory;
    
    /// <summary>
    /// Creates a new typed instance using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>The created typed instance</returns>
    public T New<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(out _, args);
    
    /// <summary>
    /// Creates a new typed instance using the specified factory type and returns its ID.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instance</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the created object</param>
    /// <param name="args">Optional parameters to pass to the factory</param>
    /// <returns>The created typed instance</returns>
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull;
    
    /// <summary>
    /// Creates multiple instances using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <returns>A list of created instances</returns>
    public IList<object> New<TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory => New<TDataFactory>(size, out _, argsEnumerable);
    
    /// <summary>
    /// Creates multiple instances using the specified factory type and returns their IDs.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <returns>A list of created instances</returns>
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory;
    
    /// <summary>
    /// Creates multiple typed instances using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <returns>A list of created typed instances</returns>
    public IList<T> New<T, TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(size, out _, argsEnumerable);
    
    /// <summary>
    /// Creates multiple typed instances using the specified factory type and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of objects to create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use for creating the instances</typeparam>
    /// <param name="size">The number of instances to create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the created objects</param>
    /// <param name="argsEnumerable">Optional sequence of parameters to pass to the factory for each instance</param>
    /// <returns>A list of created typed instances</returns>
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull;
    #endregion
    // Retrieving or creating objects synchronously
    #region Get
    /// <summary>
    /// Gets an existing instance or creates a new one using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <returns>An existing or newly created instance</returns>
    public object Get<TDataFactory>() where TDataFactory : IDataFactory => Get<TDataFactory>(out _);
    
    /// <summary>
    /// Gets an existing instance or creates a new one using the specified factory type, and returns its ID.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the retrieved or created object</param>
    /// <returns>An existing or newly created instance</returns>
    public object Get<TDataFactory>(out long createdId) where TDataFactory : IDataFactory;
    
    /// <summary>
    /// Gets an existing typed instance or creates a new one using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <returns>An existing or newly created typed instance</returns>
    public T Get<T, TDataFactory>() where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(out _);
    
    /// <summary>
    /// Gets an existing typed instance or creates a new one using the specified factory type, and returns its ID.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="createdId">Output parameter that receives the ID of the retrieved or created object</param>
    /// <returns>An existing or newly created typed instance</returns>
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull;
    
    /// <summary>
    /// Gets existing instances or creates new ones using the specified factory type, and returns their IDs.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved or created objects</param>
    /// <returns>A list of existing or newly created instances</returns>
    public IList<object> Get<TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory;
    
    /// <summary>
    /// Gets existing instances or creates new ones using the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <returns>A list of existing or newly created instances</returns>
    public IList<object> Get<TDataFactory>(int size) where TDataFactory : IDataFactory => Get<TDataFactory>(size, out _);
    
    /// <summary>
    /// Gets existing typed instances or creates new ones using the specified factory type, and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved or created objects</param>
    /// <returns>A list of existing or newly created typed instances</returns>
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory<T> where T : notnull;
    
    /// <summary>
    /// Gets existing typed instances or creates new ones using the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve or create</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to use</typeparam>
    /// <param name="size">The number of instances to retrieve or create</param>
    /// <returns>A list of existing or newly created typed instances</returns>
    public IList<T> Get<T, TDataFactory>(int size) where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(size, out _);
    #endregion
    #endregion

    #region Other Methods

    // Finding by ID
    #region GetById
    /// <summary>
    /// Retrieves a typed object by its ID from objects created by the specified factory type.
    /// </summary>
    /// <typeparam name="T">The expected type of the object</typeparam>
    /// <typeparam name="TDataFactory">The type of factory that created the object</typeparam>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The retrieved object if found; otherwise, default value for type T</returns>
    /// <remarks>
    /// This method allows you to retrieve a typed object that was previously created by a specific factory type.
    /// </remarks>
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull;
    
    /// <summary>
    /// Retrieves an object by its ID from objects created by the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory that created the object</typeparam>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The retrieved object if found; otherwise, null</returns>
    /// <remarks>
    /// This method allows you to retrieve an object that was previously created by a specific factory type.
    /// </remarks>
    public object? GetById<TDataFactory>(long createdId) where TDataFactory : IDataFactory;
    
    /// <summary>
    /// Retrieves an object by its ID from any factory's created objects.
    /// </summary>
    /// <param name="createdId">The ID of the object to retrieve</param>
    /// <returns>The retrieved object if found; otherwise, null</returns>
    /// <remarks>
    /// This method searches across all factory types for an object with the specified ID.
    /// </remarks>
    public object? GetById(long createdId);

    #endregion
    // Historical data
    #region Was
    /// <summary>
    /// Retrieves all objects that were previously created by the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory that created the objects</typeparam>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved objects</param>
    /// <returns>A list of all objects created by the specified factory type</returns>
    /// <remarks>
    /// This method is useful for retrieving all previously created objects of a specific factory type
    /// for verification or other test operations.
    /// </remarks>
    public IList<object> Was<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase;
    
    /// <summary>
    /// Retrieves all typed objects that were previously created by the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve</typeparam>
    /// <typeparam name="TDataFactory">The type of factory that created the objects</typeparam>
    /// <param name="createdIds">Output parameter that receives the IDs of the retrieved objects</param>
    /// <returns>A list of all typed objects created by the specified factory type</returns>
    /// <remarks>
    /// This method is useful for retrieving all previously created objects of a specific type and factory
    /// for verification or other test operations.
    /// </remarks>
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds) where T : notnull where TDataFactory : IDataFactoryBase<T>;
    
    /// <summary>
    /// Retrieves all objects that were previously created by the specified factory type.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory that created the objects</typeparam>
    /// <returns>A list of all objects created by the specified factory type</returns>
    public IList<object> Was<TDataFactory>() where TDataFactory : IDataFactoryBase => Was<TDataFactory>(out _);
    
    /// <summary>
    /// Retrieves all typed objects that were previously created by the specified factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects to retrieve</typeparam>
    /// <typeparam name="TDataFactory">The type of factory that created the objects</typeparam>
    /// <returns>A list of all typed objects created by the specified factory type</returns>
    public IList<T> Was<T, TDataFactory>() where T : notnull where TDataFactory : IDataFactoryBase<T> => Was<T, TDataFactory>(out _);
    #endregion
    
    #region Register
    /// <summary>
    /// Registers an existing typed object with the specified factory type for tracking and cleanup.
    /// </summary>
    /// <typeparam name="T">The type of object to register</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="createdId">Output parameter that receives the assigned ID, or null if registration fails</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration was successful; otherwise, false</returns>
    /// <remarks>
    /// Use this method when you have objects created outside the factory system that you want
    /// to be tracked and cleaned up automatically with the rest of your test data.
    /// </remarks>
    public bool Register<T, TDataFactory>(T data, out long? createdId, IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T>;
    
    /// <summary>
    /// Registers an existing typed object with the specified factory type for tracking and cleanup.
    /// </summary>
    /// <typeparam name="T">The type of object to register</typeparam>
    /// <typeparam name="TDataFactory">The type of factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration was successful; otherwise, false</returns>
    public bool Register<T, TDataFactory>(T data, IDataParams? args = null) where T : notnull where TDataFactory : IDataFactoryBase<T> => Register<T, TDataFactory>(data, out _, args);
    
    /// <summary>
    /// Registers an existing object with the specified factory type for tracking and cleanup.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="createdId">Output parameter that receives the assigned ID, or null if registration fails</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration was successful; otherwise, false</returns>
    /// <remarks>
    /// Use this method when you have objects created outside the factory system that you want
    /// to be tracked and cleaned up automatically with the rest of your test data.
    /// </remarks>
    public bool Register<TDataFactory>(object data, out long? createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase;
    
    /// <summary>
    /// Registers an existing object with the specified factory type for tracking and cleanup.
    /// </summary>
    /// <typeparam name="TDataFactory">The type of factory to associate with this object</typeparam>
    /// <param name="data">The object to register</param>
    /// <param name="args">Optional parameters to associate with the object</param>
    /// <returns>True if registration was successful; otherwise, false</returns>
    public bool Register<TDataFactory>(object data, IDataParams? args = null) where TDataFactory : IDataFactoryBase => Register<TDataFactory>(data, out _, args);
    #endregion
    #endregion
}
