using DataPreparation.Data.Setup;
using DataPreparation.Testing.Factory;

namespace DataPreparation.Factory.Testing;

public class FactoryData<T> : IFactoryData
{
    public FactoryData(long id,T data ,IDataParams? args) 
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Id = id;
        Args = args;
    }

    public long Id { get; }
    public  object Data { get; }
    public IDataParams? Args { get; }
    
    public  T GetData()
    {
        if (Data is T data) return data;
        
        throw new InvalidCastException($"Data is not of type {typeof(T)}");
    }
    
}


