using System.Reflection;
using System.Runtime.CompilerServices;
using DataPreparation.Analyzers;
using DataPreparation.Factory.Testing;
using DataPreparation.Provider;
using DataPreparation.Testing;
using DataPreparation.Factory.Testing;
using DataPreparation.Helpers;
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
         
            //Get logger if not found NullLogger
            var loggerFactory = LoggerHelper.CreateOrNullLogger(fixtureType);
            var logger = loggerFactory.CreateLogger(fixtureType);
            
            logger.LogDebug("Data Preparation for {0} started", fixtureType.Name);
            //Get copy of base data service collection
            IServiceCollection baseDataServiceCollection = new DataRegister(loggerFactory).GetBaseDataServiceCollection(fixtureType.Assembly);
            
            if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationTestServices)))
            {
                try
                {
                    test.TypeInfo.Type.GetMethod(nameof(IDataPreparationTestServices.DataPreparationServices))
                        ?.Invoke(null, [baseDataServiceCollection]);
                }
                catch (Exception e)
                {
                    logger.LogError(e,"Data Preparation Services failed to register");
                    throw;
                }
              
            }

            if (test.TypeInfo.Type.IsAssignableTo(typeof(IDataPreparationSetUpConnections)))
            {
                test.TypeInfo.Type.GetMethod(nameof(IDataPreparationSetUpConnections.SetUpConnections))
                    ?.Invoke(null, null);
            }

            //create fixture store
            Store.CreateFixtureStore(new(test), loggerFactory,baseDataServiceCollection);
            
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
            
            Store.RemoveFixtureStore(new(test));
            
        }

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Suite;
        private string _filePath;
    }
}