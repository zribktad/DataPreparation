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
    public class DataPreparationFixtureAttribute : TestFixtureAttribute, ITestAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPreparationFixtureAttribute"/> class.
        /// </summary>
        public DataPreparationFixtureAttribute([CallerFilePath]string filePath = "")
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
            if (test.TypeInfo == null)
            {
                throw new Exception("Test Fixture type not found before test");
            }

            var fixtureType = test.TypeInfo.Type;
            if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationLoggerInitializer)))
            {
                var builder = fixtureType
                    .GetMethod(nameof(IDataPreparationLoggerInitializer.InitializeDataPreparationTestLogger))
                    ?.Invoke(null, null);
                if (builder is ILoggerFactory loggerFactory)
                {
                    ILogger logger = loggerFactory.CreateLogger<ISourceFactory>();
                    FixtureStore.RegisterLoggerFactory(fixtureType, loggerFactory);
                }
            }

            IServiceCollection baseDataServiceCollection =
                DataRegister.GetBaseDataServiceCollection(fixtureType.Assembly);


            if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationTestServices)))
            {
                test.TypeInfo.Type.GetMethod(nameof(IDataPreparationTestServices.DataPreparationServices))
                    ?.Invoke(null, [baseDataServiceCollection]);
            }

            if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationSetUpConnections)))
            {
                test.TypeInfo.Type.GetMethod(nameof(IDataPreparationSetUpConnections.SetUpConnections))
                    ?.Invoke(null, null);
            }

            FixtureStore.RegisterService(fixtureType, baseDataServiceCollection);
            
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            
            if (test.TypeInfo == null)
            {
                throw new Exception("Test Fixture type not found after test");
            }

            var fixtureType = test.TypeInfo.Type;
            
            FixtureStore.RemoveService(fixtureType);
        }

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Suite;
        private string _filePath;
    }
}