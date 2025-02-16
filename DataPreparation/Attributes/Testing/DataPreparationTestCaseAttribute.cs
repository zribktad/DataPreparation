using System.Reflection;
using System.Runtime.CompilerServices;
using DataPreparation.Analyzers;
using DataPreparation.Provider;
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
        public DataPreparationTestCaseAttribute([CallerFilePath]string filePath = "")
        {
            _filePath= filePath;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            MethodAnalyzer.AnalyzeTestCase(_filePath, test.Fixture.GetType());
            // MethodAnalyzer.AnalyzeTestMethod(t);
            
            IServiceCollection baseDataServiceCollection = DataRegister.GetBaseDataServiceCollection();

            if (test.TypeInfo != null)
            {
                var testCaseInstance = Activator.CreateInstance(test.TypeInfo.Type);
                if (testCaseInstance == null)
                {
                    throw new Exception("Test case cannot be create, maybe not valid constructor");
                }

                if (testCaseInstance is IDataPreparationCaseServices servicesDataPreparation)
                {
                    servicesDataPreparation.DataPreparationServices(baseDataServiceCollection);
                }

                if (testCaseInstance is IDataPreparationSetUpConnections setUpConnections)
                {
                  var caseConnections =  setUpConnections.SetUpConnections();
                }

            }

            foreach (var testMethod in test.Tests)
            {
                TestStore.RegisterDataCollection((MethodBase)testMethod.Method.MethodInfo, baseDataServiceCollection);
            }
            

        
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
        private string _filePath;
    }
}