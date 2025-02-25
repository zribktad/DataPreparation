using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record DictParams(Dictionary<object, object>[] Args) : IDataParams
{
    static DictParams Use(Dictionary<object, object>[] args) => new DictParams(args);
   // static DictParams Use(params object[] args) => new DictParams(args.ToDictionary(arg => arg.GetType(), arg => arg));
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
}