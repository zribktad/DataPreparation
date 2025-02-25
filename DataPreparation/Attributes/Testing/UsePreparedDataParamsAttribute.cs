using System.Diagnostics.CodeAnalysis;
using DataPreparation.DataHandling;
using DataPreparation.Models;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = true, Inherited = false)]
    public class UsePreparedDataParamsAttribute :UsePreparedAttribute
    {
       
        public UsePreparedDataParamsAttribute(Type preparedDataType,  [NotNull] object[] paramsUpData, [NotNull]object[] paramsDownData)
        {
            _preparaDataType = preparedDataType ?? throw new ArgumentNullException(nameof(preparedDataType));
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
            _paramsDownData = paramsDownData ?? throw new ArgumentNullException(nameof(paramsDownData));
         
        }
        
        public UsePreparedDataParamsAttribute(Type preparedDataType,  object[] paramsUpData):this(preparedDataType,paramsUpData,[])
        {
        }
        public UsePreparedDataParamsAttribute(Type preparedDataType):this(preparedDataType,[],[])
        {
        }
       
      

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            TestStore testStore = PreparationTest.CreateTestStore(testInfo);
            // Prepare data for the test from attribute
            var preparedDataClassInstance = GetDataPreparation.GetPreparedData(testStore, [_preparaDataType]);
            var preparedData = new PreparedData(preparedDataClassInstance,_paramsUpData,_paramsDownData);
            // Add the prepared data to the store
            testStore.PreparedData.AddDataPreparation(preparedData);           
            // Up data for the test if all data are prepared
            TestDataHandler.DataUp(testStore);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public override void AfterTest(ITest test)
        {
            // Down data for the test
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = Store.GetTestStore(testInfo);
            TestDataHandler.DataDown(testStore);
            PreparationTest.RemoveTestStore(testStore);
        }


        
        private readonly Type _preparaDataType;
        private readonly object[]? _paramsUpData;
        private readonly object[]? _paramsDownData;

        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
    }
}
