using DataPreparation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Marks a test fixture (class) for integration with the DataPreparation framework.
    /// This attribute initializes the data preparation infrastructure for all tests in the fixture.
    /// </summary>
    /// <remarks>
    /// The attribute performs these key functions:
    /// 1. Sets up a fixture-level store for maintaining test data
    /// 2. Configures logging for the test fixture
    /// 3. Initializes dependency injection services
    /// 4. Handles cleanup after all tests in the fixture complete
    /// 
    /// Usage:
    /// <code>
    /// [DataPreparationFixture]
    /// public class MyTestClass
    /// {
    ///     [Test]
    ///     public void TestWithDataPreparation()
    ///     {
    ///         // Test code using data preparation
    ///     }
    /// }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DataPreparationFixtureAttribute : TestFixtureAttribute, ITestAction
    {
        // Commented code kept for reference
        // /// <summary>
        // /// Initializes a new instance of the <see cref="DataPreparationFixtureAttribute"/> class.
        // /// </summary>
        // public DataPreparationFixtureAttribute([CallerFilePath]string filePath = "")
        // {
        //     _filePath= filePath;
        // }

        /// <summary>
        /// Executed before any tests in the fixture run. Initializes the data preparation infrastructure.
        /// </summary>
        /// <param name="test">Information about the test fixture that is about to be run</param>
        /// <remarks>
        /// This method performs these initialization steps:
        /// 1. Creates a FixtureInfo object to track information about the fixture
        /// 2. Sets up appropriate logging based on fixture configuration
        /// 3. Initializes the base service collection for dependency injection
        /// 4. Allows custom service registration if the fixture implements IDataPreparationTestServices
        /// 5. Creates a FixtureStore to manage test data throughout the fixture's lifetime
        /// </remarks>
        public void BeforeTest(ITest test)
        {
            // Create fixture info to track metadata about this test fixture
            var fixtureInfo = new FixtureInfo(test, test.Fixture);
         
            // Set up logging for this fixture (falls back to NullLogger if not configured)
            var loggerFactory = LoggerHelper.CreateOrNullLogger(fixtureInfo);
            var logger = loggerFactory.CreateLogger(fixtureInfo.Type);
            
            logger.LogDebug("Data Preparation for {0} started", fixtureInfo.Type);
            
            // Initialize the base service collection from the assembly containing the fixture
            IServiceCollection baseDataServiceCollection = new DataRegister(loggerFactory, fixtureInfo.Type.Assembly)
                .GetBaseDataServiceCollection();
            
            // If the fixture implements IDataPreparationTestServices, allow it to register additional services
            if (fixtureInfo.Instance is IDataPreparationTestServices dataPreparationTestServices)
            {
                try
                {
                    dataPreparationTestServices.DataPreparationServices(baseDataServiceCollection);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Data Preparation Services failed to register");
                    throw;
                }
            }

            // Commented code kept for reference
            // if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationSetUpConnections)))
            // {
            //     test.TypeInfo.Type.GetMethod(nameof(IDataPreparationSetUpConnections.SetUpConnections))
            //         ?.Invoke(null, null);
            // }

            // Create a fixture store to hold data for this test fixture
            Store.CreateFixtureStore(fixtureInfo, loggerFactory, baseDataServiceCollection.BuildServiceProvider());
        }

        /// <summary>
        /// Executed after all tests in the fixture have completed. Cleans up the data preparation resources.
        /// </summary>
        /// <param name="test">Information about the test fixture that has just completed</param>
        /// <remarks>
        /// This method cleans up by removing the fixture store that was created for this test fixture,
        /// ensuring proper disposal of all resources including database connections, cached objects, etc.
        /// </remarks>
        public void AfterTest(ITest test)
        {
            if (test.TypeInfo == null)
            {
                throw new Exception("Test Fixture type not found after test");
            }
            
            // Remove and dispose the fixture store for this test fixture
            Store.RemoveFixtureStore(new(test, test.Fixture));
        }

        /// <summary>
        /// Specifies that this action applies at the test fixture (suite) level.
        /// </summary>
        /// <remarks>
        /// ActionTargets.Suite indicates that BeforeTest is called once before all tests in the fixture,
        /// and AfterTest is called once after all tests in the fixture have completed.
        /// </remarks>
        public ActionTargets Targets => ActionTargets.Suite;
    }
}