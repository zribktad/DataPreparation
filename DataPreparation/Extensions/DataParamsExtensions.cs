using DataPreparation.Data.Setup;

namespace DataPreparation.Extensions;

public static class DataParamsExtensions
{
    public static T? To<T>(this IDataParams dataParams) where T : class
    {
        return dataParams as T;
    }
}
