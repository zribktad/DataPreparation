using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;
public interface IDataFactory : IDataRegister // TODO 
{
    object Create(long createId, IDataParams? args);
    
}
public interface IDataFactory<T> :IDataRegister<T>, IDataFactory where T : notnull
{
    new T Create(long createId, IDataParams? args);
    
    // T Create(long createId, ListParams? args) => Create(createId, args); //TODO split it in create mechanics params
    // T Create(long createId, DictParams? args) => Create(createId, args); //TODO

    // Default Implementations
    object IDataFactory.Create(long id, IDataParams? args) => Create(id, args);
    
}

