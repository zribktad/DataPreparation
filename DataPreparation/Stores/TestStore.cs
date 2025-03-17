using DataPreparation.Data;
using DataPreparation.Exceptions;
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
    
    internal static TestStore Initialize(TestInfo testInfo)
    {
        var testStore = Get(testInfo);
        if(testStore != null)  return testStore;
      
        var loggerFactory = LoggerHelper.CreateOrNullLogger(testInfo.FixtureInfo);

        var dataPreparationAttributes = AttributeHelper.GetAttributes(testInfo.Method.MethodInfo, 
            typeof(UsePreparedAttribute));
        testStore = Create(testInfo,loggerFactory,dataPreparationAttributes);
        
        if(testInfo.FixtureInfo.Instance is IBeforeTest beforeTest)
        {
            beforeTest.BeforeTest(testStore.ServiceProvider);
        }
        
        
        return testStore;
    }
    
    internal static TestStore? Deinitialize(TestStore? testStore)
    {
        if (testStore != null)
        {
            //TODO: logging
            ExceptionAggregator? exceptionAggregator = new();
            try
            {
                testStore.SourceFactory.Dispose();
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }

            try
            {
                DataPreparationHandler.DataDown(testStore);
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }
            
            if(testStore.TestInfo.FixtureInfo.Instance is IAfterTest beforeTest)
            {
                try
                {
                    beforeTest.AfterTest(testStore.ServiceProvider);
                }
                catch (Exception e)
                {
                    exceptionAggregator.Add(e);
                }
               
            }
            try
            {
                if (Store.GetFixtureStore(testStore.TestInfo.FixtureInfo) is not { } fixtureStore)
                {
                    throw new InvalidOperationException(
                        $"No {typeof(DataPreparationFixtureAttribute)} found for {testStore.TestInfo.FixtureInfo}.");
                }
            
                testStore = fixtureStore.RemoveTestStore(testStore.TestInfo);
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }
            
            var aggregatedEx =  exceptionAggregator.Get();
            if (aggregatedEx != null)
            {
                throw aggregatedEx;
            }
        }
        return testStore;
    }
    internal static TestStore Get(ContextTestInfo testInfo)
    {
        foreach (var fixtureStore in  Store.GetFixtureStores())
        {
            var testStore = fixtureStore.GetTestStore(testInfo);
            if (testStore !=  null)
            {
                return testStore;
            }
        }

        throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for {testInfo}.");
    }
    
       #region Test Store
        private static TestStore Create(TestInfo testContextTestInfo, ILoggerFactory loggerFactory,IList<Attribute> dataPreparationAttributes)
        {
            var testLogger = loggerFactory.CreateLogger(typeof(Store));
            testLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

            if (Store.GetFixtureStore(testContextTestInfo.FixtureInfo) is { } fixtureStore)
            {
                var fixtureLogger = fixtureStore.LoggerFactory.CreateLogger(nameof(Store));
                fixtureLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

                if (!fixtureStore.CreateTestStore(testContextTestInfo, loggerFactory,dataPreparationAttributes))
                {
                    LoggerHelper.Log(logger => logger.LogTrace("Test data initialization for {0} already exists", testContextTestInfo),
                        fixtureLogger,testLogger);
                }
            }
            else
            {
                testLogger.LogError("No {0} found for {1}.", typeof(DataPreparationFixtureAttribute), testContextTestInfo.FixtureInfo);
                throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for { testContextTestInfo.FixtureInfo}.");
            }
            LoggerHelper.Log(logger => logger.LogDebug("Test data initialization for {0} created", testContextTestInfo), 
                fixtureStore.LoggerFactory.CreateLogger(typeof(Store)),testLogger);
  
            return Get(testContextTestInfo)!;
        }

      
        private static TestStore? Get(TestInfo testInfo)
        {
            var store =  Store.GetFixtureStore(testInfo.FixtureInfo).GetTestStore(testInfo);
            return store;
        }

        #endregion
    
    
   
}

