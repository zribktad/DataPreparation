namespace DataPreparation.Data.Setup;

public interface IDataRegister<T>:IDataRegister, IDataFactoryBase<T> where T : notnull
{ 
    bool Delete(long createId, T data, IDataParams? args);
    
    bool  IDataRegister.Delete(long createId, object data, IDataParams? args) => Delete(createId, (T)data, args);
}

public interface IDataRegister: IDataFactoryBase 
{ 
    bool Delete(long createId, object data, IDataParams? args);
}