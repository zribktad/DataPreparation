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

/// <summary>
/// Manages test-specific data and services for a single test method execution.
/// Acts as a container for all resources needed during test execution and coordinates 
/// their initialization and cleanup.
/// </summary>
/// <remarks>
/// TestStore is created for each test method and contains:
/// - Test metadata (TestInfo)
/// - Service provider for dependency injection
/// - Source factory for data creation
/// - Logger factory for test-specific logging
/// - Prepared data store for test data lifecycle management
/// </remarks>
internal class TestStore
{
    /// <summary>Information about the test being executed</summary>
    public TestInfo TestInfo { get; }
    
    /// <summary>Service provider for dependency injection within the test</summary>
    public IServiceProvider ServiceProvider { get; }
    
    /// <summary>Service scope that will be disposed after test completes</summary>
    private IServiceScope ServiceScope;
    
    /// <summary>Factory for creating test data sources</summary>
    public ISourceFactory SourceFactory { get; }
    
    /// <summary>Factory for creating loggers specific to this test</summary>
    public ILoggerFactory LoggerFactory { get; }
    
    /// <summary>Tracks usage of data preparation attributes in this test</summary>
    public AttributeUsingCounter AttributeUsingCounter { get; } 
    
    /// <summary>Contains all data preparation objects for this test</summary>
    public DataPreparationTestStores PreparedData { get; } 
    
    /// <summary>
    /// Creates a new TestStore for a specific test method.
    /// </summary>
    /// <param name="testInfo">Information about the test being executed</param>
    /// <param name="loggerFactory">Logger factory for creating test-specific loggers</param>
    /// <param name="serviceScope">Service scope for dependency injection</param>
    /// <param name="dataPreparationAttributes">List of data preparation attributes applied to the test method</param>
    internal TestStore(TestInfo testInfo, ILoggerFactory loggerFactory, IServiceScope serviceScope, IList<Attribute> dataPreparationAttributes)
    {
        TestInfo = testInfo;
        LoggerFactory = loggerFactory;
        PreparedData = new(loggerFactory);
        AttributeUsingCounter = new(dataPreparationAttributes);
        ServiceScope = serviceScope;
        ServiceProvider = serviceScope.ServiceProvider;
        SourceFactory = new SourceFactory(ServiceProvider, LoggerFactory.CreateLogger<ISourceFactory>());
    }
    
    /// <summary>
    /// Returns a string representation of this test store.
    /// </summary>
    public override string ToString()
    {
        return "Store for " + TestInfo;
    }
    
    /// <summary>
    /// Initializes a TestStore for the specified test.
    /// If a TestStore already exists for this test, returns it. Otherwise, creates a new one.
    /// </summary>
    /// <param name="testInfo">Information about the test requiring a TestStore</param>
    /// <returns>An initialized TestStore instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when no test fixture is found for this test</exception>
    internal static TestStore Initialize(TestInfo testInfo)
    {
        // Return existing test store if already initialized
        var testStore = Get(testInfo);
        if(testStore != null) return testStore;
      
        // Create logger factory for this test
        var loggerFactory = LoggerHelper.CreateOrNullLogger(testInfo.FixtureInfo);

        // Get data preparation attributes applied to the test method
        var dataPreparationAttributes = AttributeHelper.GetAttributes(testInfo.Method.MethodInfo, 
            typeof(UsePreparedAttribute));
        
        // Create a new test store
        testStore = Create(testInfo, loggerFactory, dataPreparationAttributes);
        
        // Execute beforeTest hook if implemented by the fixture
        if(testInfo.FixtureInfo.Instance is IBeforeTest beforeTest)
        {
            beforeTest.BeforeTest(testStore.ServiceProvider);
        }
        
        return testStore;
    }
    
    /// <summary>
    /// Cleans up and deinitializes a TestStore after test execution.
    /// This handles cleanup of all data and resources created during test execution.
    /// </summary>
    /// <param name="testStore">The TestStore to deinitialize (can be null)</param>
    /// <returns>The deinitialized TestStore (usually null)</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture store is found for this test</exception>
    /// <exception cref="AggregateException">Thrown when errors occur during cleanup</exception>
    internal static TestStore? Deinitialize(TestStore? testStore)
    {
        if (testStore != null)
        {
            // Create exception aggregator to collect all cleanup exceptions
            ExceptionAggregator? exceptionAggregator = new();
            
            try
            {
                // Dispose the source factory asynchronously
                testStore.SourceFactory.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }

            try
            {
                // Run data cleanup operations (DataDown)
                DataPreparationHandler.DataDown(testStore).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }
            
            // Execute afterTest hook if implemented by fixture
            if(testStore.TestInfo.FixtureInfo.Instance is IAfterTest afterTest)
            {
                try
                {
                    afterTest.AfterTest(testStore.ServiceProvider);
                }
                catch (Exception e)
                {
                    exceptionAggregator.Add(e);
                }
            }
            
            try
            {
                // Get the fixture store and remove this test store from it
                if (Store.GetFixtureStore(testStore.TestInfo.FixtureInfo) is not { } fixtureStore)
                {
                    throw new InvalidOperationException(
                        $"No {typeof(DataPreparationFixtureAttribute)} found for {testStore.TestInfo.FixtureInfo}.");
                }
            
                // Remove the test store and dispose its service scope
                testStore = fixtureStore.RemoveTestStore(testStore.TestInfo);
                testStore?.ServiceScope.Dispose();
            }
            catch (AggregateException e)
            {
                exceptionAggregator.Add(e);
            }
            
            // If any exceptions occurred during cleanup, throw an aggregated exception
            var aggregatedEx = exceptionAggregator.Get();
            if (aggregatedEx != null)
            {
                throw aggregatedEx;
            }
        }
        return testStore;
    }
    
    /// <summary>
    /// Gets an existing TestStore for the specified context test info.
    /// Searches all fixture stores for a matching test store.
    /// </summary>
    /// <param name="testInfo">The context test info to look up</param>
    /// <returns>The matching TestStore or null if not found</returns>
    internal static TestStore? Get(ContextTestInfo testInfo)
    {
        // Search all fixture stores for a matching test store
        foreach (var fixtureStore in Store.GetFixtureStores())
        {
            var testStore = fixtureStore.GetTestStore(testInfo);
            if (testStore != null)
            {
                return testStore;
            }
        }
        return null;
    }
    
    #region Test Store Creation
    
    /// <summary>
    /// Creates a new TestStore for the specified test context.
    /// </summary>
    /// <param name="testContextTestInfo">Information about the test context</param>
    /// <param name="loggerFactory">Logger factory for creating test-specific loggers</param>
    /// <param name="dataPreparationAttributes">List of data preparation attributes applied to the test method</param>
    /// <returns>The newly created TestStore</returns>
    /// <exception cref="InvalidOperationException">Thrown when no fixture store is found for this test</exception>
    private static TestStore Create(TestInfo testContextTestInfo, ILoggerFactory loggerFactory, IList<Attribute> dataPreparationAttributes)
    {
        // Create logger for this operation
        var testLogger = loggerFactory.CreateLogger(typeof(Store));
        testLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

        // Get the fixture store for this test
        if (Store.GetFixtureStore(testContextTestInfo.FixtureInfo) is { } fixtureStore)
        {
            var fixtureLogger = fixtureStore.LoggerFactory.CreateLogger(nameof(Store));
            fixtureLogger.LogTrace("Test data initialization for {0} started", testContextTestInfo);

            // Create test store in the fixture store, handling the case if it already exists
            if (!fixtureStore.CreateTestStore(testContextTestInfo, loggerFactory, dataPreparationAttributes))
            {
                LoggerHelper.Log(logger => logger.LogTrace("Test data initialization for {0} already exists", testContextTestInfo),
                    fixtureLogger, testLogger);
            }
        }
        else
        {
            // If no fixture store exists, log error and throw exception
            testLogger.LogError("No {0} found for {1}.", typeof(DataPreparationFixtureAttribute), testContextTestInfo.FixtureInfo);
            throw new InvalidOperationException($"No {typeof(DataPreparationFixtureAttribute)} found for {testContextTestInfo.FixtureInfo}.");
        }
        
        // Log successful creation
        LoggerHelper.Log(logger => logger.LogDebug("Test data initialization for {0} created", testContextTestInfo), 
            fixtureStore.LoggerFactory.CreateLogger(typeof(Store)), testLogger);

        // Return the created test store
        return Get(testContextTestInfo)!;
    }

    /// <summary>
    /// Gets a TestStore for a specific test from its fixture store.
    /// </summary>
    /// <param name="testInfo">Information about the test</param>
    /// <returns>The TestStore for the specified test, or null if not found</returns>
    private static TestStore? Get(TestInfo testInfo)
    {
        var store = Store.GetFixtureStore(testInfo.FixtureInfo).GetTestStore(testInfo);
        return store;
    }

    #endregion
}

