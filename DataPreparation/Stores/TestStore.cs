using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
using DataPreparation.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataPreparation.Models.Data;

/// <summary>
/// Manages test-specific data and services for a single test method execution.
/// Acts as a pure data container for all resources needed during test execution.
/// </summary>
/// <remarks>
/// TestStore is created for each test method and contains:
/// - Test metadata (TestInfo)
/// - Service provider for dependency injection
/// - Source factory for data creation
/// - Logger factory for test-specific logging
/// - Prepared data store for test data lifecycle management
///
/// Lifecycle management (Initialize/Deinitialize) is handled by <see cref="TestStoreLifecycleManager"/>.
/// </remarks>
internal class TestStore
{
    /// <summary>Information about the test being executed</summary>
    public TestInfo TestInfo { get; }
    
    /// <summary>Service provider for dependency injection within the test</summary>
    public IServiceProvider ServiceProvider { get; }
    
    /// <summary>Service scope that will be disposed after test completes</summary>
    internal IServiceScope ServiceScope { get; }
    
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
    public override string ToString() => "Store for " + TestInfo;
}
