using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;
public interface IDataFactory : IDataFactoryBase // TODO 
{
    object Create(long createId, IDataParams? args);
    bool Delete(long createId, object data, IDataParams? args);
    
}
public interface IDataFactory<T> : IDataFactory where T : notnull
{
    new T Create(long createId, IDataParams? args);
    bool Delete(long createId, T data, IDataParams? args);
    
    // T Create(long createId, ObjectsParams? args) => Create(createId, args); //TODO split it in create mechanics params
    // T Create(long createId, DictParams? args) => Create(createId, args); //TODO

    // Default Implementations
    object IDataFactory.Create(long id, IDataParams? args) => Create(id, args);

    bool IDataFactory.Delete(long id, object data, IDataParams? args) =>
        data is T t ? Delete(id, t, args) : throw new InvalidCastException($"Data is not of type {typeof(T)}");
}

