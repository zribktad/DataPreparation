using System.Diagnostics.CodeAnalysis;
using DataPreparation.DataHandling;
using DataPreparation.Models;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UsePreparedDataParamsAttribute : NUnitAttribute, ITestAction
    {
       
        public UsePreparedDataParamsAttribute(Type dataProviders,  [NotNull] object[] paramsUpData, [NotNull]object[] paramsDownData)
        {
            _dataProviders = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
            _paramsDownData = paramsDownData ?? throw new ArgumentNullException(nameof(paramsDownData));
         
        }
        
        public UsePreparedDataParamsAttribute(Type dataProviders,  object[] paramsUpData)
        {
            _dataProviders = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
        }
        public UsePreparedDataParamsAttribute(Type dataProviders)
        {
            _dataProviders = dataProviders ?? throw new ArgumentNullException(nameof(dataProviders));
        }
      

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public void BeforeTest(ITest test)
        {
            //set the service provider for static global access
            TestData.ServiceProvider = CaseProviderStore.GetRegistered(test.Fixture!.GetType());
            
            // Prepare data for the test from attribute
            var preparedDataClassInstance = GetDataPreparation.PrepareData(test, _dataProviders);
            var preparedData = new PreparedData(preparedDataClassInstance,_paramsUpData,_paramsDownData);
            // Add the prepared data to the store
            TestDataPreparationStore.AddDataPreparation(test.Method!.MethodInfo, preparedData);
            // Up data for the test if all data are prepared
            TestDataHandler.DataUp(test.Method.MethodInfo);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public void AfterTest(ITest test)
        {
            // Down data for the test
            TestDataHandler.DataDown(test.Method!.MethodInfo);
        }


        
        private readonly Type _dataProviders;
        private readonly object[]? _paramsUpData;
        private readonly object[]? _paramsDownData;

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public ActionTargets Targets => ActionTargets.Test;
    }
}
