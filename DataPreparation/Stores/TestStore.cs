using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DataPreparation.Models.Data;

public class TestStore
{
    public TestInfo TestInfo { get; }
    public IServiceProvider ServiceProvider { get; }
    public ISourceFactory SourceFactory { get; }
    public ILoggerFactory LoggerFactory { get; }
    public AttributeUsingCounter AttributeUsingCounter { get; } 
    public DataPreparationTestStores PreparedData { get;} 
    internal TestStore(TestInfo testInfo, ILoggerFactory loggerFactory, IServiceCollection serviceCollection, IList<Attribute> dataPreparationAttributes)
    {
        TestInfo = testInfo;
        LoggerFactory = loggerFactory;
        PreparedData = new(loggerFactory);
        AttributeUsingCounter = new( dataPreparationAttributes);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        SourceFactory = new SourceFactory(ServiceProvider,LoggerFactory.CreateLogger<ISourceFactory>());
    }
    
    public override string ToString()
    {
        return "Store for " + TestInfo;
    }
    
    public static TestStore CreateTestStore(TestInfo testInfo)
    {
        var testStore = Store.GetTestStore(testInfo);
        if(testStore != null)  return testStore;
      
        var loggerFactory = LoggerHelper.CreateOrNullLogger(testInfo.FixtureInfo.Type);

        var dataPreparationAttributes = AttributeHelper.GetAttributes(testInfo.Method.MethodInfo, 
            typeof(UsePreparedAttribute));
        
        return Store.CreateTestStore(testInfo,loggerFactory,dataPreparationAttributes);
    }
    
    public static TestStore? RemoveTestStore(TestStore? testStore)
    {
        if (testStore != null)
        {
            testStore.SourceFactory.Dispose();
            return Store.RemoveTestStore(testStore.TestInfo);
        }
        return testStore;
    }
    
    
   
}

