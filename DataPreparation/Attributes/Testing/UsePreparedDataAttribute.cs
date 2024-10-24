using DataPreparation.Data;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataAttribute : NUnitAttribute, ITestAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsePreparedDataAttribute"/> class.
        /// </summary>
        /// <param name="dataProviders">The types of data providers to use for preparing data.</param>
        public UsePreparedDataAttribute(params Type[] dataProviders)
        {
            _dataProviders = dataProviders;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            DataPreparation.TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture.GetType());
            _preparedDataList = PrepareDataList(test, _dataProviders);
            TestDataPreparationStore.AddDataPreparation(test.Method.MethodInfo, _preparedDataList);
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            // Down data for the test
            TestDataHandler.DataDown(test.Method.MethodInfo);
        }

        /// <summary>
        /// Prepares the data list for the test.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        /// <param name="dataProviders">The types of data providers to use for preparing data.</param>
        /// <returns>A list of prepared data.</returns>
        /// <exception cref="Exception">Thrown when prepared data for a data provider is not found.</exception>
        private List<IDataPreparation> PrepareDataList(ITest test, Type[] dataProviders)
        {
            List<IDataPreparation> preparedDataList = new();
            foreach (var dataPreparationType in dataProviders)
            {
                var preparedData = CaseProviderStore.GetTestCaseServiceData(test, dataPreparationType);
                if (preparedData == null)
                {
                    throw new Exception($"Prepared data for {dataPreparationType.FullName} not found");
                }
                preparedDataList.Add(preparedData);
            }
            return preparedDataList;
        }

        private List<IDataPreparation> _preparedDataList = new();
        private readonly Type[] _dataProviders;

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;
    }
}
