using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

public interface IFactoryData
{
  long Id { get; }
  object Data { get; }
  IDataParams? Args { get; }
}