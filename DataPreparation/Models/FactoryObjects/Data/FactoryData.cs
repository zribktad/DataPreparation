using DataPreparation.Data.Setup;
using DataPreparation.Testing.Factory;

namespace DataPreparation.Factory.Testing;

public class FactoryData(long id, object data, IDataParams? args, IDataFactoryBase factoryBase) : IFactoryData
{
    public long Id { get; } = id;
    public object Data { get; } = data ?? throw new ArgumentNullException(nameof(data));
    public IDataParams? Args { get; } = args;

    public IDataFactoryBase FactoryBase { get; } = factoryBase;
    
    public  T GetData<T>()
    {
        if (Data is T data) return data;
        throw new InvalidCastException($"Data is not of type {typeof(T)}");
    }

    public override string ToString()
    {
        return $"Id: {Id}, Data: {Data}, Args: {Args}, FactoryBase: {FactoryBase}";
    }
}


