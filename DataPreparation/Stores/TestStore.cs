using DataPreparation.Factory.Testing;
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
    public ILoggerFactory? LoggerFactory { get; }
    public AttributeUsing AttributeUsing { get; } 
    public DataPreparationTestStores PreparedData { get;} = new();
    internal TestStore(TestInfo testInfo, ILoggerFactory loggerFactory, IServiceCollection serviceCollection, IList<Attribute> dataPreparationAttributes)
    {
        TestInfo = testInfo;
        LoggerFactory = loggerFactory;
        AttributeUsing = new( dataPreparationAttributes);
        ServiceProvider = serviceCollection.BuildServiceProvider();
        SourceFactory = new SourceFactory(ServiceProvider,LoggerFactory.CreateLogger<ISourceFactory>());
    }
    
    public override string ToString()
    {
        return "Store for " + TestInfo;
    }
    
    
   
}

