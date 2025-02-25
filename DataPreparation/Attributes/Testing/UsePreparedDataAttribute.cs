using DataPreparation.Data;
using DataPreparation.DataHandling;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute : UsePreparedAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataAttribute"/> class.
        /// </summary>
        /// <param name="dataProviders">The types of data providers to use for preparing data.</param>
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            _dataProviders = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            // Prepare data for the test from attribute
            var preparedDataList = GetDataPreparation.PrepareDataList(test, _dataProviders);
            // Add the prepared data to the store
            TestDataPreparationStore.AddDataPreparation(test.Method.MethodInfo, preparedDataList);
            // Up data for the test if all data are prepared
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public override void AfterTest(ITest test)
        {
            // Down data for the test
            TestDataHandler.DataDown(test.Method.MethodInfo);
        }


        
        private readonly Type[] _dataProviders;

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
    }
}
