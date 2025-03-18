using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;
public interface IDataFactory : IDataRegister
{
    object Create(long createId, IDataParams? args);
}
public interface IDataFactory<T> :IDataRegister<T>, IDataFactory where T : notnull
{
    new T Create(long createId, IDataParams? args);
    object IDataFactory.Create(long id, IDataParams? args) => Create(id, args);
    
}

