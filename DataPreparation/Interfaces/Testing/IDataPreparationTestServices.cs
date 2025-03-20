using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Testing;

public interface IDataPreparationTestServices
{
     void  DataPreparationServices(IServiceCollection serviceCollection);
        
}