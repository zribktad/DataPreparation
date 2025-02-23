using DataPreparation.Factory.Testing;

namespace DataPreparation.Data.Setup;

public interface IDataParams
{
    //Find an object of type T that satisfies the predicate
    public bool Find<T>( out T? result, Func<T, bool>? predicate = null) => throw new NotImplementedException();
}