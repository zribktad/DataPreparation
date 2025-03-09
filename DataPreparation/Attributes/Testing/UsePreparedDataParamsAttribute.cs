using System.Diagnostics.CodeAnalysis;
using DataPreparation.DataHandlers;
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
            TestStore testStore = TestStore.CreateTestStore(testInfo);
            // Prepare data for the test from attribute
            var preparedDataClassInstanceInList = GetDataPreparation.GetPreparedData(testStore, [_preparaDataType]);
            // Add the prepared data to the store
            if(preparedDataClassInstanceInList.Count == 0) return;
            testStore.PreparedData.AddDataPreparation(preparedDataClassInstanceInList[0],_paramsUpData,_paramsDownData);           
            // Up data for the test if all data are prepared
            DataPreparationHandler.DataUp(testStore);
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
            DataPreparationHandler.DataDown(testStore);
            TestStore.RemoveTestStore(testStore);
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
