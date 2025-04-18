
using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

/// <summary>
/// Interface for creating and retrieving data for test.
/// </summary>
public interface ISourceFactory : IAsyncDisposable
{
    #region Asynchronous Methods

    // Creating new objects asynchronously

    #region New
    public Task<object> NewAsync<TDataFactory>(IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => NewAsync<TDataFactory>(out _, args, token);
    public Task<object> NewAsync<TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    public Task<T> NewAsync<T, TDataFactory>(IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => NewAsync<T, TDataFactory>(out _, args, token);
    public Task<T> NewAsync<T, TDataFactory>(out long createdId, IDataParams? args = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    public Task<object[]> NewAsync<TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => NewAsync<TDataFactory>(size, out _, argsEnumerable, token);
    public Task<object[]> NewAsync<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    public Task<T[]> NewAsync<T, TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => NewAsync<T, TDataFactory>(size, out _, argsEnumerable, token);
    public Task<T[]> NewAsync<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    
    #endregion
    // Retrieving or creating objects asynchronously
    #region Get
    public Task<object> GetAsync<TDataFactory>(CancellationToken token = default) where TDataFactory : IDataFactoryAsync => GetAsync<TDataFactory>(out _,token);
    public Task<object> GetAsync<TDataFactory>(out long createdId, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    public Task<T> GetAsync<T, TDataFactory>(CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => GetAsync<T, TDataFactory>(out _, token);
    public Task<T> GetAsync<T, TDataFactory>(out long createdId, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    public Task<object[]> GetAsync<TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync;
    public Task<object[]> GetAsync<TDataFactory>(int size, CancellationToken token = default) where TDataFactory : IDataFactoryAsync => GetAsync<TDataFactory>(size, out _, token);
    public Task<T[]> GetAsync<T, TDataFactory>(int size, out IList<long> createdIds, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull;
    public Task<T[]> GetAsync<T, TDataFactory>(int size, CancellationToken token = default) where TDataFactory : IDataFactoryAsync<T> where T : notnull => GetAsync<T, TDataFactory>(size, out _, token);
    #endregion
    #endregion

    #region Synchronous Methods

    // Creating new objects synchronously
    #region New
    public object New<TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory => New<TDataFactory>(out _, args);
    public object New<TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory;
    public T New<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(out _, args);
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null) where TDataFactory : IDataFactory<T> where T : notnull;
    public IList<object> New<TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory => New<TDataFactory>(size, out _, argsEnumerable);
    public IList<object> New<TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory;
    public IList<T> New<T, TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull => New<T, TDataFactory>(size, out _, argsEnumerable);
    public IList<T> New<T, TDataFactory>(int size, out IList<long> createdIds, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> where T : notnull;
    #endregion
    // Retrieving or creating objects synchronously
    #region Get
    public object Get<TDataFactory>() where TDataFactory : IDataFactory => Get<TDataFactory>(out _);
    public object Get<TDataFactory>(out long createdId) where TDataFactory : IDataFactory;
    public T Get<T, TDataFactory>() where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(out _);
    public T Get<T, TDataFactory>(out long createdId) where TDataFactory : IDataFactory<T> where T : notnull;
    public IList<object> Get<TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory;
    public IList<object> Get<TDataFactory>(int size) where TDataFactory : IDataFactory => Get<TDataFactory>(size, out _);
    public IList<T> Get<T, TDataFactory>(int size, out IList<long> createdIds) where TDataFactory : IDataFactory<T> where T : notnull;
    public IList<T> Get<T, TDataFactory>(int size) where TDataFactory : IDataFactory<T> where T : notnull => Get<T, TDataFactory>(size, out _);
    #endregion
    #endregion

    #region Other Methods

    // Finding by ID
    #region GetById
    public T? GetById<T, TDataFactory>(long createdId) where TDataFactory : IDataFactory<T> where T : notnull;
    public object? GetById<TDataFactory>(long createdId) where TDataFactory : IDataFactory;
    public object? GetById(long createdId);

    #endregion
    // Historical data
    #region Was
    public IList<object> Was<TDataFactory>() where TDataFactory : IDataFactoryBase => Was<TDataFactory>(out _);
    public IList<object> Was<TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase;
    public IList<T> Was<T, TDataFactory>() where TDataFactory : IDataFactoryBase<T> where T : notnull => Was<T, TDataFactory>(out _);
    public IList<T> Was<T, TDataFactory>(out IList<long> createdIds) where TDataFactory : IDataFactoryBase<T> where T : notnull;
    #endregion
    
    #region Register
    public bool Register<T,TDataFactory>(T data, out long? createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase<T> where T : notnull;
    public bool Register<T,TDataFactory>(T data, IDataParams? args = null) where TDataFactory : IDataFactoryBase<T> where T : notnull => Register<T,TDataFactory>(data, out _, args);
    
    public bool Register<TDataFactory>(object data, out long? createdId, IDataParams? args = null) where TDataFactory : IDataFactoryBase;
    public bool Register<TDataFactory>(object data, IDataParams? args = null) where TDataFactory : IDataFactoryBase => Register<TDataFactory>(data, out _, args);
    #endregion
    #endregion
}
