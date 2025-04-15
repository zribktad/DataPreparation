namespace DataPreparation.Data.Setup;

public interface IDataFactoryAsync :IDataRegisterAsync // TODO 
{
    Task<object>  Create(long createId, IDataParams? args, CancellationToken token = default);
}

public interface IDataFactoryAsync<T> : IDataRegisterAsync<T>, IDataFactoryAsync  where T : notnull
{
    Task<T> Create(long createId, IDataParams? args, CancellationToken token = default);
    async Task<object> IDataFactoryAsync.Create(long createId, IDataParams? args, CancellationToken token)
    {
        var task = Create(createId, args, token);
        return task.IsCompletedSuccessfully
            ? task.Result
            : await task.ConfigureAwait(false);
    }


}