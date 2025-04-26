using System.Reflection;
using DataPreparation.Data;
using DataPreparation.Helpers;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing.Factory
{
    /// <summary>
    /// Marks a test method for data preparation, enabling automatic setup and cleanup of test data.
    /// This attribute must be used in conjunction with <see cref="DataPreparationFixtureAttribute"/> 
    /// applied to the containing test class.
    /// </summary>
    /// <remarks>
    /// This attribute performs these key functions:
    /// 1. Initializes a test-specific store for tracking and managing test data
    /// 2. Sets up data before the test executes based on applied preparation attributes
    /// 3. Ensures proper cleanup of test data after the test completes
    /// 
    /// Usage:
    /// <code>
    /// [DataPreparationFixture]
    /// public class MyTestClass
    /// {
    ///     [DataPreparationTest]
    ///     [UsePreparedData(typeof(MyTestDataClass))]
    ///     public void TestWithPreparedData()
    ///     {
    ///         // Test code using prepared data
    ///     }
    /// }
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method,Inherited = false)]
    public class DataPreparationTestAttribute : TestAttribute, ITestAction
    {
        /// <summary>
        /// Stores information about the test being executed
        /// </summary>
        private TestInfo _testInfo = null!;
        
        /// <summary>
        /// Executed before the test method runs. Initializes test-specific data preparation.
        /// </summary>
        /// <param name="test">Information about the test that is going to be executed</param>
        /// <remarks>
        /// This method:
        /// 1. Creates a TestInfo object to track metadata about the current test
        /// 2. Initializes a TestStore for managing data specific to this test
        /// 3. Triggers data preparation via TestStore.Initialize which executes any
        ///    data setup methods specified by attributes like UsePreparedData
        /// </remarks>
        public void BeforeTest(ITest test)
        {
            _testInfo = TestInfo.CreateTestInfo(test);
            TestStore.Initialize(_testInfo);
        }

        /// <summary>
        /// Executed after the test method completes. Cleans up test-specific data.
        /// </summary>
        /// <param name="test">Information about the test that has been executed</param>
        /// <remarks>
        /// This method:
        /// 1. Retrieves the TestStore for the current test
        /// 2. Calls TestStore.Deinitialize which executes cleanup operations
        ///    including running any "down" methods for test data that was set up
        /// 3. Ensures all resources created during the test are properly disposed
        /// </remarks>
        public void AfterTest(ITest test)
        {
            var testStore = TestStore.Get(_testInfo);
            TestStore.Deinitialize(testStore);
        }

        /// <summary>
        /// Specifies that this action applies at the individual test method level.
        /// </summary>
        /// <remarks>
        /// ActionTargets.Test indicates that BeforeTest is called before each test method executes,
        /// and AfterTest is called after each test method completes.
        /// </remarks>
        public ActionTargets Targets => ActionTargets.Test;
    }
}
