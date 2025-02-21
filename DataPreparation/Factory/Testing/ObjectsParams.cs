using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record ObjectsParams(params object[] Args) : IDataParams
{
    //Find an object of type T that satisfies the predicate
    public object? Find<T>(Func<T, bool>? predicate = null) => Array.Find(Args, arg => arg.GetType() == typeof(T) && (predicate?.Invoke((T)arg) ?? true)); 

    public static ObjectsParams Use(params object[] args) => new ObjectsParams(args);
}