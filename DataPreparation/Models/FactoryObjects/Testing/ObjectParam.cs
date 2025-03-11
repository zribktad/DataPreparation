using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record  ObjectParam(object Record) : IDataParams
{
    public bool Find<T>(out T? result, Func<T, bool>? predicate = null)
    {
        if (Record is T t && (predicate == null || predicate(t)))
        {
            result = t;
            return true;
        }
        result = default;
        return false;
    }
}
