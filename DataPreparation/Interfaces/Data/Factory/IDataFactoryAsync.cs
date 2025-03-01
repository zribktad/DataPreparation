namespace DataPreparation.Data.Setup;

public interface IDataFactoryAsync : IDataFactoryBase // TODO 
{
    Task<object>  Create(long createId, IDataParams? args, CancellationToken token = default);
    Task<bool> Delete(long createId, object data, IDataParams? args);
}

public interface IDataFactoryAsync<T> : IDataFactoryBase<T> ,IDataFactoryAsync where T : notnull
{
    Task<T> Create(long createId, IDataParams? args, CancellationToken token = default);
    Task<bool> Delete(long createId, T data, IDataParams? args);
    async Task<object> IDataFactoryAsync.Create(long createId, IDataParams? args, CancellationToken token)
    {
        var task = Create(createId, args, token);
        return task.IsCompletedSuccessfully
            ? task.Result
            : await task.ConfigureAwait(false);
    }

    Task<bool> IDataFactoryAsync.Delete(long createId, object data, IDataParams? args) => Delete(createId, (T)data, args);

}