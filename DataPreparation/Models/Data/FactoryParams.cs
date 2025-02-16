using DataPreparation.Data.Setup;

namespace DataPreparation.Models.Data;

public class ObjectsParams(params object[] args) : IDataParams
{
    public object Args { get; init; } = args;
    public static ObjectsParams Use(params object[] args) => new ObjectsParams(args);
}
public class DictParams(Dictionary<object, object>[] args) : IDataParams
{
    public object Args { get; init; } = args;
    static DictParams Use(Dictionary<object, object>[] args) => new DictParams(args);
   // static DictParams Use(params object[] args) => new DictParams(args.ToDictionary(arg => arg.GetType(), arg => arg));
}