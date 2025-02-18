using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record DictParams(Dictionary<object, object>[] args) : IDataParams
{
    public object Args { get; init; } = args;
    static DictParams Use(Dictionary<object, object>[] args) => new DictParams(args);
   // static DictParams Use(params object[] args) => new DictParams(args.ToDictionary(arg => arg.GetType(), arg => arg));
}