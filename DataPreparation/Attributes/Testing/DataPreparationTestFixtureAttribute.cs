using System.Reflection;
using System.Runtime.CompilerServices;
using DataPreparation.Analyzers;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Factory.Testing;
using DataPreparation.Testing.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            if (test.TypeInfo != null)
            {
                if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationLoggerInitializer)))
                {
                    var builder =  test.TypeInfo.Type.GetMethod(nameof(IDataPreparationLoggerInitializer.InitializeDataPreparationTestLogger) )?.Invoke(null, null);
                    if(builder is ILoggerFactory loggerFactory)
                    {
                        ILogger logger = loggerFactory.CreateLogger<object>();
                        logger.LogInformation("Data Preparation Test Started***********************************");
                    }
                }
                
                IServiceCollection baseDataServiceCollection =
                    DataRegister.GetBaseDataServiceCollection(test.Fixture.GetType().Assembly);
                

                if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationTestServices)))
                {
                    test.TypeInfo.Type.GetMethod(nameof(IDataPreparationTestServices.DataPreparationServices))?.Invoke(null, [baseDataServiceCollection]);
                }
                
                if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationSetUpConnections)))
                {
                    test.TypeInfo.Type.GetMethod(nameof(IDataPreparationSetUpConnections.SetUpConnections))?.Invoke(null, null);
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
            else
            {
                throw new Exception("Test Fixture type not found");
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
                TestStore.DeleteProvider(testMethod.Method.MethodInfo);
                TestStore.DeleteFactory(testMethod.Method.MethodInfo);
            }
        }

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Suite;
        private string _filePath;
    }
}