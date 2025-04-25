using DataPreparation.Data;
using DataPreparation.Data.Setup;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace OrderService.Test;

[DataPreparationFixture]
public class ExampleTextFixture : IDataPreparationLogger, IDataPreparationTestServices, IBeforeTest, IAfterTest
{
    public ILoggerFactory InitializeDataPreparationTestLogger()
    {
        // Initialize the logger factory
        return new NullLoggerFactory();
    }
    public void DataPreparationServices(IServiceCollection serviceCollection)
    {
        // Register your services here
    }

    public void BeforeTest(IServiceProvider testProvider)
    {
        // Setup code before test with DI provider
    }
    public void AfterTest(IServiceProvider testProvider)
    {
        // Cleanup code after test with DI provider
    }
    
    [DataPreparationTest] // Test method marker
    [UsePreparedDataParamsFor(typeof(TestedClass),["params"])] // Use prepared data for the class
    public void TestExample() // Test method
    {
       var sourceFactory = PreparationContext.GetFactory(); // Get the factory
       var serviceProvider = PreparationContext.GetProvider(); // Get the service provider

       object createData = sourceFactory.New<ExampleFactory>(new ObjectParam("params")); // Create new data with params
       object latestData = sourceFactory.Get<ExampleFactory>(out var id); // Get latest data and return id
       IList<object> allData = sourceFactory.Was<ExampleFactory>(); // Get all data
       object? foundData= sourceFactory.GetById(id); // Get data by id
       sourceFactory.Register<ExampleFactory>(new object()); // Register new data
    }
}

public class TestedClass
{
}

[PreparationClassFor(typeof(TestedClass))]
public class TestedClassData
{
    
    [UpData]
    public void Up(string param)
    {
        
    }
    
    [DownData]
    public void Down(string param)
    {
        
    }
}

public class ExampleFactory :IDataFactory<object>
{
    public bool Delete(long createId, object data, IDataParams? args)
    {
       return true;
    }

    public object Create(long createId, IDataParams? args)
    {
        return new object();
    }
}