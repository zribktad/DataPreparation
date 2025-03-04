using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DataPreparation.Data;
using DataPreparation.DataHandlers;
using DataPreparation.Models.Data;
using DataPreparation.Provider;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace DataPreparation.Testing
{
    /// <summary>
    /// Attribute to specify that prepared data should be used for the test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsePreparedDataParamsForAttribute : UsePreparedAttribute
    {
       
        public UsePreparedDataParamsForAttribute(Type classType,  string methodName,[NotNull] object[] paramsUpData, [NotNull]object[] paramsDownData)
        {
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _useClassDataPreparation = false;
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
            _paramsDownData = paramsDownData ?? throw new ArgumentNullException(nameof(paramsDownData));
        }
      
        public UsePreparedDataParamsForAttribute(Type classType, [NotNull] object[] classParamsUpData, [NotNull]object[] classParamsDownData,  string methodName,  [NotNull] object[] paramsUpData, [NotNull]object[] paramsDownData)
        {
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            _useClassDataPreparation = true;
            _classParamsUpData = classParamsUpData ?? throw new ArgumentNullException(nameof(classParamsUpData));
            _classParamsDownData = classParamsDownData ?? throw new ArgumentNullException(nameof(classParamsDownData));
            _paramsUpData = paramsUpData ?? throw new ArgumentNullException(nameof(paramsUpData));
            _paramsDownData = paramsDownData ?? throw new ArgumentNullException(nameof(paramsDownData));
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            TestStore testStore = PreparationTest.CreateTestStore(testInfo);
            
            // Prepare class data for the test from attribute
            var preparedData = GetDataPreparation.GetPreparedDataFromCode(testStore, _useClassDataPreparation, _classType, [_methodName]); 
            if(preparedData.Count == 0) return;
            //add data to store
            testStore.PreparedData.AddDataPreparation(preparedData[0],_paramsUpData,_paramsDownData);
            
            
            // Up data for the test if all data are prepared
            DataPreparationHandler.DataUp(testStore);
            throw new Exception("Test failed");
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public override void AfterTest(ITest test)
        {
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = Store.GetTestStore(testInfo);
            DataPreparationHandler.DataDown(testStore);
            PreparationTest.RemoveTestStore(testStore);
        }

       
        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
        
        private readonly Type _classType;
        private readonly string _methodName;
        private readonly bool _useClassDataPreparation = false;
        private readonly object[] _classParamsUpData;
        private readonly object[] _classParamsDownData;
        private readonly object[] _paramsUpData;
        private readonly object[] _paramsDownData;
    }
}
