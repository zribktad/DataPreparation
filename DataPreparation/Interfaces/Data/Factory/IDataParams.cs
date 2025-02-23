using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;

public interface IDataParams
{
    public bool Find<T>( out T? result, Func<T, bool>? predicate = null) => throw new NotImplementedException();
}