namespace DataPreparation.Data.Setup;

public interface IDataFactory // TODO 
{
    object Create(long id,IDataParams? args );
    bool Delete(long id, object data, IDataParams? args);
}

public interface IDataFactory<T> : IDataFactory
{
    T Create(long id, IDataParams? args);
    bool Delete(long id, T data, IDataParams? args);

    // Default Implementations
    object IDataFactory.Create(long id, IDataParams? args) => Create(id, args);

    bool IDataFactory.Delete(long id, object data, IDataParams? args) =>
        data is T t ? Delete(id, t, args) : throw new InvalidCastException($"Data is not of type {typeof(T)}");
}