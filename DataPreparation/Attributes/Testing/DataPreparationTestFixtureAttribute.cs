using System.Reflection;
using System.Runtime.CompilerServices;
using DataPreparation.Analyzers;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Factory.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify the test case for which data preparation is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DataPreparationTestFixtureAttribute : NUnitAttribute, ITestAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPreparationTestFixtureAttribute"/> class.
        /// </summary>
        public DataPreparationTestFixtureAttribute([CallerFilePath]string filePath = "")
        {
            _filePath= filePath;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            // MethodAnalyzer.AnalyzeTestFixture(_filePath, test.Fixture.GetType()); //TODO: Analyze
            // MethodAnalyzer.AnalyzeTestMethod(t);
            
            IServiceCollection baseDataServiceCollection = DataRegister.GetBaseDataServiceCollection(test.Fixture.GetType().Assembly);
            
            if (test.TypeInfo != null)
            {
                var testFixtureInstance = Activator.CreateInstance(test.TypeInfo.Type);
                if (testFixtureInstance == null)
                {
                    throw new Exception("Test case cannot be create, maybe not valid constructor");
                }

                if (testFixtureInstance is IDataPreparationTestServices servicesDataPreparation)
                {
                    servicesDataPreparation.DataPreparationServices(baseDataServiceCollection);
                }

                if (testFixtureInstance is IDataPreparationSetUpConnections setUpConnections)
                {
                  var caseConnections =  setUpConnections.SetUpConnections(); //Todo: Implement
                }

            }

            foreach (var testMethod in test.Tests)
            {
                if (TestStore.RegisterDataCollection((MethodBase)testMethod.Method.MethodInfo,
                        baseDataServiceCollection))
                {
                    Console.Error.WriteLine($"Data preparation for {testMethod.Method.MethodInfo.Name} failed.");
                }
            }
            

        
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            foreach (var testMethod in test.Tests)
            {
                TestStore.DeleteProvider((MethodBase)testMethod.Method.MethodInfo);
                TestStore.DeleteFactory((MethodBase)testMethod.Method.MethodInfo);
            }
        }

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Suite;
        private string _filePath;
    }
}