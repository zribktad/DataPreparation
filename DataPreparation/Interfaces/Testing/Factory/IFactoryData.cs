using DataPreparation.Data.Setup;

namespace DataPreparation.Testing.Factory;

internal interface IFactoryData
{
  long Id { get; }
  object Data { get; }
  IDataParams? Args { get; }
  
  IDataFactoryBase FactoryBase { get; }
}