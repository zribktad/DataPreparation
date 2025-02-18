using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

public interface ISourceFactory
{
    public T New<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T>  => New<T,TDataFactory>(out _ ,args);
    public T New<T, TDataFactory>(out long createdId, IDataParams? args = null ) where TDataFactory : IDataFactory<T>;
    public IEnumerable<T> New<T,TDataFactory>(int size, IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T> => New<T,TDataFactory>(size,out _ ,argsEnumerable) ;

    public IEnumerable<T> New<T, TDataFactory>(int size, out IList<long> createdIds,
        IEnumerable<IDataParams?>? argsEnumerable = null) where TDataFactory : IDataFactory<T>;
    public IEnumerable<T> Was<T,TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory<T>;
    public T? Get<T,TDataFactory>(long createdId) where TDataFactory : IDataFactory<T>;
    
    
    
    
}