using DataPreparation.Data.Setup;
using DataPreparation.Testing.Factory;

namespace DataPreparation.Factory.Testing;

public class SourceFactory : ISourceFactory
{
    
    public T Get<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory
    {
        throw new NotImplementedException();
    }

    public T Get<T>(IDataParams? args = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> Get<T>(int size, IDataParams? args = null)
    {
        throw new NotImplementedException();
    }

    public IList<T> Was<T>(IDataParams? args = null)
    {
        throw new NotImplementedException();
    }

    public T New<T>(IDataParams? args = null)
    {
        throw new NotImplementedException();
    }

    public IList<T> New<T>(int size, IDataParams? args = null)
    {
        throw new NotImplementedException();
    }
}