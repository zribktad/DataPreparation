using System;
using Microsoft.Extensions.DependencyInjection;

namespace DataPreparation.Testing
{
    public interface IDataPreparationTestCase
    {
        void DataPreparationServices(IServiceCollection serviceCollection);

    }

   
}
