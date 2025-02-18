using DataPreparation.Data.Setup;

namespace DataPreparation.Factory.Testing;

public record ObjectsParams(params object[] args) : IDataParams
{
    public object Args { get; init; } = args;
    public static ObjectsParams Use(params object[] args) => new ObjectsParams(args);
}