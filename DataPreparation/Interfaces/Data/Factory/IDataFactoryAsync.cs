namespace DataPreparation.Data.Setup;

public interface IDataFactoryAsync : IDataFactoryBase // TODO 
{
    Task<object>  Create(long createId, IDataParams? args);
    Task<bool> Delete(long createId, object data, IDataParams? args);
}

public interface IDataFactoryAsync<T> : IDataFactoryAsync where T : notnull
{
    new Task<T> Create(long createId, IDataParams? args);
    Task<bool> Delete(long createId, T data, IDataParams? args);
    Task<object> IDataFactoryAsync.Create(long createId, IDataParams? args) => Task.FromResult<object>(Create(createId, args));

    Task<bool> IDataFactoryAsync.Delete(long createId, object data, IDataParams? args) =>
        data is T t ? Delete(createId, t, args) : throw new InvalidCastException($"Data is not of type {typeof(T)}");
}