namespace DataPreparation.Data.Setup;

public interface IDataFactoryBase 
{
  
}

public interface IDataFactoryBase<T> : IDataFactoryBase where T : notnull
{
  
}