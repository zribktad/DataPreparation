using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

/// <summary>
/// Interface for creating and retrieving data sources.
/// </summary>
public interface ISourceFactory
{
    /// <summary>
    /// Creates a new instance of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instance to create.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="args">Optional parameters for data creation.</param>
    /// <returns>A new instance of type T.</returns>
    public T New<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(out _, args);

    /// <summary>
    /// Creates a new instance of type T using the specified data factory and returns the created ID.
    /// </summary>
    /// <typeparam name="T">The type of the instance to create.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="createdId">The ID of the created instance.</param>
    /// <param name="args">Optional parameters for data creation.</param>
    /// <returns>A new instance of type T.</returns>
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull;

    /// <summary>
    /// Creates a list of new instances of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instances to create.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="size">The number of instances to create.</param>
    /// <param name="argsEnumerable">Optional parameters for data creation.</param>
    /// <returns>A list of new instances of type T.</returns>
    public IList<T> New<T, TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(size, out _, argsEnumerable);

    /// <summary>
    /// Creates a list of new instances of type T using the specified data factory and returns the created IDs.
    /// </summary>
    /// <typeparam name="T">The type of the instances to create.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="size">The number of instances to create.</param>
    /// <param name="createdIds">The IDs of the created instances.</param>
    /// <param name="argsEnumerable">Optional parameters for data creation.</param>
    /// <returns>A list of new instances of type T.</returns>
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull;

    /// <summary>
    /// Retrieves existing instances of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instances to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="args">Optional parameters for data retrieval.</param>
    /// <returns>An enumerable of existing instances of type T.</returns>
    public IEnumerable<T> Was<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull => Was<T, TDataFactory>(out _, args);

    /// <summary>
    /// Retrieves existing instances of type T using the specified data factory and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of the instances to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="ids">The IDs of the retrieved instances.</param>
    /// <param name="args">Optional parameters for data retrieval.</param>
    /// <returns>An enumerable of existing instances of type T.</returns>
    public IEnumerable<T> Was<T, TDataFactory>(out IEnumerable<long> ids, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull;

    /// <summary>
    /// Retrieves a single instance of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instance to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <returns>An instance of type T, or null if not found.</returns>
    public T Get<T, TDataFactory>() where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(out _);

    /// <summary>
    /// Retrieves a single instance of type T using the specified data factory and returns its ID.
    /// </summary>
    /// <typeparam name="T">The type of the instance to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="createdId">The ID of the retrieved instance.</param>
    /// <returns>An instance of type T, or null if not found.</returns>
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull;

    /// <summary>
    /// Retrieves a list of instances of type T using the specified data factory and returns their IDs.
    /// </summary>
    /// <typeparam name="T">The type of the instances to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="size">The number of instances to retrieve.</param>
    /// <param name="createdIds">The IDs of the retrieved instances.</param>
    /// <returns>A list of instances of type T, or null if not found.</returns>
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory<T> where T : notnull;

    /// <summary>
    /// Retrieves a list of instances of type T using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instances to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="size">The number of instances to retrieve.</param>
    /// <returns>A list of instances of type T, or null if not found.</returns>
    public IList<T> Get<T, TDataFactory>(int size) where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(size, out _);

    /// <summary>
    /// Retrieves a single instance of type T by its ID using the specified data factory.
    /// </summary>
    /// <typeparam name="T">The type of the instance to retrieve.</typeparam>
    /// <typeparam name="TDataFactory">The type of the data factory.</typeparam>
    /// <param name="createdId">The ID of the instance to retrieve.</param>
    /// <returns>An instance of type T, or null if not found.</returns>
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull;
}