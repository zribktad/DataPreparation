using DataPreparation.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify the test case for which data preparation is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DataPreparationTestCaseAttribute : NUnitAttribute, ITestAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPreparationTestCaseAttribute"/> class.
        /// </summary>
        public DataPreparationTestCaseAttribute()
        {
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            IServiceCollection serviceCollection = DataRegister.GetBaseDataServiceCollection();

            if (test.TypeInfo != null)
            {
                var testCaseInstance = Activator.CreateInstance(test.TypeInfo.Type);
                if (testCaseInstance == null)
                {
                    throw new Exception("Test case cannot be create, maybe not valid constructor");
                }

                if (testCaseInstance is IDataPreparationCaseServices servicesDataPreparation)
                {
                    servicesDataPreparation.DataPreparationServices(serviceCollection);
                }

                if (testCaseInstance is IDataPreparationSetUpConnections setUpConnections)
                {
                  var caseConnections =  setUpConnections.SetUpConnections();
                }

            }

            CaseProviderStore.RegisterDataCollection(test.Fixture.GetType(), serviceCollection);

            TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture.GetType());
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
        }

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Suite;
    }
}