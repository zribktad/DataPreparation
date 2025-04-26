using DataPreparation.Data;
using DataPreparation.DataHandlers;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Connects a test method with one or more data preparation classes that will be automatically executed
    /// before the test runs and cleaned up after the test completes.
    /// </summary>
    /// <remarks>
    /// This attribute enables a test to declare what test data it needs, which will be automatically:
    /// 1. Created before the test runs using methods marked with [UpData]
    /// 2. Made available to the test during execution
    /// 3. Cleaned up after the test completes using methods marked with [DownData]
    /// 
    /// This creates a clean separation between test logic and data setup/teardown logic.
    /// 
    /// Example usage:
    /// <code>
    /// [DataPreparationTest]
    /// [UsePreparedData(typeof(CustomerTestData), typeof(OrderTestData))]
    /// public void Order_WhenCustomerHasCredit_CanBeProcessed()
    /// {
    ///     // Test code that uses the prepared customer and order data
    /// }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute : UsePreparedAttribute
    {
        /// <summary>
        /// The types of data preparation classes that will be executed for this test
        /// </summary>
        private readonly Type[] _preparedDataTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataAttribute"/> class.
        /// </summary>
        /// <param name="dataProviders">The types of data preparation classes to use for setting up test data</param>
        /// <remarks>
        /// Each type provided should contain methods marked with [UpData] and [DownData] or with interaface which implement it.
        /// The framework will automatically:
        /// - Create instances of each provided type
        /// - Execute methods marked with [UpData] before the test runs
        /// - Execute methods marked with [DownData] after the test completes
        /// - Pass data returned from UpData methods to matching DownData methods
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if dataProviders is null</exception>
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            _preparedDataTypes = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
        }

        /// <summary>
        /// Method called before the test is executed to prepare the required test data.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        /// <remarks>
        /// This method:
        /// 1. Creates test info and initializes a test store if it doesn't exist
        /// 2. Gets prepared data instances for all specified data provider types
        /// 3. Adds the prepared data to the test store
        /// 4. Executes all [UpData] methods in the preparation classes
        /// </remarks>
        public override void BeforeTest(ITest test)
        {
            // Create or get the test store for the current test
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = TestStore.Initialize(testInfo);
            
            // Prepare data for the test from the data types specified in the attribute
            var preparedDataList = GetDataPreparation.GetPreparedData(testStore, _preparedDataTypes);
            
            // Add the prepared data to the store for execution and later cleanup
            testStore.PreparedData.AddDataPreparation(preparedDataList);
            
            // Execute the UpData methods in all prepared data classes
            DataPreparationHandler.DataUp(testStore);
        }

        /// <summary>
        /// Method called after the test is executed to clean up the prepared test data.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        /// <remarks>
        /// This method:
        /// 1. Gets the test store associated with the completed test
        /// 2. Deinitializes the test store, which includes:
        ///    - Executing all [DownData] methods in reverse order
        ///    - Disposing any resources associated with the test
        ///    - Cleaning up the test store itself
        /// </remarks>
        public override void AfterTest(ITest test)
        {
            // Get the test store and clean up all test data
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = TestStore.Get(testInfo);
            TestStore.Deinitialize(testStore);
        }

        /// <summary>
        /// Specifies that this action applies at the individual test method level.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
    }
}
