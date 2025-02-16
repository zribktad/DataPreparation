namespace DataPreparation.Data.Setup;

public interface IDataFactory
{
}

public interface IDataFactory<T> : IDataFactory
{
    T Create(IDataParams? args );
    bool Delete(IDataParams? args);
}