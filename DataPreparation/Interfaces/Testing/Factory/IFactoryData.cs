using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

public interface IFactoryData
{
  object Data { get; }
  IDataParams? Args { get; }
}