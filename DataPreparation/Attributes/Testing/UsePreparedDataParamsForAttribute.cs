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
        
        public UsePreparedDataParamsForAttribute(Type classType, string? methodName,object[] methodMethodParams)
            :this(classType, false, null,null,methodName, methodMethodParams, methodMethodParams)

        {
            
        }
         public UsePreparedDataParamsForAttribute(Type classType, string? methodName,object[] methodMethodParamsUpData,  object[] methodMethodParamsDownData)
             :this(classType, false, null,null,methodName, methodMethodParamsUpData, methodMethodParamsDownData)

        {
            
        }
        
        public UsePreparedDataParamsForAttribute(Type classType,object[] classParams)
            :this(classType, true, classParams, classParams, null, null, null)
        {
           
        }
        public UsePreparedDataParamsForAttribute(Type classType,object[] classParamsUpData,  object[] classParamsDownData)
            :this(classType, true, classParamsUpData, classParamsDownData, null, null, null)
        {
           
        }

        public UsePreparedDataParamsForAttribute(Type classType, object[] classParams,  string? methodName,  object[] methodMethodParams)
            :this( classType,true,  classParams,  classParams,   methodName, methodMethodParams,  methodMethodParams)
        {
            _methodName = methodName?? throw new ArgumentNullException(nameof(methodName));;
        }
        public UsePreparedDataParamsForAttribute(Type classType, object[] classParamsUpData, object[] classParamsDownData,  string? methodName,  object[] methodMethodParamsUpData, object[] methodMethodParamsDownData)
            :this( classType,true,  classParamsUpData,  classParamsDownData,   methodName, methodMethodParamsUpData,  methodMethodParamsDownData)
        {
            _methodName = methodName?? throw new ArgumentNullException(nameof(methodName));;
        }
        
        private UsePreparedDataParamsForAttribute(Type classType, bool useClassDataPreparation, object[]? classParamsUpData, object[]? classParamsDownData,  string? methodName,  object[]? methodMethodParamsUpData, object[]? methodMethodParamsDownData)
        {
            _classType = classType ?? throw new ArgumentNullException(nameof(classType));
            _methodName = methodName;
            _useClassDataPreparation = useClassDataPreparation;
            _classClassParamsUpData = classParamsUpData;
            _classParamsDownData = classParamsDownData;
            _methodParamsUpData = methodMethodParamsUpData;
            _methodParamsDownData = methodMethodParamsDownData;
        }

        /// <summary>
        /// Method to be called before the test is executed.
        /// </summary>
        /// <param name="test">The test that is going to be executed.</param>
        public override void BeforeTest(ITest test)
        {
            
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            TestStore testStore = TestStore.Initialize(testInfo);
            
            // Prepare class data for the test from attribute
            var preparedData = GetDataPreparation.GetPreparedDataFromCode(testStore, _useClassDataPreparation, _classType, _methodName);
            var (paramsUpData, paramsDown) = GetDataPreparation.FilterParams(_useClassDataPreparation, _methodName, _classClassParamsUpData, _methodParamsUpData,
                _classParamsDownData, _methodParamsDownData);
            if(preparedData.Count == 0) return;
            //add data to store
            testStore.PreparedData.AddDataPreparationList(preparedData,paramsUpData,paramsDown);
            
            // Up data for the test if all data are prepared
            DataPreparationHandler.DataUp(testStore);
        }

        /// <summary>
        /// Method to be called after the test is executed.
        /// </summary>
        /// <param name="test">The test that has been executed.</param>
        public override void AfterTest(ITest test)
        {
            TestInfo testInfo = TestInfo.CreateTestInfo(test);
            var testStore = TestStore.Get(testInfo);
         
            TestStore.Deinitialize(testStore);
        }

       
        /// <summary>
        /// Gets the targets for the action.
        /// </summary>
        public override ActionTargets Targets => ActionTargets.Test;
        
        private readonly Type _classType;
        private readonly string? _methodName;
        private readonly bool _useClassDataPreparation = false;
        private readonly object[]? _classClassParamsUpData;
        private readonly object[]? _classParamsDownData;
        private readonly object[]? _methodParamsUpData;
        private readonly object[]? _methodParamsDownData;
    }
}
