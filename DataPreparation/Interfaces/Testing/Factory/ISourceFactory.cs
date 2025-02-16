using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

public interface ISourceFactory
{
    public T Get<T, TDataFactory>(IDataParams? args = null) where TDataFactory : IDataFactory;
   
    public T Get<T>(IDataParams? args = null);
    public IEnumerable<T> Get<T>(int size,IDataParams? args = null);
    public IList<T> Was<T>(IDataParams? args = null);
    public T New<T>(IDataParams? args = null);
    public IList<T> New<T>(int size,IDataParams? args = null);
}