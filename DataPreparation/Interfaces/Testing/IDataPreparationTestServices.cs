using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Testing
{
    public interface IDataPreparationTestServices
    {
        static abstract void  DataPreparationServices(IServiceCollection serviceCollection);
        
    }
    
}
