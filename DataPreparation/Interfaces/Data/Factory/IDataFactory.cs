using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;

public interface IDataFactory // TODO 
{
  
}

public interface IDataFactorySync : IDataFactory // TODO 
{
    object Create(long createId, IDataParams? args);
    bool Delete(long createId, object data, IDataParams? args);
    
}
public interface IDataFactory<T> : IDataFactorySync where T : notnull
{
    new T Create(long createId, IDataParams? args);
    bool Delete(long createId, T data, IDataParams? args);
    
    // T Create(long createId, ObjectsParams? args) => Create(createId, args); //TODO split it in create mechanics params
    // T Create(long createId, DictParams? args) => Create(createId, args); //TODO

    // Default Implementations
    object IDataFactorySync.Create(long id, IDataParams? args) => Create(id, args);

    bool IDataFactorySync.Delete(long id, object data, IDataParams? args) =>
        data is T t ? Delete(id, t, args) : throw new InvalidCastException($"Data is not of type {typeof(T)}");
}

public interface IDataFactoryASync : IDataFactory // TODO 
{
    Task<object>  Create(long createId, IDataParams? args);
    Task<bool> Delete(long createId, object data, IDataParams? args);
}

public interface IDataFactoryAsync<T> : IDataFactoryASync where T : notnull
{
    new Task<T> Create(long createId, IDataParams? args);
    Task<bool> Delete(long createId, T data, IDataParams? args);
    Task<object> IDataFactoryASync.Create(long createId, IDataParams? args) => Task.FromResult<object>(Create(createId, args));

    Task<bool> IDataFactoryASync.Delete(long createId, object data, IDataParams? args) =>
        data is T t ? Delete(createId, t, args) : throw new InvalidCastException($"Data is not of type {typeof(T)}");
}