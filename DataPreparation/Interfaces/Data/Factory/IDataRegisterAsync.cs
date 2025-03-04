namespace DataPreparation.Data.Setup;

public interface IDataRegisterAsync<T> :IDataRegisterAsync, IDataFactoryBase<T> where T : notnull
{
    Task<bool> Delete(long createId, T data, IDataParams? args);
    
    Task<bool> IDataRegisterAsync.Delete(long createId, object data, IDataParams? args) => Delete(createId, (T)data, args);

}

public interface IDataRegisterAsync : IDataFactoryBase
{
    Task<bool> Delete(long createId, object data, IDataParams? args);
}