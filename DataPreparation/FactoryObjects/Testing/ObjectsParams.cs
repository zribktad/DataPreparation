using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record ObjectsParams(params object[] Args) : IDataParams
{
    //Find an object of type T that satisfies the predicate
    public bool Find<T>(out T? result, Func<T, bool>? predicate = null)
    {
        foreach (var arg in Args)
        {
            if (arg is T t && (predicate == null || predicate(t)))
            {
                result = t;
                return true;
            }
        }

        result = default;
        return false;
    }

    public static ObjectsParams Use(params object[] args) => new ObjectsParams(args);
}